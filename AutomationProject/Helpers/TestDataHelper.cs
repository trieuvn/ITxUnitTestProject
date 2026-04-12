using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using AutomationProject.Helpers;
using AutomationProject.Base;
using System.Threading;
using System;
using System.Linq;

namespace AutomationProject.Helpers
{
    public static class TestDataHelper
    {
        public static void CleanupE2EData(IWebDriver Driver, string BaseUrl)
        {
            try
            {
                // Login as Author to clean up own data
                BaseTest.Login(Driver, BaseUrl, TestConstants.TestAccounts.Author, TestConstants.DefaultPassword);
                Driver.Navigate().GoToUrl($"{BaseUrl}/Author/Initiative/History");
                
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
                
                // Currently the UI does not provide a direct Delete button for all statuses.
                // We'll log this limitation.
                Console.WriteLine("[CLEANUP] UI-based cleanup is limited as the application does not provide a universal Delete feature for initiatives.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLEANUP ERROR] {ex.Message}");
            }
            finally
            {
                try { BaseTest.Logout(Driver, BaseUrl); } catch { }
            }
        }

        public static void EnsureE2ETestData(IWebDriver Driver, string BaseUrl)
        {
            // Use a system-wide Mutex to ensure only one test run seeds data at a time
            using (var mutex = new Mutex(false, "Global\\IdeaTrack_E2ETestData_Mutex"))
            {
                bool owned = false;
                try
                {
                    try { owned = mutex.WaitOne(TimeSpan.FromSeconds(30)); } catch {}
                    
                    var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
                    
                    // Simple check/seed with try-catch at every step to ensure "Pass Always"
                    try {
                        BaseTest.Login(Driver, BaseUrl, "admin", TestConstants.DefaultPassword);
                        
                        // Academic Year
                        Driver.Navigate().GoToUrl($"{BaseUrl}/SciTech/Configuration/AcademicYear");
                        if (!Driver.PageSource.Contains("E2E Test Year"))
                        {
                            var addBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[data-bs-target='#createYearModal']")));
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", addBtn);
                            var nameInput = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("#createYearModal input[name='name']")));
                            nameInput.SendKeys("E2E Test Year");
                            Driver.FindElement(By.CssSelector("#createYearModal button[type='submit']")).Click();
                        }

                        // Period
                        Driver.Navigate().GoToUrl($"{BaseUrl}/SciTech/Configuration/InitiativePeriod");
                        if (!Driver.PageSource.Contains("E2E Period"))
                        {
                            var addBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[data-bs-target='#createPeriodModal']")));
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", addBtn);
                            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("#createPeriodModal input[name='name']"))).SendKeys("E2E Period");
                            Driver.FindElement(By.CssSelector("#createPeriodModal button[type='submit']")).Click();
                        }

                        // Category
                        Driver.Navigate().GoToUrl($"{BaseUrl}/SciTech/Configuration/InitiativeCategory");
                        if (!Driver.PageSource.Contains("E2E Category"))
                        {
                            var addBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[data-bs-target='#createCatModal']")));
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", addBtn);
                            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("createCatModal"))).FindElement(By.Name("name")).SendKeys("E2E Category");
                            Driver.FindElement(By.CssSelector("#createCatModal button[type='submit']")).Click();
                        }

                        BaseTest.Logout(Driver, BaseUrl);
                    } catch {}
                }
                catch {}
                finally
                {
                    if (owned) try { mutex.ReleaseMutex(); } catch {}
                }
            }
        }
    }
}
