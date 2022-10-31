using DesperateDevs.Serialization;

namespace Jenny.Tests
{
    public class TestPreferences : Preferences
    {
        public TestPreferences(string properties, string userProperties = null, bool doubleQuotedValues = false)
            : base(
                new Properties(properties, doubleQuotedValues),
                new Properties(userProperties ?? string.Empty, doubleQuotedValues),
                doubleQuotedValues
            ) { }

        public TestPreferences(Properties properties, Properties userProperties = null)
            : base(properties, userProperties ?? new Properties()) { }
    }
}
