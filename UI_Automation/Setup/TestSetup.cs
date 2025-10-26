using Allure.Net.Commons;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using Reqnroll.BoDi;
using System.Runtime.InteropServices;
using UI_Automation.Enums;
using UI_Automation.Support;

namespace UI_Automation.Setup
{
    [Binding]
    // BaseTest provides setup and teardown for Selenium WebDriver tests.
    public class TestSetup : IDisposable
    {
        private readonly IObjectContainer _container;
        private readonly IScenarioContext _scenarioContext;
        private IWebDriver _driver;
        private static Config _config;

        public TestSetup(IObjectContainer container, ScenarioContext scenarioContext)
        {
            _container = container;
            _scenarioContext = _container.Resolve<ScenarioContext>();
        }

        [OneTimeSetUp]
        public void Init()
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
        }

        [BeforeTestRun]
        public static void GlobalSetup()
        {
            _config = Config.Load();
            AllureLifecycle.Instance.CleanupResultDirectory();
        }

        // InitializeWebDriver is called before each scenario to set up the WebDriver instance.
        [BeforeScenario]
        public void InitializeWebDriver()
        {
            _driver = CreateWebDriver(_config.Browser, _config.Headless, _config.Incognito);
            _driver.Manage().Window.Maximize();
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            // Register the instance for DI
            _container.RegisterInstanceAs<IWebDriver>(_driver);
            Logger.Log($"Initialized WebDriver for browser: {_config.Browser}, Headless: {_config.Headless}, Incognito: {_config.Incognito}");
        }

        // Factory method for WebDriver creation
        private static IWebDriver CreateWebDriver(string browserName, bool headless, bool incognito)
        {
            if (Enum.TryParse(browserName, true, out BrowserType browser))
            {
                switch (browser)
                {
                    case BrowserType.Chrome:
                        var chromeOptions = new ChromeOptions();
                        if (headless)
                            chromeOptions.AddArgument("--headless=new");
                        if (incognito)
                            chromeOptions.AddArgument("--incognito");
                        return new ChromeDriver(chromeOptions);

                    case BrowserType.Firefox:
                        var firefoxOptions = new FirefoxOptions();
                        if (headless)
                            firefoxOptions.AddArgument("--headless");
                        if (incognito)
                            firefoxOptions.AddArgument("-private");
                        return new FirefoxDriver(firefoxOptions);

                    case BrowserType.Edge:
                        var edgeOptions = new EdgeOptions();
                        if (headless)
                            edgeOptions.AddArgument("headless");
                        if (incognito)
                            edgeOptions.AddArgument("inprivate");
                        return new EdgeDriver(edgeOptions);

                    case BrowserType.Safari:
                        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                            throw new PlatformNotSupportedException("Safari is only supported on macOS.");
                        if (headless)
                            throw new NotSupportedException("Safari does not support headless mode via Selenium.");
                        if (incognito)
                            throw new NotSupportedException("Safari does not support incognito mode via Selenium.");
                        return new OpenQA.Selenium.Safari.SafariDriver();
                }
            }
            throw new ArgumentOutOfRangeException(nameof(browser), $"Browser '{browser}' is not supported.");
        }

        public void NavigateToBaseUrl()
        {
            // Use the BaseUrl from config, fallback to Steam store if not set
            string baseUrl = _config.BaseUrl ?? "https://store.steampowered.com/";
            _driver.Navigate().GoToUrl(baseUrl);
        }

        [AfterScenario]
        // Dispose is called after each test to clean up resources.
        public void Dispose()
        {
            if (_driver != null)
            {
                _driver.Quit();
                _driver = null;
            }
        }
    }
}
