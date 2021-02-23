using System;
using System.Collections.Generic;
using System.Text;

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
        public ICacheEntry CreateEntry(object key)
        {
            throw new NotImplementedException();
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
