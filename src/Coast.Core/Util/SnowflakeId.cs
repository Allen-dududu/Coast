﻿namespace Coast.Core
{
    using System;
    using System.Net.NetworkInformation;
    using System.Threading;
    using Coast.Core.Util;

    public class SnowflakeId
    {
        /// <summary>
        /// Start time 2022-03-28 21:07:48.
        /// </summary>
        public const long Twepoch = 1648472868000L;

        /// <summary>
        /// The number of bits occupied by workerId.
        /// </summary>
        private const int WorkerIdBits = 10;

        /// <summary>
        /// The number of bits occupied by timestamp.
        /// </summary>
        private const int TimestampBits = 41;

        /// <summary>
        /// The number of bits occupied by sequence.
        /// </summary>
        private const int SequenceBits = 12;

        /// <summary>
        /// Maximum supported machine id, the result is 1023.
        /// </summary>
        private const int MaxWorkerId = ~(-1 << WorkerIdBits);

        /// <summary>
        /// mask that help to extract timestamp and sequence from a long.
        /// </summary>
        private const long TimestampAndSequenceMask = ~(-1L << (TimestampBits + SequenceBits));

        /// <summary>
        /// business meaning: machine ID (0 ~ 1023)
        /// actual layout in memory:
        /// highest 1 bit: 0
        /// middle 10 bit: workerId
        /// lowest 53 bit: all 0.
        /// </summary>
        private long _workerId { get; set; }

        /// <summary>
        /// timestamp and sequence mix in one Long
        /// highest 11 bit: not used
        /// middle  41 bit: timestamp
        /// lowest  12 bit: sequence.
        /// </summary>
        private long _timestampAndSequence;

        private static SnowflakeId? _snowflakeId;

        private static readonly object SLock = new object();

        private readonly object _lock = new object();

        public SnowflakeId(long workerId)
        {
            InitTimestampAndSequence();
            // sanity check for workerId
            if (workerId > MaxWorkerId || workerId < 0)
                throw new ArgumentException($"worker Id can't be greater than {MaxWorkerId} or less than 0");

            _workerId = workerId << (TimestampBits + SequenceBits);
        }

        public static SnowflakeId Default()
        {
            if (_snowflakeId != null)
            {
                return _snowflakeId;
            }

            lock (SLock)
            {
                if (_snowflakeId != null)
                {
                    return _snowflakeId;
                }

                if (!long.TryParse(CoastConstant.WorkerId, out var workerId))
                {
                    workerId = IdUtil.GenerateWorkerId(MaxWorkerId);
                }

                return _snowflakeId = new SnowflakeId(workerId);
            }
        }

        public virtual long NextId()
        {
            lock (_lock)
            {
                WaitIfNecessary();
                long timestampWithSequence = _timestampAndSequence & TimestampAndSequenceMask;
                return _workerId | timestampWithSequence;
            }
        }

        /// <summary>
        /// init first timestamp and sequence immediately.
        /// </summary>
        private void InitTimestampAndSequence()
        {
            long timestamp = GetNewestTimestamp();
            long timestampWithSequence = timestamp << SequenceBits;
            _timestampAndSequence = timestampWithSequence;
        }

        /// <summary>
        /// block current thread if the QPS of acquiring UUID is too high
        /// that current sequence space is exhausted.
        /// </summary>
        private void WaitIfNecessary()
        {
            long currentWithSequence = ++_timestampAndSequence;
            long current = currentWithSequence >> SequenceBits;
            long newest = GetNewestTimestamp();

            if (current >= newest)
            {
                Thread.Sleep(5);
            }
        }

        /// <summary>
        /// get newest timestamp relative to twepoch.
        /// </summary>
        /// <returns></returns>
        private long GetNewestTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - Twepoch;
        }
    }

    internal static class IdUtil
    {
        /// <summary>
        /// auto generate workerId, try using mac first, if failed, then randomly generate one
        /// </summary>
        /// <returns>workerId</returns>
        public static long GenerateWorkerId(int maxWorkerId)
        {
            try
            {
                return GenerateWorkerIdBaseOnMac();
            }
            catch
            {
                return GenerateRandomWorkerId(maxWorkerId);
            }
        }

        /// <summary>
        /// use lowest 10 bit of available MAC as workerId.
        /// </summary>
        /// <returns>workerId</returns>
        private static long GenerateWorkerIdBaseOnMac()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            if (nics == null || nics.Length < 1)
            {
                throw new Exception("no available mac found");
            }

            var adapter = nics[0];
            PhysicalAddress address = adapter.GetPhysicalAddress();
            byte[] mac = address.GetAddressBytes();

            return ((mac[4] & 0B11) << 8) | (mac[5] & 0xFF);
        }

        /// <summary>
        /// randomly generate one as workerId.
        /// </summary>
        /// <returns>random Id less than maxWorkerId.</returns>
        private static long GenerateRandomWorkerId(int maxWorkerId)
        {
            return new Random().Next(maxWorkerId + 1);
        }
    }
}
