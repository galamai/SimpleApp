﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.AzureStorage.Timers
{
    internal class RandomizedDelayStrategy : IDelayStrategy
    {
        public const double RandomizationFactor = 0.2;

        private readonly TimeSpan _minimumInterval;
        private readonly TimeSpan _maximumInterval;
        private readonly TimeSpan _deltaBackoff;

        private TimeSpan _currentInterval;
        private uint _backoffExponent;
        private Random _random;

        public RandomizedDelayStrategy(TimeSpan minimumInterval, TimeSpan maximumInterval)
            : this(minimumInterval, maximumInterval, minimumInterval)
        {
        }

        public RandomizedDelayStrategy(TimeSpan minimumInterval, TimeSpan maximumInterval,
            TimeSpan deltaBackoff)
        {
            if (minimumInterval.Ticks < 0)
                throw new ArgumentOutOfRangeException(nameof(minimumInterval), "The TimeSpan must not be negative.");

            if (maximumInterval.Ticks < 0)
                throw new ArgumentOutOfRangeException(nameof(maximumInterval), "The TimeSpan must not be negative.");

            if (minimumInterval.Ticks > maximumInterval.Ticks)
                throw new ArgumentException("The minimumInterval must not be greater than the maximumInterval.",
                        nameof(minimumInterval));

            _minimumInterval = minimumInterval;
            _maximumInterval = maximumInterval;
            _deltaBackoff = deltaBackoff;
        }

        public TimeSpan GetNextDelay(bool executionSucceeded)
        {
            if (executionSucceeded)
            {
                _currentInterval = _minimumInterval;
                _backoffExponent = 1;
            }
            else if (_currentInterval != _maximumInterval)
            {
                TimeSpan backoffInterval = _minimumInterval;

                if (_backoffExponent > 0)
                {
                    if (_random == null)
                        _random = new Random();

                    double incrementMsec = _random.Next(1.0 - RandomizationFactor, 1.0 + RandomizationFactor) *
                        Math.Pow(2.0, _backoffExponent - 1) *
                        _deltaBackoff.TotalMilliseconds;
                    backoffInterval += TimeSpan.FromMilliseconds(incrementMsec);
                }

                if (backoffInterval < _maximumInterval)
                {
                    _currentInterval = backoffInterval;
                    _backoffExponent++;
                }
                else
                {
                    _currentInterval = _maximumInterval;
                }
            }

            return _currentInterval;
        }
    }
}
