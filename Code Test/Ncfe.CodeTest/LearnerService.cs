using System;
using Ncfe.CodeTest.DataStores;
using Ncfe.CodeTest.Failover;

namespace Ncfe.CodeTest
{
    public class LearnerService
    {
        private readonly IArchivedLearnerStore _archivedLearnerStore;
        private readonly ILearnerDataStore _learnerDataStore;

        /// <summary>
        /// Wires up the default production dependencies so existing callers that use
        /// <c>new LearnerService()</c> keep working. Prefer the injecting constructor
        /// (registered in a DI container) for new code.
        /// </summary>
        public LearnerService()
            : this(
                new ArchivedLearnerStore(new ArchivedDataService()),
                new FailoverAwareLearnerDataStore(
                    new MainLearnerDataStore(new LearnerDataAccess()),
                    new FailoverLearnerDataStore(),
                    new FailoverModeEvaluator(new FailoverRepository(), new AppSettingsFailoverSettings(), new SystemClock())))
        {
        }

        public LearnerService(IArchivedLearnerStore archivedLearnerStore, ILearnerDataStore learnerDataStore)
        {
            _archivedLearnerStore = archivedLearnerStore ?? throw new ArgumentNullException(nameof(archivedLearnerStore));
            _learnerDataStore = learnerDataStore ?? throw new ArgumentNullException(nameof(learnerDataStore));
        }

        public Learner GetLearner(int learnerId, bool isLearnerArchived)
        {
            if (isLearnerArchived)
            {
                return _archivedLearnerStore.GetArchivedLearner(learnerId);
            }

            var learnerResponse = _learnerDataStore.GetLearner(learnerId);

            if (learnerResponse == null)
            {
                return null;
            }

            // The live/failover store may report that the learner has since been archived
            return learnerResponse.IsArchived
                ? _archivedLearnerStore.GetArchivedLearner(learnerId)
                : learnerResponse.Learner;
        }
    }
}
