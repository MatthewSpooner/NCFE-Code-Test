using System;

namespace Ncfe.CodeTest.Failover
{
    public class SystemClock : IClock
    {
        public DateTime Now => DateTime.Now;
    }
}
