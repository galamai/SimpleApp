using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Newtonsoft.Json;
using SimpleApp.Domain;
using SimpleApp.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.AzureStorage.Storage
{
    public class StateStore : IStateStore
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
#if DEBUG
            Formatting = Formatting.Indented
#endif
        };

        private readonly StateStoreOptions _options;
        private readonly CloudBlobContainer _container;

        public StateStore(IOptions<StateStoreOptions> optionsAccessor)
        {
            _options = optionsAccessor.Value;
            var account = CloudStorageAccount.Parse(_options.ConnectionString);
            var client = account.CreateCloudBlobClient();
            _container = client.GetContainerReference(_options.ContainerName);
        }

        public async Task<TState> FindByIdAsync<TState>(string key, CancellationToken cancellationToken = default(CancellationToken)) where TState : class
        {
            var blob = _container.GetBlockBlobReference(key);
            try
            {
                var payload = await blob
                    .DownloadTextAsync(Encoding.UTF8, AccessCondition.GenerateEmptyCondition(), null, null, cancellationToken)
                    ;

                var state = JsonConvert.DeserializeObject<TState>(payload, SerializerSettings);
                if (state is IHasETag hasETag)
                {
                    hasETag.ETag = blob.Properties.ETag;
                }
                return state;
            }
            catch(StorageException ex) when (
                ex.IsNotFoundContainerNotFound() ||
                ex.IsNotFoundBlobNotFound())
            {
                return null;
            }
            catch(SerializationException)
            {
                return null;
            }
        }

        public async Task SaveAsync<TState>(string key, TState state)
        {
            var blob = _container.GetBlockBlobReference(key);
            var json = JsonConvert.SerializeObject(state, SerializerSettings);

            if (state is IHasETag hasETag)
            {
                try
                {
                    try
                    {
                        hasETag.ETag = await UploadTextAsync(blob, json, hasETag.ETag);
                    }
                    catch(StorageException ex) when (ex.IsNotFoundContainerNotFound())
                    {
                        await _container.CreateIfNotExistsAsync();
                        hasETag.ETag = await UploadTextAsync(blob, json, hasETag.ETag);
                    }

                }
                catch(StorageException ex) when (ex.RequestInformation?.HttpStatusCode == 412)
                {
                    throw new ConcurrencyException($"State `{key}` of type `{state.GetType().Name}` modified.", ex);
                }
            }
            else
            {
                try
                {
                    await blob.UploadTextAsync(json);
                }
                catch (StorageException ex) when (ex.IsNotFoundContainerNotFound())
                {
                    await _container.CreateIfNotExistsAsync();
                    await blob.UploadTextAsync(json);
                }
            }
        }

        private async Task<string> UploadTextAsync(CloudBlockBlob blob, string payload, string eTag)
        {
            await blob.UploadTextAsync(payload, AccessCondition.GenerateIfMatchCondition(eTag), null, null);
            return blob.Properties.ETag;
        }
    }
}
