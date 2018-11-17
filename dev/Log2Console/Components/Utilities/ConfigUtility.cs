using System;
using System.Collections.Concurrent;
using System.Configuration;

namespace Log2Console.Components.Utilities
{
    public static class ConfigUtility
    {
        private static ConcurrentDictionary<string, object> _CacheDictionary = new ConcurrentDictionary<string, object>();

        public static string ConnectionStringsGet(string cnnStringName, bool required = true)
        {
            var cnnString = ConfigurationManager.ConnectionStrings[cnnStringName];
            if (cnnString == null)
            {
                if (required) { throw new ArgumentNullException("ConnectionStrings", $"Missing ConnectionString: '{cnnStringName}'"); }
                else { return null; }
            }

            return cnnString.ConnectionString;
        }

        public static string CnnStringOrAppSettings(string cnnStringName, string appSettingKey)
        {
            var cacheKey = $"{cnnStringName}|{appSettingKey}";

            return (string)_CacheDictionary.GetOrAdd(cacheKey, _ =>
            {
                // search connectionStrings first using cnnStringName
                var value = ConnectionStringsGet(cnnStringName, required: false);
                // search appSettings using appSettingKey
                if (string.IsNullOrWhiteSpace(value)) { value = ConfigurationManager.AppSettings[appSettingKey]; }
                // raise error if not found on any
                if (string.IsNullOrWhiteSpace(value)) { throw new ArgumentNullException(cacheKey); }

                return value;
            });
        }

        public static string AppSettingsToStringWithFallback(string key, string fallbackKey, string defaultValue = null)
        {
            var value = AppSettingsToString(key, required: false);
            if (string.IsNullOrWhiteSpace(value)) { value = AppSettingsToString(fallbackKey, defaultValue, required: false); }
            // valor requerido
            if (string.IsNullOrWhiteSpace(value)) { throw new ArgumentNullException(key); }
            return value;
        }


        public static string AppSettingsToString(string key, string defaultValue = null, bool required = true)
        {
            return (string)_CacheDictionary.GetOrAdd(key, _ =>
            {
                string stringValue = ConfigurationManager.AppSettings[key];
                string value;
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    if (required) { throw new ArgumentNullException(key); }
                    value = defaultValue;
                }
                else
                { value = stringValue; }

                return value;
            });
        }

        public static int AppSettingsToInt32(string key, int? defaultValue = null, int? minValue = null)
        {
            return (int)_CacheDictionary.GetOrAdd(key, _ =>
            {
                string stringValue = ConfigurationManager.AppSettings[key];
                int value;
                if (string.IsNullOrWhiteSpace(stringValue))
                { value = defaultValue ?? throw new ArgumentNullException(key); }
                else
                { value = Convert.ToInt32(stringValue); }

                if (minValue.HasValue && value < minValue) { throw new ArgumentOutOfRangeException(key, $"AppSetting '{key}' value cannot be less than '{minValue.Value}'"); }

                return value;
            });
        }


        public static bool AppSettingsToBoolean(string key, String defaultValue = "0")
        {
            return (bool)_CacheDictionary.GetOrAdd(key, _ =>
            {
                string stringValue = (ConfigurationManager.AppSettings[key] ?? defaultValue);
                bool value = stringValue == "1" || stringValue.ToUpper() == "TRUE";
                return value;
            });

        }
    }
}
