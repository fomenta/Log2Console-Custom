using Log2Console.Components.Utilities;

namespace Log2Console.Core
{
    public static class Log2ConsoleConfig
    {
        public static class UserSettings
        {
            public static class Storage
            {
                public static bool UseAppDir { get => ConfigUtility.AppSettingsToBoolean("Log2Console.UserSettings.Storage.UseAppDir", defaultValue: "1"); }
                public static bool UseJsonFormat { get => ConfigUtility.AppSettingsToBoolean("Log2Console.UserSettings.Storage.UseJsonFormat", defaultValue: "1"); }
            }
        }
    }
}
