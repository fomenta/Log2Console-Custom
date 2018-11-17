using Log2Console.Settings;
using NUnit.Framework;

namespace Log2Console.Tests
{
    [TestFixture, Category("Settings")]
    public class UserSettingsTest
    {

        [Test]
        public void GetSettings()
        {
            var loaded = UserSettings.Load();
            Assert.IsTrue(loaded, nameof(loaded));

            var loadedSettings = UserSettings.Instance;
            Assert.IsNotNull(loadedSettings, nameof(loadedSettings));

            loadedSettings.Save();
        }
    }
}
