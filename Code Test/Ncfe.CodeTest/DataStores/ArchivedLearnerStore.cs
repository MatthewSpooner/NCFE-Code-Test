using System;

namespace Ncfe.CodeTest.DataStores
{
    /// <summary>
    /// Adapter over <see cref="ArchivedDataService"/>, whose signature must not change.
    /// </summary>
    public class ArchivedLearnerStore : IArchivedLearnerStore
    {
        private readonly ArchivedDataService _archivedDataService;

        public ArchivedLearnerStore(ArchivedDataService archivedDataService)
        {
            _archivedDataService = archivedDataService ?? throw new ArgumentNullException(nameof(archivedDataService));
        }

        public Learner GetArchivedLearner(int learnerId)
        {
            return _archivedDataService.GetArchivedLearner(learnerId);
        }
    }
}
