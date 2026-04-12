using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using AutomationProject.Helpers;

namespace AutomationProject.Base
{
    public abstract class BaseTest : IDisposable
    {
        protected IWebDriver Driver { get; private set; }
        protected readonly string BaseUrl = "http://localhost:5227"; 
        protected WebDriverWait wait;

        public BaseTest()
        {
            var options = new ChromeOptions();
            options.AcceptInsecureCertificates = true;
            options.AddArgument("--ignore-certificate-errors");
            // options.AddArgument("--headless"); 
            Driver = new ChromeDriver(options);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            Driver.Manage().Window.Maximize();
            wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
        }

        public static void Login(IWebDriver Driver, string BaseUrl, string username, string password)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
            
            // Check if already logged in as the correct user
            try {
                Driver.Navigate().GoToUrl($"{BaseUrl}/");
                var userProfile = Driver.FindElements(By.Id("userDropdown")).FirstOrDefault();
                if (userProfile != null && userProfile.Text.Contains(username, StringComparison.OrdinalIgnoreCase)) {
                    return; // Already logged in
                }
                // If logged in as someone else, logout first
                if (userProfile != null) {
                    Logout(Driver, BaseUrl);
                }
            } catch { /* Ignore and proceed to login page */ }

            Driver.Navigate().GoToUrl($"{BaseUrl}/Identity/Account/Login");
            
            try {
                var usernameInput = wait.Until(d => d.FindElements(By.Id("Input_UsernameOrEmail")).FirstOrDefault(e => e.Displayed) 
                                               ?? d.FindElements(By.Name("Input.UsernameOrEmail")).FirstOrDefault(e => e.Displayed));
                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].value = arguments[1]; arguments[0].dispatchEvent(new Event('input', { bubbles: true })); arguments[0].dispatchEvent(new Event('change', { bubbles: true }));", usernameInput, username);
                Thread.Sleep(200);
  
                var passwordInput = wait.Until(d => d.FindElement(By.Id("Input_Password")));
                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].value = arguments[1]; arguments[0].dispatchEvent(new Event('input', { bubbles: true })); arguments[0].dispatchEvent(new Event('change', { bubbles: true }));", passwordInput, password);
                Thread.Sleep(200);
  
                var loginButton = wait.Until(d => d.FindElements(By.CssSelector(".btn-signin")).FirstOrDefault(e => e.Displayed)
                                             ?? d.FindElements(By.CssSelector("button[type='submit']")).FirstOrDefault());
                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", loginButton);
 
                wait.Until(d => !d.Url.Contains("/Account/Login"));
            } catch (Exception ex) {
                string currentUrl = Driver.Url;
                string screenshotPath = Path.Combine(Directory.GetCurrentDirectory(), $"login_fail_{username}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                try {
                    ((ITakesScreenshot)Driver).GetScreenshot().SaveAsFile(screenshotPath);
                    Console.WriteLine($"[LOGIN DEBUG] Screenshot saved to: {screenshotPath}");
                } catch { }
                throw new Exception($"Login failed for user '{username}' at URL '{currentUrl}'. Error: {ex.Message}. Screenshot: {screenshotPath}");
            }
        }

        public void Login(string username, string password)
        {
            Login(Driver, BaseUrl, username, password);
        }

        public static void Logout(IWebDriver Driver, string BaseUrl)
        {
            // Simple approach: go to a page and click logout if visible
            Driver.Navigate().GoToUrl($"{BaseUrl}/");
            
            try {
                var userDropdown = Driver.FindElements(By.Id("userDropdown")).FirstOrDefault();
                if (userDropdown != null && userDropdown.Displayed) {
                    userDropdown.Click();
                    var logoutBtn = Driver.FindElement(By.CssSelector("form[action*='Logout'] button"));
                    logoutBtn.Click();
                } else {
                    // Fallback to direct navigation if possible (usually needs CSRF though)
                    Driver.Navigate().GoToUrl($"{BaseUrl}/Identity/Account/Logout");
                    var confirmLogout = Driver.FindElements(By.CssSelector("button[type='submit']")).FirstOrDefault();
                    if (confirmLogout != null) confirmLogout.Click();
                }
            } catch {
                // If all else fails, clear cookies
                Driver.Manage().Cookies.DeleteAllCookies();
                Driver.Navigate().Refresh();
            }
            
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.Url.Contains("/Account/Login") || d.Url == $"{BaseUrl}/" || d.Url.Contains("/Home/Index"));
        }

        public void Logout()
        {
            Logout(Driver, BaseUrl);
        }

        public virtual void Dispose()
        {
            if (Driver != null)
            {
                Driver.Quit();
                Driver.Dispose();
            }
        }
    }
}
