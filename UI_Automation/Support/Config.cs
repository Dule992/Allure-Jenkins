using System.Text.Json;
using System.Text.Json.Serialization;

namespace UI_Automation.Support
{
    // Represents strongly-typed configuration loaded from appsettings.json
    public class Config
    {
        // Example properties - add more as needed to match your appsettings.json structure
        public string BaseUrl { get; set; }
        public string Browser { get; set; }
        public bool Headless { get; set; } = true; // Default to headless mode
        public bool Incognito { get; set; } = true;

        // Singleton instance for global access
        private static Config _instance;
        private static readonly object _lock = new();

        // Add a public constructor with all properties, annotated with [JsonConstructor]
        [JsonConstructor]
        public Config(string baseUrl, string browser, bool headless = true, bool incognito = true)
        {
            BaseUrl = baseUrl;
            Browser = browser;
            Headless = headless;
            Incognito = incognito;
        }

        /// <summary>
        /// Loads configuration from appsettings.json and returns a singleton instance.
        /// </summary>
        /// <param name="fileName">The name of the settings file (default: "appsettings.json").</param>
        /// <returns>Config instance with properties set from the file.</returns>
        public static Config Load(string fileName = "appsettings.json")
        {
            if (_instance != null)
                return _instance;

            lock (_lock)
            {
                if (_instance != null)
                    return _instance;

                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var filePath = Path.Combine(basePath, fileName);

                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"Configuration file '{fileName}' not found at '{filePath}'.");

                var json = File.ReadAllText(filePath);
                _instance = JsonSerializer.Deserialize<Config>(json);

                if (_instance == null)
                    throw new InvalidOperationException("Failed to deserialize configuration.");

                return _instance;
            }
        }
    }
}
