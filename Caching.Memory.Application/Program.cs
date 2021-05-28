using System;

namespace Caching.Memory.Application
{
    class Program
    {
        static void Main(string[] args)
        {
            //var cache = new MemoryCache(new MemoryCacheOptions()
            //{
            //    Clock =new SystemClock(),
            //    SizeLimit=10L
            //});
            //var entry = cache.CreateEntry("111");
            //entry.Value = "222";
            //entry.Size = 5L;
            //entry.Dispose();
        }

        static int f(int n)
        {
            if (n <= 2)
                return n;
            return 0;
        }
    }
}
