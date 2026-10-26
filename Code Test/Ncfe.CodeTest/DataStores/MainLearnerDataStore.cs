using System;

namespace Ncfe.CodeTest.DataStores
{
    /// <summary>
    /// Adapter over the 3rd party <see cref="LearnerDataAccess"/>, whose signature must not change.
    /// </summary>
    public class MainLearnerDataStore : ILearnerDataStore
    {
        private readonly LearnerDataAccess _learnerDataAccess;

        public MainLearnerDataStore(LearnerDataAccess learnerDataAccess)
        {
            _learnerDataAccess = learnerDataAccess ?? throw new ArgumentNullException(nameof(learnerDataAccess));
        }

        public LearnerResponse GetLearner(int learnerId)
        {
            return _learnerDataAccess.LoadLearner(learnerId);
        }
    }
}
