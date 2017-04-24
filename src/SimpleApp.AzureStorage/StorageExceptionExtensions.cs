using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.WindowsAzure.Storage
{
    public static class StorageExceptionExtensions
    {
        public static bool IsServerSideError(this StorageException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return exception.RequestInformation?.HttpStatusCode >= 500 &&
                exception.RequestInformation.HttpStatusCode < 600;
        }

        public static bool IsConflict(this StorageException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return exception.RequestInformation?.HttpStatusCode == 409;
        }

        public static bool IsNotFound(this StorageException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return exception.RequestInformation?.HttpStatusCode == 404;
        }

        public static bool IsBadRequestPopReceiptMismatch(this StorageException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return exception.RequestInformation?.HttpStatusCode == 400 &&
                exception.RequestInformation.ExtendedErrorInformation?.ErrorCode == "PopReceiptMismatch";
        }

        public static bool IsNotFoundQueueNotFound(this StorageException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return exception.RequestInformation?.HttpStatusCode == 404 &&
                exception.RequestInformation.ExtendedErrorInformation?.ErrorCode == "QueueNotFound";
        }

        public static bool IsNotFoundMessageOrQueueNotFound(this StorageException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return exception.RequestInformation?.HttpStatusCode == 404 &&
                (exception.RequestInformation.ExtendedErrorInformation?.ErrorCode == "MessageNotFound" ||
                exception.RequestInformation.ExtendedErrorInformation?.ErrorCode == "QueueNotFound");
        }

        public static bool IsConflictQueueBeingDeletedOrDisabled(this StorageException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return exception.RequestInformation?.HttpStatusCode == 409 &&
                exception.RequestInformation.ExtendedErrorInformation?.ErrorCode == "QueueBeingDeleted";
        }

        public static bool IsNotFoundContainerNotFound(this StorageException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return exception.RequestInformation?.HttpStatusCode == 404 &&
                exception.RequestInformation.ExtendedErrorInformation?.ErrorCode == "ContainerNotFound";
        }

        public static bool IsNotFoundBlobNotFound(this StorageException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return exception.RequestInformation?.HttpStatusCode == 404 &&
                exception.RequestInformation.ExtendedErrorInformation?.ErrorCode == "BlobNotFound";
        }

        public static bool IsNotFoundTableNotFound(this StorageException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return exception.RequestInformation?.HttpStatusCode == 404 &&
                exception.RequestInformation.ExtendedErrorInformation?.ErrorCode == "TableNotFound";
        }

        public static bool IsPreconditionFailed(this StorageException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return exception.RequestInformation?.HttpStatusCode == 412;
        }
    }
}
