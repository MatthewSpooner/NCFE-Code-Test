using Moq;
using Ncfe.CodeTest.DataStores;
using Ncfe.CodeTest.Failover;
using Xunit;

namespace Ncfe.CodeTest.Tests
{
    public class FailoverAwareLearnerDataStoreTests
    {
        private const int LearnerId = 7;

        private readonly Mock<ILearnerDataStore> _mainStore = new Mock<ILearnerDataStore>(MockBehavior.Strict);
        private readonly Mock<ILearnerDataStore> _failoverStore = new Mock<ILearnerDataStore>(MockBehavior.Strict);
        private readonly Mock<IFailoverModeEvaluator> _evaluator = new Mock<IFailoverModeEvaluator>();
        private readonly FailoverAwareLearnerDataStore _sut;

        public FailoverAwareLearnerDataStoreTests()
        {
            _sut = new FailoverAwareLearnerDataStore(_mainStore.Object, _failoverStore.Object, _evaluator.Object);
        }

        [Fact]
        public void GetLearner_WhenNotInFailoverMode_UsesMainStore()
        {
            var response = new LearnerResponse();
            _evaluator.Setup(e => e.IsInFailoverMode()).Returns(false);
            _mainStore.Setup(s => s.GetLearner(LearnerId)).Returns(response);

            var result = _sut.GetLearner(LearnerId);

            Assert.Same(response, result);
            _failoverStore.Verify(s => s.GetLearner(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void GetLearner_WhenInFailoverMode_UsesFailoverStore()
        {
            var response = new LearnerResponse();
            _evaluator.Setup(e => e.IsInFailoverMode()).Returns(true);
            _failoverStore.Setup(s => s.GetLearner(LearnerId)).Returns(response);

            var result = _sut.GetLearner(LearnerId);

            Assert.Same(response, result);
            _mainStore.Verify(s => s.GetLearner(It.IsAny<int>()), Times.Never);
        }
    }
}
