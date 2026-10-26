using System;

namespace Ncfe.CodeTest.Failover
{
    /// <summary>
    /// Abstraction over the system clock so time-based logic can be tested deterministically.
    /// </summary>
    public interface IClock
    {
        DateTime Now { get; }
    }
}
