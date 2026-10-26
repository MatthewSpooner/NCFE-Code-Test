using System;
using Ncfe.CodeTest.Failover;

namespace Ncfe.CodeTest.DataStores
{
    /// <summary>
    /// Routes requests to the failover store when the system is in failover mode,
    /// otherwise to the main store. Consumers just see a single <see cref="ILearnerDataStore"/>.
    /// </summary>
    public class FailoverAwareLearnerDataStore : ILearnerDataStore
    {
        private readonly ILearnerDataStore _mainStore;
        private readonly ILearnerDataStore _failoverStore;
        private readonly IFailoverModeEvaluator _failoverModeEvaluator;

        public FailoverAwareLearnerDataStore(
            ILearnerDataStore mainStore,
            ILearnerDataStore failoverStore,
            IFailoverModeEvaluator failoverModeEvaluator)
        {
            _mainStore = mainStore ?? throw new ArgumentNullException(nameof(mainStore));
            _failoverStore = failoverStore ?? throw new ArgumentNullException(nameof(failoverStore));
            _failoverModeEvaluator = failoverModeEvaluator ?? throw new ArgumentNullException(nameof(failoverModeEvaluator));
        }

        public LearnerResponse GetLearner(int learnerId)
        {
            var store = _failoverModeEvaluator.IsInFailoverMode() ? _failoverStore : _mainStore;

            return store.GetLearner(learnerId);
        }
    }
}
