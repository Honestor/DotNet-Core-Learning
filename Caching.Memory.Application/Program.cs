using System;

namespace Caching.Memory.Application
{
    class Program
    {
        static void Main(string[] args)
        {
            var cache = new MemoryCache();
            var entry = cache.CreateEntry("111");
            entry.Value = "222";
            entry.Dispose();
        }
    }
}
