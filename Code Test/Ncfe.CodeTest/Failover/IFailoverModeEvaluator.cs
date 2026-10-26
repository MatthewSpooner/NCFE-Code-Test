namespace Ncfe.CodeTest.Failover
{
    /// <summary>
    /// Decides whether learner requests should be served from the failover store.
    /// </summary>
    public interface IFailoverModeEvaluator
    {
        bool IsInFailoverMode();
    }
}
