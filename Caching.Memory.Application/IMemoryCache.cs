using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Caching.Memory.Application
{
    public interface IMemoryCache
    {
        bool TryGetValue(object key, out object value);

        ICacheEntry CreateEntry(object key);

        void Remove(object key);
    }

    public class MemoryCache : IMemoryCache
    {
        private readonly Action<CacheEntry> _setEntry;
        private readonly Action<CacheEntry> _entryExpiration;
        private readonly MemoryCacheOptions _options;
        private readonly ConcurrentDictionary<object, CacheEntry> _entries;

        public MemoryCache(IOptions<MemoryCacheOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }
            _setEntry = SetEntry;
            _entryExpiration = EntryExpired;
            _options = optionsAccessor.Value;
            _entries = new ConcurrentDictionary<object, CacheEntry>();
        }
      

        public ICacheEntry CreateEntry(object key)
        {
            return new CacheEntry(
                key,
                _setEntry,
                _entryExpiration
            );
        }

        /// <summary>
        /// 创建缓存实体
        /// </summary>
        /// <param name="entry"></param>
        private void SetEntry(CacheEntry entry)
        {
            if (_options.SizeLimit.HasValue && !entry.Size.HasValue)
            {
                throw new InvalidOperationException($"Cache entry must specify a value for {nameof(entry.Size)} when {nameof(_options.SizeLimit)} is set.");
            }

            var utcNow = _options.Clock.UtcNow;

            DateTimeOffset? absoluteExpiration = null;
            if (entry._absoluteExpirationRelativeToNow.HasValue)
            {
                absoluteExpiration = utcNow + entry._absoluteExpirationRelativeToNow;
            }
            else if (entry._absoluteExpiration.HasValue)
            {
                absoluteExpiration = entry._absoluteExpiration;
            }

            if (absoluteExpiration.HasValue)
            {
                if (!entry._absoluteExpiration.HasValue || absoluteExpiration.Value < entry._absoluteExpiration.Value)
                {
                    entry._absoluteExpiration = absoluteExpiration;
                }
            }

            // Initialize the last access timestamp at the time the entry is added
            entry.LastAccessed = utcNow;

            if (_entries.TryGetValue(entry.Key, out CacheEntry priorEntry))
            {
                priorEntry.SetExpired(EvictionReason.Replaced);
            }

            var exceedsCapacity = UpdateCacheSizeExceedsCapacity(entry);
            if (!entry.CheckExpired(utcNow) && !exceedsCapacity)
            {
                var entryAdded = false;

                if (priorEntry == null)
                {
                    // Try to add the new entry if no previous entries exist.
                    entryAdded = _entries.TryAdd(entry.Key, entry);
                }
                else
                {
                    // Try to update with the new entry if a previous entries exist.
                    entryAdded = _entries.TryUpdate(entry.Key, entry, priorEntry);

                    if (entryAdded)
                    {
                        if (_options.SizeLimit.HasValue)
                        {
                            // The prior entry was removed, decrease the by the prior entry's size
                            Interlocked.Add(ref _cacheSize, -priorEntry.Size.Value);
                        }
                    }
                    else
                    {
                        // The update will fail if the previous entry was removed after retrival.
                        // Adding the new entry will succeed only if no entry has been added since.
                        // This guarantees removing an old entry does not prevent adding a new entry.
                        entryAdded = _entries.TryAdd(entry.Key, entry);
                    }
                }

                if (entryAdded)
                {
                    entry.AttachTokens();
                }
                else
                {
                    if (_options.SizeLimit.HasValue)
                    {
                        // Entry could not be added, reset cache size
                        Interlocked.Add(ref _cacheSize, -entry.Size.Value);
                    }
                    entry.SetExpired(EvictionReason.Replaced);
                    entry.InvokeEvictionCallbacks();
                }

                if (priorEntry != null)
                {
                    priorEntry.InvokeEvictionCallbacks();
                }
            }

        }

        private long _cacheSize = 0;
        private bool UpdateCacheSizeExceedsCapacity(CacheEntry entry)
        {
            if (!_options.SizeLimit.HasValue)
            {
                return false;
            }

            var newSize = 0L;
            for (var i = 0; i < 100; i++)
            {
                var sizeRead = Interlocked.Read(ref _cacheSize);
                newSize = sizeRead + entry.Size.Value;

                if (newSize < 0 || newSize > _options.SizeLimit)
                {
                    // Overflow occured, return true without updating the cache size
                    return true;
                }

                if (sizeRead == Interlocked.CompareExchange(ref _cacheSize, newSize, sizeRead))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 实体过期方法
        /// </summary>
        /// <param name="entry"></param>
        private void EntryExpired(CacheEntry entry) 
        { 
        
        }

        public void Remove(object key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(object key, out object value)
        {
            throw new NotImplementedException();
        }
    }
}
