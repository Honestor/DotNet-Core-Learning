using System;
using System.Collections.Generic;
using System.Text;

namespace Caching.Memory.Application
{
    public interface ISystemClock
    {
        DateTimeOffset UtcNow { get; }
    }

    public class SystemClock : ISystemClock
    {
        public DateTimeOffset UtcNow
        {
            get
            {
                return DateTimeOffset.UtcNow;
            }
        }
    }
}
