using System;
using Allure.Net.Commons;
using NUnit.Framework;
using Reqnroll;

namespace API_Automation.Hooks
{
    [Binding]
    public sealed class Hooks
    {
        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            try
            {
                
                Environment.SetEnvironmentVariable("ALLURE_CONFIG", "allureConfig.json");

                AllureLifecycle.Instance.CleanupResultDirectory();
                TestContext.Out.WriteLine("Allure lifecycle initialized (root allure-results).");
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine($"Allure init failed: {ex.Message}");
            }
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            TestContext.Out.WriteLine("Allure test execution completed.");
        }
    }
}
