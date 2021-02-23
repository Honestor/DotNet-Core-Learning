using System;
using System.Collections.Generic;
using System.Text;

namespace Caching.Memory.Application
{
    public class CacheEntryHelper
    {
        internal static IDisposable EnterScope(CacheEntry entry)
        {
            var scopes = GetOrCreateScopes();

            var scopeLease = new ScopeLease(scopes);
            Scopes = scopes.Push(entry);

            return scopeLease;
        }

        private static CacheEntryStack GetOrCreateScopes()
        {
            var scopes = Scopes;
            if (scopes == null)
            {
                scopes = CacheEntryStack.Empty;
                Scopes = scopes;
            }

            return scopes;
        }

        private sealed class ScopeLease : IDisposable
        {
            readonly CacheEntryStack _cacheEntryStack;

            public ScopeLease(CacheEntryStack cacheEntryStack)
            {
                _cacheEntryStack = cacheEntryStack;
            }

            public void Dispose()
            {
                Scopes = _cacheEntryStack;
            }
        }
    }
}
