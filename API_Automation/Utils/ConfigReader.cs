using Microsoft.Extensions.Configuration;
using System.IO;

namespace API_Automation.Utils
{
    public static class ConfigReader
    {
        private static readonly IConfigurationRoot configuration;

        static ConfigReader()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }

        public static string GetBaseUrl(string key)
            => configuration[$"baseUrls:{key}"];

        public static int GetTimeout()
            => int.Parse(configuration["timeoutSeconds"]);
    }
}
