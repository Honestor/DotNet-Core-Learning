using System;
using System.Collections.Generic;
using System.Text;

namespace Caching.Memory.Application
{
    public interface ICacheEntry : IDisposable
    {
        object Key { get; }

        object Value { get; set; }
    }

    public class CacheEntry:ICacheEntry
    {
        public object Key { get; private set; }

        public object Value { get; set; }

        private readonly Action<CacheEntry> _notifyCacheOfExpiration;

        private readonly Action<CacheEntry> _notifyCacheEntryDisposed;

        private readonly Action<CacheEntry> _setEntry;

        private readonly Action<CacheEntry> _entryExpirationNotification;

        private IDisposable _scope;

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
    }
}
