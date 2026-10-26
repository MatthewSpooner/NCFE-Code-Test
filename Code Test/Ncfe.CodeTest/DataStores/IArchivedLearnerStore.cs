namespace Ncfe.CodeTest.DataStores
{
    /// <summary>
    /// Retrieves learners from the archive.
    /// </summary>
    public interface IArchivedLearnerStore
    {
        Learner GetArchivedLearner(int learnerId);
    }
}
