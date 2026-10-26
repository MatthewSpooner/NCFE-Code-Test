namespace Ncfe.CodeTest.DataStores
{
    /// <summary>
    /// Adapter over the static <see cref="FailoverLearnerDataAccess"/>, whose signature must not change.
    /// Wrapping the static call lets consumers depend on <see cref="ILearnerDataStore"/> and be unit tested.
    /// </summary>
    public class FailoverLearnerDataStore : ILearnerDataStore
    {
        public LearnerResponse GetLearner(int learnerId)
        {
            return FailoverLearnerDataAccess.GetLearnerById(learnerId);
        }
    }
}
