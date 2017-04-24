using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.AzureStorage.Messaging
{
    public class MessageReceiverOptions
    {
        private const int DefaultBatchSize = 16;
        private const int MaxBatchSize = 32;

        private int _batchSize = DefaultBatchSize;
        private int _newBatchThreshold = -1;
        private TimeSpan _maxPollingInterval = QueuePollingIntervals.DefaultMaximum;
        private string _queue = "messeges";
        private string _poisonQueue = "poison-messages";

        public string ConnectionString { get; set; } = "UseDevelopmentStorage=true";
        public string Queue
        {
            get => _queue;
            set
            {
                NameValidator.ValidateQueueName(value);
                _queue = value;
            }
        }

        public string PoisonQueue
        {
            get => _poisonQueue;
            set
            {
                NameValidator.ValidateQueueName(value);
                _poisonQueue = value;
            }
        }

        public int BatchSize
        {
            get => _batchSize;

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (value > MaxBatchSize)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _batchSize = value;
            }
        }

        public int NewBatchThreshold
        {
            get
            {
                if (_newBatchThreshold == -1)
                {
                    return _batchSize / 2;
                }
                return _newBatchThreshold;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _newBatchThreshold = value;
            }
        }

        public TimeSpan MaxPollingInterval
        {
            get { return _maxPollingInterval; }

            set
            {
                if (value < QueuePollingIntervals.Minimum)
                {
                    throw new ArgumentException($"MaxPollingInterval must not be less than {QueuePollingIntervals.Minimum}.", nameof(value));
                }

                _maxPollingInterval = value;
            }
        }
    }
}
