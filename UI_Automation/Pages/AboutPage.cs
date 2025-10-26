using OpenQA.Selenium;
namespace UI_Automation.Pages
{
    public class AboutPage
    {
        private readonly IWebDriver _driver;

        // Locators
        private IWebElement InstallSteamButton => _driver.FindElement(By.CssSelector("#about_greeting .about_install_steam_link"));
        private IList<IWebElement> OnlineStatus => _driver.FindElements(By.XPath("//div[contains(@class,'gamers_online')]/parent::div"));
        private IList<IWebElement> PlayingNowStatus => _driver.FindElements(By.XPath("//div[contains(@class,'gamers_in_game')]/parent::div"));

        public AboutPage(IWebDriver driver)
        {
            _driver = driver;
        }

        // Actions

        //public void GoTo(string url)
        //{
        //    _driver.Navigate().GoToUrl(url);
        //}

        public bool IsInstallSteamButtonClickable()
        {
            return InstallSteamButton.Displayed && InstallSteamButton.Enabled;
        }

        public bool CompareIfPlayingNowStatusIsLessThanOnlineStatus()
        {
            int onlineStatus = int.Parse(OnlineStatus.Last().Text.Replace("ONLINE", "").Replace(",", ""));
            int playingNowStatus = int.Parse(PlayingNowStatus.Last().Text.Replace("PLAYING NOW", "").Replace(",", ""));

            return onlineStatus > playingNowStatus;
        }
    }
}
