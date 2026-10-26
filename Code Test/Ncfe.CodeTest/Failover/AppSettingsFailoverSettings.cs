using System;
using System.Collections.Specialized;
using System.Configuration;

namespace Ncfe.CodeTest.Failover
{
    /// <summary>
    /// Reads failover settings from appSettings. The threshold and window fall back to the
    /// values that were previously hard coded, so existing configuration keeps working.
    /// </summary>
    public class AppSettingsFailoverSettings : IFailoverSettings
    {
        public const string IsFailoverModeEnabledKey = "IsFailoverModeEnabled";
        public const string FailedRequestThresholdKey = "FailoverFailedRequestThreshold";
        public const string FailedRequestWindowMinutesKey = "FailoverFailedRequestWindowMinutes";

        public const int DefaultFailedRequestThreshold = 100;
        public const int DefaultFailedRequestWindowMinutes = 10;

        private readonly NameValueCollection _appSettings;

        public AppSettingsFailoverSettings()
            : this(ConfigurationManager.AppSettings)
        {
        }

        public AppSettingsFailoverSettings(NameValueCollection appSettings)
        {
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        }

        public bool IsFailoverModeEnabled =>
            bool.TryParse(_appSettings[IsFailoverModeEnabledKey], out var enabled) && enabled;

        public int FailedRequestThreshold =>
            ReadPositiveInt(FailedRequestThresholdKey, DefaultFailedRequestThreshold);

        public TimeSpan FailedRequestWindow =>
            TimeSpan.FromMinutes(ReadPositiveInt(FailedRequestWindowMinutesKey, DefaultFailedRequestWindowMinutes));

        private int ReadPositiveInt(string key, int defaultValue)
        {
            return int.TryParse(_appSettings[key], out var value) && value > 0 ? value : defaultValue;
        }
    }
}
