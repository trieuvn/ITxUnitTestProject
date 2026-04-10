using System;
using System.Threading;
using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using AutomationProject.Base;
using AutomationProject.Helpers;

namespace AutomationProject.Faculty
{
    public class FacultyInitiativeTests : BaseTest
    {
        public FacultyInitiativeTests() : base()
        {
            // Trong XUnit, constructor sẽ được chạy trước MỖI testcase (tương đương với [TestInitialize] của MSTest).
            // Tiến hành tự động đăng nhập trước bằng tài khoản FacultyLeader
            Login(TestConstants.TestAccounts.FacultyLeader, TestConstants.DefaultPassword);
            
            // Đợi 2s để trang load xong các session/cookies trước khi đi vào detail từng trang
            Thread.Sleep(2000); 
        }

        [Fact]
        public void TC01_Review_AcceptInitiative()
        {
            // Arrange: Điều hướng đến trang duyệt sáng kiến chi tiết
            Driver.Navigate().GoToUrl($"{BaseUrl}/Faculty/Processing/Detail/1"); 

            // Act: Nhập comments và bấm Approve
            var commentInput = Driver.FindElement(By.Id("ReviewComment"));
            commentInput.SendKeys("Sáng kiến tốt");
            
            var acceptBtn = Driver.FindElement(By.Id("btn-accept"));
            acceptBtn.Click();
            
            // Ngầm chờ web xử lý
            Thread.Sleep(1000);

            // Assert: Kiểm tra thông báo có hiển thị "Initiative accepted..." và trạng thái chuyển thành Faculty_Approved
            var successMessage = Driver.FindElement(By.CssSelector(".alert-success")).Text;
            Assert.Contains("Initiative accepted", successMessage);

            var statusBadge = Driver.FindElement(By.Id("initiative-status")).Text;
            Assert.Equal("Faculty_Approved", statusBadge);
        }

        [Fact]
        public void TC02_Review_RequestRevision()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Faculty/Processing/Detail/1"); 

            var commentInput = Driver.FindElement(By.Id("ReviewComment"));
            commentInput.SendKeys("Vui lòng bổ sung số liệu");
            
            var requestRevisionBtn = Driver.FindElement(By.Id("btn-request-revision"));
            requestRevisionBtn.Click();
            
            Thread.Sleep(1000);

            var successMessage = Driver.FindElement(By.CssSelector(".alert-success")).Text;
            Assert.Contains("Revision request sent", successMessage);

            var statusBadge = Driver.FindElement(By.Id("initiative-status")).Text;
            Assert.Equal("Revision_Required", statusBadge);
        }

        [Fact]
        public void TC03_Review_RequestRevision_BlankComment()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Faculty/Processing/Detail/1");

            var commentInput = Driver.FindElement(By.Id("ReviewComment"));
            commentInput.Clear(); // Blank comment
            
            var requestRevisionBtn = Driver.FindElement(By.Id("btn-request-revision"));
            requestRevisionBtn.Click();
            
            Thread.Sleep(1000);

            // Assert: Form báo lỗi vui lòng nhập lý do
            var errorMessage = Driver.FindElement(By.CssSelector(".text-danger")).Text;
            Assert.Contains("Vui lòng nhập lý do chỉnh sửa", errorMessage);
        }

        [Fact]
        public void TC04_Review_RejectInitiative()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Faculty/Processing/Detail/1"); 

            var commentInput = Driver.FindElement(By.Id("ReviewComment"));
            commentInput.SendKeys("Không khả thi");
            
            var rejectBtn = Driver.FindElement(By.Id("btn-reject"));
            rejectBtn.Click();
            
            Thread.Sleep(1000);

            var successMessage = Driver.FindElement(By.CssSelector(".alert-success")).Text;
            Assert.Contains("Initiative has been rejected", successMessage);

            var statusBadge = Driver.FindElement(By.Id("initiative-status")).Text;
            Assert.Equal("Rejected", statusBadge);
        }

        [Fact]
        public void TC05_Search_Initiatives()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Faculty/Processing");

            var searchInput = Driver.FindElement(By.Id("searchInput"));
            searchInput.SendKeys("Giải pháp AI");
            
            var searchBtn = Driver.FindElement(By.Id("btn-search"));
            searchBtn.Click();

            Thread.Sleep(1500);

            var titleResults = Driver.FindElements(By.CssSelector(".initative-table .title-col"));
            foreach (var element in titleResults)
            {
                Assert.Contains("Giải pháp AI", element.Text, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void TC06_FilterBy_AcademicYear_Period()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Dashboard/Faculty");

            // Chọn select year
            var yearSelectElement = Driver.FindElement(By.Id("AcademicYearId"));
            var yearSelect = new SelectElement(yearSelectElement);
            yearSelect.SelectByText("2025-2026");

            // Chọn select đợt tải
            var periodSelectElement = Driver.FindElement(By.Id("PeriodId"));
            var periodSelect = new SelectElement(periodSelectElement);
            periodSelect.SelectByText("2025-2026");

            var filterBtn = Driver.FindElement(By.Id("btn-filter"));
            filterBtn.Click();

            Thread.Sleep(3000);

            // Kiểm tra số dư / layout trên trang để confirm đã filter thành công.
            Assert.Contains("2025-2026", Driver.PageSource);
        }

        [Fact]
        public void TC07_Export_Dashboard_To_Excel()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Dashboard/Faculty");

            var exportBtn = Driver.FindElement(By.Id("btn-export"));
            exportBtn.Click();

            Thread.Sleep(3000); // Chờ web sinh và tải file xuống 

            // Check trong thư mục Downloads của hệ điều hành
            string userDownloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
            string expectedFilePrefix = $"Initiative_Stats_{DateTime.Now:ddMMyyyy}.csv";
            string expectedFilePath = System.IO.Path.Combine(userDownloadsFolder, expectedFilePrefix);

            Assert.True(System.IO.File.Exists(expectedFilePath), "File Excel/CSV không tồn tại trong thư mục Downloads.");

            // Dọn dẹp
            if (System.IO.File.Exists(expectedFilePath))
            {
                System.IO.File.Delete(expectedFilePath);
            }
        }

        [Fact]
        public void TC08_View_File_PDF()
        {
            // API Convert Viewer
            Driver.Navigate().GoToUrl($"{BaseUrl}/Document/ViewFile?fileName=tailieu.docx");

            Thread.Sleep(6000); // Quá trình LibreOffice convert sang docx hơi mất thời gian (khoảng vài giây)
            
            // Kì vọng là hệ thống hiển thị màn hình PDF Viewer thành công thay vì báo lỗi
            var pdfCanvas = Driver.FindElements(By.Id("pdf-canvas")); // IFrame/Canvas id mock
            Assert.NotEmpty(pdfCanvas);

            var errorBlock = Driver.FindElements(By.CssSelector(".convert-error"));
            Assert.Empty(errorBlock);
        }

        [Fact]
        public void TC09_UpdateProfile_ValidData()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Identity/Account/Manage");

            Driver.FindElement(By.Id("Input_FullName")).Clear();
            Driver.FindElement(By.Id("Input_FullName")).SendKeys("TS. Lê B");

            Driver.FindElement(By.Id("Input_Email")).Clear();
            Driver.FindElement(By.Id("Input_Email")).SendKeys("leb@truong.edu.vn");

            Driver.FindElement(By.Id("Input_PhoneNumber")).Clear();
            Driver.FindElement(By.Id("Input_PhoneNumber")).SendKeys("0901234567");

            Driver.FindElement(By.Id("update-profile-button")).Click();
            Thread.Sleep(1000);

            var alertMessage = Driver.FindElement(By.CssSelector(".alert-success")).Text;
            Assert.Contains("Cập nhật thành công", alertMessage);
        }

        [Fact]
        public void TC10_UpdateProfile_InvalidEmail()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Identity/Account/Manage");

            // Chỉ focus đổi email
            Driver.FindElement(By.Id("Input_Email")).Clear();
            Driver.FindElement(By.Id("Input_Email")).SendKeys("leb-truong.edu.vn");

            Driver.FindElement(By.Id("update-profile-button")).Click();
            Thread.Sleep(1000);

            var errorVal = Driver.FindElement(By.Id("Input_Email-error")).Text;
            Assert.Contains("Invalid email address", errorVal);
        }

        [Fact]
        public void TC11_UpdateProfile_MissingFullName()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Identity/Account/Manage");

            // Xoá FullName
            Driver.FindElement(By.Id("Input_FullName")).Clear();
            Driver.FindElement(By.Id("Input_FullName")).SendKeys("");

            Driver.FindElement(By.Id("update-profile-button")).Click();
            Thread.Sleep(1000);

            var errorVal = Driver.FindElement(By.Id("Input_FullName-error")).Text;
            Assert.Contains("Full name is required", errorVal);
        }
    }
}
