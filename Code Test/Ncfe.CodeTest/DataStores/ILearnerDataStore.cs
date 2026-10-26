namespace Ncfe.CodeTest.DataStores
{
    /// <summary>
    /// A store that can load a (non-archived) learner record.
    /// Implemented by both the main and the failover data stores.
    /// </summary>
    public interface ILearnerDataStore
    {
        LearnerResponse GetLearner(int learnerId);
    }
}
