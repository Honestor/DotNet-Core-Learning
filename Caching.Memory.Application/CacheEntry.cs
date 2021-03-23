using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caching.Memory.Application
{
    public interface ICacheEntry : IDisposable
    {
        /// <summary>
        /// 缓存的键
        /// </summary>
        object Key { get; }

        /// <summary>
        /// 缓存的值
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// 缓存实体的大小
        /// </summary>
        long? Size { get; set; }
    }

    public class CacheEntry:ICacheEntry
    {
        public object Key { get; private set; }

        public object Value { get; set; }

        /// <summary>
        /// 缓存实体大小
        /// </summary>
        private long? _size;
        public long? Size
        {
            get => _size;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(value)} must be non-negative.");
                }

                _size = value;
            }
        }

        private readonly Action<CacheEntry> _notifyCacheOfExpiration;

        private readonly Action<CacheEntry> _notifyCacheEntryDisposed;

        private readonly Action<CacheEntry> _setEntry;

        private readonly Action<CacheEntry> _entryExpirationNotification;

        internal DateTimeOffset? _absoluteExpiration;

        /// <summary>
        /// 滑动过期时间
        /// </summary>
        private TimeSpan? _slidingExpiration;
        public TimeSpan? SlidingExpiration
        {
            get
            {
                return _slidingExpiration;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(SlidingExpiration),
                        value,
                        "The sliding expiration value must be positive.");
                }
                _slidingExpiration = value;
            }
        }

        internal IList<IChangeToken> _expirationTokens;
        public IList<IChangeToken> ExpirationTokens
        {
            get
            {
                if (_expirationTokens == null)
                {
                    _expirationTokens = new List<IChangeToken>();
                }

                return _expirationTokens;
            }
        }

        internal TimeSpan? _absoluteExpirationRelativeToNow;

        private IDisposable _scope;

        internal EvictionReason EvictionReason { get; private set; }

        /// <summary>
        /// 最后一次处理时间
        /// </summary>
        internal DateTimeOffset LastAccessed { get; set; }

        private IList<IDisposable> _expirationTokenRegistrations;

        internal CacheEntry(
            object key,
            Action<CacheEntry> notifyCacheEntryDisposed,
            Action<CacheEntry> notifyCacheOfExpiration)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (notifyCacheEntryDisposed == null)
            {
                throw new ArgumentNullException(nameof(notifyCacheEntryDisposed));
            }

            if (notifyCacheOfExpiration == null)
            {
                throw new ArgumentNullException(nameof(notifyCacheOfExpiration));
            }

            Key = key;
            _notifyCacheEntryDisposed = notifyCacheEntryDisposed;
            _notifyCacheOfExpiration = notifyCacheOfExpiration;

            _scope = CacheEntryHelper.EnterScope(this);
        }

        public ICacheEntry CreateEntry(object key)
        {
            return new CacheEntry(
                key,
                _setEntry,
                _entryExpirationNotification
            );
        }

        private bool _isExpired;
        internal void SetExpired(EvictionReason reason)
        {
            if (EvictionReason == EvictionReason.None)
            {
                EvictionReason = reason;
            }
            _isExpired = true;
            DetachTokens();
        }

        internal readonly object _lock = new object();
        private void DetachTokens()
        {
            lock (_lock)
            {
                var registrations = _expirationTokenRegistrations;
                if (registrations != null)
                {
                    _expirationTokenRegistrations = null;
                    for (int i = 0; i < registrations.Count; i++)
                    {
                        var registration = registrations[i];
                        registration.Dispose();
                    }
                }
            }
        }

        private bool _added = false;
        public void Dispose()
        {
            if (!_added)
            {
                _added = true;
                _scope.Dispose();
                _notifyCacheEntryDisposed(this);
                //PropagateOptions(CacheEntryHelper.Current);
            }
        }

        internal bool CheckExpired(DateTimeOffset now)
        {
            return _isExpired || CheckForExpiredTime(now) || CheckForExpiredTokens();
        }

        private bool CheckForExpiredTime(DateTimeOffset now)
        {
            //绝对过期
            if (_absoluteExpiration.HasValue && _absoluteExpiration.Value <= now)
            {
                SetExpired(EvictionReason.Expired);
                return true;
            }

            //滑动过期
            if (_slidingExpiration.HasValue && (now - LastAccessed) >= _slidingExpiration)
            {
                SetExpired(EvictionReason.Expired);
                return true;
            }

            return false;
        }

        internal bool CheckForExpiredTokens()
        {
            if (_expirationTokens != null)
            {
                for (int i = 0; i < _expirationTokens.Count; i++)
                {
                    var expiredToken = _expirationTokens[i];
                    if (expiredToken.HasChanged)
                    {
                        SetExpired(EvictionReason.TokenExpired);
                        return true;
                    }
                }
            }
            return false;
        }

        internal void AttachTokens()
        {
            if (_expirationTokens != null)
            {
                lock (_lock)
                {
                    for (int i = 0; i < _expirationTokens.Count; i++)
                    {
                        IChangeToken expirationToken = _expirationTokens[i];
                        if (expirationToken.ActiveChangeCallbacks)
                        {
                            if (_expirationTokenRegistrations == null)
                            {
                                _expirationTokenRegistrations = new List<IDisposable>(1);
                            }
                            IDisposable registration = expirationToken.RegisterChangeCallback(ExpirationCallback, this);
                            _expirationTokenRegistrations.Add(registration);
                        }
                    }
                }
            }
        }
    }

    public enum EvictionReason
    {
        None,

        /// <summary>
        /// Manually
        /// </summary>
        Removed,

        /// <summary>
        /// Overwritten
        /// </summary>
        Replaced,

        /// <summary>
        /// Timed out
        /// </summary>
        Expired,

        /// <summary>
        /// Event
        /// </summary>
        TokenExpired,

        /// <summary>
        /// Overflow
        /// </summary>
        Capacity,
    }
}
