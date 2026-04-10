using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using AutomationProject.Helpers;

namespace AutomationProject.Base
{
    public abstract class BaseTest : IDisposable
    {
        protected IWebDriver Driver { get; private set; }
        
        // Cập nhật URL tới hệ thống IdeaTrack lúc chạy thật
        protected readonly string BaseUrl = "https://localhost:7206"; 

        public BaseTest()
        {
            // Khởi tạo webdriver
            var options = new ChromeOptions();
            options.AcceptInsecureCertificates = true;
            options.AddArgument("--ignore-certificate-errors");
            // options.AddArgument("--headless"); // Mở comment nếu muốn chạy ngầm không hiện trình duyệt
            Driver = new ChromeDriver(options);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            Driver.Manage().Window.Maximize();
        }

        /// <summary>
        /// Hàm Login dùng chung được tái sử dụng cho các test class kế thừa.
        /// </summary>
        /// <param name="username">Username từ TestAccounts constant</param>
        /// <param name="password">Mặc định là TestConstants.DefaultPassword (123456)</param>
        public void Login(string username, string password = TestConstants.DefaultPassword)
        {
            // Điều hướng tới trang đăng nhập
            Driver.Navigate().GoToUrl($"{BaseUrl}/Identity/Account/Login"); // Cập nhật route login tuỳ dự án
            
            // Tìm và cấu hình các elements đăng nhập (Cập nhật locators nếu form không dùng id này)
            var usernameInput = Driver.FindElement(By.Id("Input_UserName")); // Hoặc Name/XPath phù hợp
            var passwordInput = Driver.FindElement(By.Id("Input_Password"));
            var loginButton = Driver.FindElement(By.Id("login-submit"));

            // Nhập thông tin & Submit
            usernameInput.Clear();
            usernameInput.SendKeys(username);
            
            passwordInput.Clear();
            passwordInput.SendKeys(password);
            
            loginButton.Click();
        }

        public void Dispose()
        {
            // Đóng trình duyệt sau khi test xong để giải phóng tài nguyên
            if (Driver != null)
            {
                Driver.Quit();
                Driver.Dispose();
            }
        }
    }
}
