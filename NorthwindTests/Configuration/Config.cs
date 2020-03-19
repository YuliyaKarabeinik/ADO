using System.Configuration;

namespace NorthwindTests.Configuration
{
    public static class Config
    {
        public static ConnectionStringSettings ConnectionStringItem => ConfigurationManager.ConnectionStrings["NorthwindConnection"];
        public static string ConnectionString => ConnectionStringItem.ConnectionString;
        public static string ProviderName => ConnectionStringItem.ProviderName;
    }
}
