using System;
using System.Linq;

namespace Ncfe.CodeTest.Failover
{
    /// <summary>
    /// The system is in failover mode when failover is enabled and the number of failed
    /// requests within the configured window exceeds the configured threshold.
    /// </summary>
    public class FailoverModeEvaluator : IFailoverModeEvaluator
    {
        private readonly IFailoverRepository _failoverRepository;
        private readonly IFailoverSettings _settings;
        private readonly IClock _clock;

        public FailoverModeEvaluator(IFailoverRepository failoverRepository, IFailoverSettings settings, IClock clock)
        {
            _failoverRepository = failoverRepository ?? throw new ArgumentNullException(nameof(failoverRepository));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public bool IsInFailoverMode()
        {
            // Check the cheap config flag first to avoid a needless database call
            if (!_settings.IsFailoverModeEnabled)
            {
                return false;
            }

            var windowStart = _clock.Now - _settings.FailedRequestWindow;
            var failoverEntries = _failoverRepository.GetFailOverEntries();

            var failedRequests = failoverEntries?.Count(entry => entry.DateTime > windowStart) ?? 0;

            return failedRequests > _settings.FailedRequestThreshold;
        }
    }
}
