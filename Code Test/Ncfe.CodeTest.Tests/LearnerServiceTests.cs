using System;
using Moq;
using Ncfe.CodeTest.DataStores;
using Xunit;

namespace Ncfe.CodeTest.Tests
{
    public class LearnerServiceTests
    {
        private const int LearnerId = 42;

        private readonly Mock<IArchivedLearnerStore> _archivedLearnerStore = new Mock<IArchivedLearnerStore>(MockBehavior.Strict);
        private readonly Mock<ILearnerDataStore> _learnerDataStore = new Mock<ILearnerDataStore>(MockBehavior.Strict);
        private readonly LearnerService _sut;

        public LearnerServiceTests()
        {
            _sut = new LearnerService(_archivedLearnerStore.Object, _learnerDataStore.Object);
        }

        [Fact]
        public void GetLearner_WhenLearnerIsArchived_ReturnsLearnerFromArchiveWithoutQueryingDataStore()
        {
            var archivedLearner = new Learner { Id = LearnerId, Name = "Archived" };
            _archivedLearnerStore.Setup(s => s.GetArchivedLearner(LearnerId)).Returns(archivedLearner);

            var result = _sut.GetLearner(LearnerId, isLearnerArchived: true);

            Assert.Same(archivedLearner, result);
            _learnerDataStore.Verify(s => s.GetLearner(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void GetLearner_WhenLearnerIsNotArchived_ReturnsLearnerFromDataStore()
        {
            var learner = new Learner { Id = LearnerId, Name = "Live" };
            _learnerDataStore.Setup(s => s.GetLearner(LearnerId))
                .Returns(new LearnerResponse { IsArchived = false, Learner = learner });

            var result = _sut.GetLearner(LearnerId, isLearnerArchived: false);

            Assert.Same(learner, result);
            _archivedLearnerStore.Verify(s => s.GetArchivedLearner(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void GetLearner_WhenDataStoreReportsLearnerArchived_ReturnsLearnerFromArchive()
        {
            var archivedLearner = new Learner { Id = LearnerId, Name = "Archived" };
            _learnerDataStore.Setup(s => s.GetLearner(LearnerId))
                .Returns(new LearnerResponse { IsArchived = true, Learner = new Learner { Name = "Stale" } });
            _archivedLearnerStore.Setup(s => s.GetArchivedLearner(LearnerId)).Returns(archivedLearner);

            var result = _sut.GetLearner(LearnerId, isLearnerArchived: false);

            Assert.Same(archivedLearner, result);
        }

        [Fact]
        public void GetLearner_WhenDataStoreReturnsNoResponse_ReturnsNull()
        {
            _learnerDataStore.Setup(s => s.GetLearner(LearnerId)).Returns((LearnerResponse)null);

            var result = _sut.GetLearner(LearnerId, isLearnerArchived: false);

            Assert.Null(result);
        }

        [Fact]
        public void Constructor_WithNullDependencies_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new LearnerService(null, _learnerDataStore.Object));
            Assert.Throws<ArgumentNullException>(() => new LearnerService(_archivedLearnerStore.Object, null));
        }

        [Fact]
        public void DefaultConstructor_WiresProductionDependencies()
        {
            var service = new LearnerService();

            Assert.NotNull(service.GetLearner(LearnerId, isLearnerArchived: true));
        }
    }
}
