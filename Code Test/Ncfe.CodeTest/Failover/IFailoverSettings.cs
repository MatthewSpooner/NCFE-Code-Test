using System;

namespace Ncfe.CodeTest.Failover
{
    public interface IFailoverSettings
    {
        /// <summary>Whether failover mode is allowed at all.</summary>
        bool IsFailoverModeEnabled { get; }

        /// <summary>Failover mode is entered when failed requests exceed this number.</summary>
        int FailedRequestThreshold { get; }

        /// <summary>The time period in which failed requests are counted.</summary>
        TimeSpan FailedRequestWindow { get; }
    }
}
