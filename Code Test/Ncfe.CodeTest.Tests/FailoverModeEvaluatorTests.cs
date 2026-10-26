using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Ncfe.CodeTest.Failover;
using Xunit;

namespace Ncfe.CodeTest.Tests
{
    public class FailoverModeEvaluatorTests
    {
        private const int Threshold = 100;
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(10);
        private static readonly DateTime Now = new DateTime(2024, 3, 1, 12, 0, 0);

        private readonly Mock<IFailoverRepository> _repository = new Mock<IFailoverRepository>();
        private readonly Mock<IFailoverSettings> _settings = new Mock<IFailoverSettings>();
        private readonly Mock<IClock> _clock = new Mock<IClock>();
        private readonly FailoverModeEvaluator _sut;

        public FailoverModeEvaluatorTests()
        {
            _settings.Setup(s => s.IsFailoverModeEnabled).Returns(true);
            _settings.Setup(s => s.FailedRequestThreshold).Returns(Threshold);
            _settings.Setup(s => s.FailedRequestWindow).Returns(Window);
            _clock.Setup(c => c.Now).Returns(Now);

            _sut = new FailoverModeEvaluator(_repository.Object, _settings.Object, _clock.Object);
        }

        [Fact]
        public void IsInFailoverMode_WhenFailuresExceedThresholdWithinWindow_ReturnsTrue()
        {
            GivenEntries(Threshold + 1, Now.AddMinutes(-1));

            Assert.True(_sut.IsInFailoverMode());
        }

        [Fact]
        public void IsInFailoverMode_WhenFailuresEqualThreshold_ReturnsFalse()
        {
            GivenEntries(Threshold, Now.AddMinutes(-1));

            Assert.False(_sut.IsInFailoverMode());
        }

        [Fact]
        public void IsInFailoverMode_IgnoresFailuresOutsideWindow()
        {
            var entries = CreateEntries(Threshold, Now.AddMinutes(-1))
                .Concat(CreateEntries(50, Now - Window))           // exactly on the boundary: excluded
                .Concat(CreateEntries(50, Now.AddMinutes(-30)))
                .ToList();
            _repository.Setup(r => r.GetFailOverEntries()).Returns(entries);

            Assert.False(_sut.IsInFailoverMode());
        }

        [Fact]
        public void IsInFailoverMode_WhenFailoverDisabled_ReturnsFalseWithoutQueryingRepository()
        {
            _settings.Setup(s => s.IsFailoverModeEnabled).Returns(false);
            GivenEntries(Threshold + 1, Now.AddMinutes(-1));

            Assert.False(_sut.IsInFailoverMode());
            _repository.Verify(r => r.GetFailOverEntries(), Times.Never);
        }

        [Fact]
        public void IsInFailoverMode_WhenRepositoryReturnsNull_ReturnsFalse()
        {
            _repository.Setup(r => r.GetFailOverEntries()).Returns((List<FailoverEntry>)null);

            Assert.False(_sut.IsInFailoverMode());
        }

        [Fact]
        public void IsInFailoverMode_UsesConfiguredThresholdAndWindow()
        {
            _settings.Setup(s => s.FailedRequestThreshold).Returns(2);
            _settings.Setup(s => s.FailedRequestWindow).Returns(TimeSpan.FromMinutes(1));
            var entries = CreateEntries(3, Now.AddSeconds(-30))
                .Concat(CreateEntries(10, Now.AddMinutes(-5)))
                .ToList();
            _repository.Setup(r => r.GetFailOverEntries()).Returns(entries);

            Assert.True(_sut.IsInFailoverMode());
        }

        private void GivenEntries(int count, DateTime dateTime)
        {
            _repository.Setup(r => r.GetFailOverEntries()).Returns(CreateEntries(count, dateTime).ToList());
        }

        private static IEnumerable<FailoverEntry> CreateEntries(int count, DateTime dateTime)
        {
            return Enumerable.Range(0, count).Select(i => new FailoverEntry { DateTime = dateTime });
        }
    }
}
