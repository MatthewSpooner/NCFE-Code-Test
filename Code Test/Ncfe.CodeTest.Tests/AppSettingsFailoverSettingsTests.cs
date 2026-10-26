using System;
using System.Collections.Specialized;
using Ncfe.CodeTest.Failover;
using Xunit;

namespace Ncfe.CodeTest.Tests
{
    public class AppSettingsFailoverSettingsTests
    {
        [Theory]
        [InlineData("true", true)]
        [InlineData("True", true)]
        [InlineData("TRUE", true)]
        [InlineData("false", false)]
        [InlineData("yes", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsFailoverModeEnabled_ParsesSetting(string value, bool expected)
        {
            var settings = CreateSettings(AppSettingsFailoverSettings.IsFailoverModeEnabledKey, value);

            Assert.Equal(expected, settings.IsFailoverModeEnabled);
        }

        [Fact]
        public void WhenNotConfigured_UsesDefaultThresholdAndWindow()
        {
            var settings = new AppSettingsFailoverSettings(new NameValueCollection());

            Assert.Equal(100, settings.FailedRequestThreshold);
            Assert.Equal(TimeSpan.FromMinutes(10), settings.FailedRequestWindow);
        }

        [Fact]
        public void WhenConfigured_UsesConfiguredThresholdAndWindow()
        {
            var settings = new AppSettingsFailoverSettings(new NameValueCollection
            {
                { AppSettingsFailoverSettings.FailedRequestThresholdKey, "25" },
                { AppSettingsFailoverSettings.FailedRequestWindowMinutesKey, "5" }
            });

            Assert.Equal(25, settings.FailedRequestThreshold);
            Assert.Equal(TimeSpan.FromMinutes(5), settings.FailedRequestWindow);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("0")]
        [InlineData("-5")]
        public void WhenThresholdInvalid_UsesDefault(string value)
        {
            var settings = CreateSettings(AppSettingsFailoverSettings.FailedRequestThresholdKey, value);

            Assert.Equal(AppSettingsFailoverSettings.DefaultFailedRequestThreshold, settings.FailedRequestThreshold);
        }

        private static AppSettingsFailoverSettings CreateSettings(string key, string value)
        {
            return new AppSettingsFailoverSettings(new NameValueCollection { { key, value } });
        }
    }
}
