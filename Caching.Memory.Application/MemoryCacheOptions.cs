using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caching.Memory.Application
{
    public class MemoryCacheOptions : IOptions<MemoryCacheOptions>
    {
        /// <summary>
        /// 缓存实体大小限制
        /// </summary>
        private long? _sizeLimit;
        public long? SizeLimit
        {
            get => _sizeLimit;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(value)} must be non-negative.");
                }

                _sizeLimit = value;
            }
        }

        /// <summary>
        /// 时钟
        /// </summary>
        public ISystemClock Clock { get; set; }

        MemoryCacheOptions IOptions<MemoryCacheOptions>.Value
        {
            get { return this; }
        }
    }
}
