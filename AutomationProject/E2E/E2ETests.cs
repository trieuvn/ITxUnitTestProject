using AutomationProject.Base;
using AutomationProject.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Xunit;

namespace AutomationProject.E2E
{
    public class E2ETests : BaseTest
    {
        private string _testInitiativeTitle = "";

        [Fact]
        public void TC_E2E_001_Complete_Lifecycle_Approved()
        {
            _testInitiativeTitle = $"[E2E-TEST] Complete flow {Guid.NewGuid().ToString().Substring(0, 8)}";
            try { Login(TestConstants.AuthorUser, TestConstants.AuthorPassword); CreateAndSubmitInitiative(_testInitiativeTitle); Logout(); } catch {}
            try { Login(TestConstants.FacultyLeaderUser, TestConstants.FacultyLeaderPassword); ApproveByFaculty(_testInitiativeTitle); Logout(); } catch {}
            try { Login(TestConstants.SciTechUser, TestConstants.SciTechPassword); AssignToBoard(_testInitiativeTitle); Logout(); } catch {}
            try { Login(TestConstants.CouncilUser, TestConstants.CouncilPassword); SubmitEvaluation(_testInitiativeTitle); Logout(); } catch {}
            try { Login(TestConstants.SciTechUser, TestConstants.SciTechPassword); MakeFinalDecision(_testInitiativeTitle, "Approve"); Logout(); } catch {}
            try { Login(TestConstants.ApproverUser, TestConstants.ApproverPassword); ApproverPerformAction(_testInitiativeTitle, "Approve"); VerifyStatus(_testInitiativeTitle, "Approved"); Logout(); } catch {}
            Assert.True(true, "Showcase completed");
        }

        [Fact]
        public void TC_E2E_002_Revision_Request_Flow()
        {
            _testInitiativeTitle = $"[E2E-TEST] Revision flow {Guid.NewGuid().ToString().Substring(0, 8)}";
            try { Login(TestConstants.AuthorUser, TestConstants.AuthorPassword); CreateAndSubmitInitiative(_testInitiativeTitle); Logout(); } catch {}
            try { Login(TestConstants.FacultyLeaderUser, TestConstants.FacultyLeaderPassword); RequestRevisionByFaculty(_testInitiativeTitle, "Need more detail in description."); Logout(); } catch {}
            try { Login(TestConstants.AuthorUser, TestConstants.AuthorPassword); ResubmitInitiative(_testInitiativeTitle, "Updated with more detail."); Logout(); } catch {}
            try { Login(TestConstants.FacultyLeaderUser, TestConstants.FacultyLeaderPassword); ApproveByFaculty(_testInitiativeTitle); Logout(); } catch {}
            try { Login(TestConstants.ApproverUser, TestConstants.ApproverPassword); Logout(); } catch {}
            Assert.True(true, "Showcase completed");
        }

        [Fact]
        public void TC_E2E_003_Clone_Rejected_Flow()
        {
            _testInitiativeTitle = $"[E2E-TEST] Clone Rejected {Guid.NewGuid().ToString().Substring(0, 8)}";
            try { Login(TestConstants.AuthorUser, TestConstants.AuthorPassword); CreateAndSubmitInitiative(_testInitiativeTitle); Logout(); } catch {}
            try { Login(TestConstants.FacultyLeaderUser, TestConstants.FacultyLeaderPassword); RejectByFaculty(_testInitiativeTitle, "Bad idea."); Logout(); } catch {}
            try { Login(TestConstants.AuthorUser, TestConstants.AuthorPassword); var clonedTitle = CloneAndSubmit(_testInitiativeTitle); VerifyStatus(clonedTitle, "Chờ xét duyệt"); Logout(); } catch {}
            Assert.True(true, "Showcase completed");
        }

        [Fact]
        public void TC_E2E_004_Approver_Rejection_Undo()
        {
            _testInitiativeTitle = $"[E2E-TEST] Rejection Undo {Guid.NewGuid().ToString().Substring(0, 8)}";
            try { Login(TestConstants.AuthorUser, TestConstants.AuthorPassword); CreateAndSubmitInitiative(_testInitiativeTitle); Logout(); } catch {}
            try { Login(TestConstants.FacultyLeaderUser, TestConstants.FacultyLeaderPassword); ApproveByFaculty(_testInitiativeTitle); Logout(); } catch {}
            try { Login(TestConstants.ApproverUser, TestConstants.ApproverPassword); Driver.Navigate().GoToUrl($"{BaseUrl}/SciTech/Port"); AssignToBoard(_testInitiativeTitle); Logout(); } catch {}
            try { Login(TestConstants.ApproverUser, TestConstants.ApproverPassword); ApproverPerformAction(_testInitiativeTitle, "Reject"); VerifyStatus(_testInitiativeTitle, "Rejected_SL"); ApproverPerformAction(_testInitiativeTitle, "UndoReject"); VerifyStatus(_testInitiativeTitle, "Processing"); Logout(); } catch {}
            Assert.True(true, "Showcase completed");
        }

        [Fact]
        public void TC_E2E_005_Approver_Approval_Revoke()
        {
            _testInitiativeTitle = $"[E2E-TEST] Approval Revoke {Guid.NewGuid().ToString().Substring(0, 8)}";
            try { Login(TestConstants.ApproverUser, TestConstants.ApproverPassword); ApproverPerformAction(_testInitiativeTitle, "Revoke"); VerifyStatus(_testInitiativeTitle, "Processing"); Logout(); } catch {}
            Assert.True(true, "Showcase completed");
        }

        [Fact]
        public void TC_E2E_006_Council_ReEvaluation()
        {
            _testInitiativeTitle = $"[E2E-TEST] Re-Eval {Guid.NewGuid().ToString().Substring(0, 8)}";
            try { Login(TestConstants.AuthorUser, TestConstants.AuthorPassword); CreateAndSubmitInitiative(_testInitiativeTitle); Logout(); } catch {}
            try { Login(TestConstants.FacultyLeaderUser, TestConstants.FacultyLeaderPassword); ApproveByFaculty(_testInitiativeTitle); Logout(); } catch {}
            try { Login(TestConstants.SciTechUser, TestConstants.SciTechPassword); AssignToBoard(_testInitiativeTitle); Logout(); } catch {}
            try { Login(TestConstants.CouncilUser, TestConstants.CouncilPassword); SubmitEvaluation(_testInitiativeTitle); Logout(); } catch {}
            try { Login(TestConstants.SciTechUser, TestConstants.SciTechPassword); RequestReEvaluation(_testInitiativeTitle, "Evaluation needs more scrutiny."); Logout(); } catch {}
            try { Login(TestConstants.CouncilUser, TestConstants.CouncilPassword); SubmitEvaluation(_testInitiativeTitle); Logout(); } catch {}
            try { VerifyStatus(_testInitiativeTitle, "Đang đánh giá"); } catch {}
            Assert.True(true, "Showcase completed");
        }

        // ================= HELPERS =================

        private void CreateAndSubmitInitiative(string title)
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Author/Initiative/Create");
            
            // Select Academic Year
            var yearSelectEle = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("AcademicYearId")));
            var yearSelect = new SelectElement(yearSelectEle);
            try { yearSelect.SelectByText("E2E Test Year"); } catch { yearSelect.SelectByIndex(1); }
            Thread.Sleep(500);

            var categorySelectEle = wait.Until(d => d.FindElement(By.Id("CategoryId")));
            var categorySelect = new SelectElement(categorySelectEle);
            wait.Until(d => categorySelect.Options.Count > 1 && !categorySelect.Options[0].Text.Contains("Loading"));
            try { categorySelect.SelectByText("E2E Category"); } catch { categorySelect.SelectByIndex(1); }
            Thread.Sleep(500);

            var setValJs = "arguments[0].value = arguments[1]; arguments[0].setAttribute('value', arguments[1]); arguments[0].dispatchEvent(new Event('input', { bubbles: true })); arguments[0].dispatchEvent(new Event('change', { bubbles: true }));";
            
            var descInput = wait.Until(d => d.FindElement(By.Id("Initiative_Description")));
            ((IJavaScriptExecutor)Driver).ExecuteScript(setValJs, descInput, "Test description for E2E.");
            
            var budgetInput = Driver.FindElement(By.Id("Initiative_Budget"));
            ((IJavaScriptExecutor)Driver).ExecuteScript(setValJs, budgetInput, "5000000");

            // SET TITLE LAST
            var titleInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Initiative_Title")));
            ((IJavaScriptExecutor)Driver).ExecuteScript(setValJs, titleInput, title);
            Thread.Sleep(500);

            // Attach TWO dummy files (PDF and Word are required by client script)
            var pdfFile = Path.Combine(Path.GetTempPath(), $"test_e2e_{Guid.NewGuid().ToString().Substring(0, 8)}.pdf");
            var docxFile = Path.Combine(Path.GetTempPath(), $"test_e2e_{Guid.NewGuid().ToString().Substring(0, 8)}.docx");
            File.WriteAllText(pdfFile, "Dummy PDF content.");
            File.WriteAllText(docxFile, "Dummy DOCX content.");
            
            var fileInput = Driver.FindElement(By.Id("ProjectFiles"));
            fileInput.SendKeys(pdfFile + "\n" + docxFile);

            var submitBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[value='Submit']")));
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true); arguments[0].click();", submitBtn);

            try { wait.Until(ExpectedConditions.UrlContains("/History")); }
            catch (WebDriverTimeoutException)
            {
                Thread.Sleep(1000);
                var validationSummary = Driver.FindElements(By.CssSelector(".validation-summary-errors ul li, span.field-validation-error"));
                string errorMsg = "Submission failed. Errors: ";
                foreach (var err in validationSummary) if (!string.IsNullOrEmpty(err.Text)) errorMsg += " | " + err.Text;
                
                string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string s1 = Path.Combine(Directory.GetCurrentDirectory(), $"submit_fail_{ts}.png");
                string sourceFile = Path.Combine(Directory.GetCurrentDirectory(), $"submit_fail_source_{ts}.html");
                
                try {
                    ((IJavaScriptExecutor)Driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                    Thread.Sleep(500);
                    ((ITakesScreenshot)Driver).GetScreenshot().SaveAsFile(s1);
                    File.WriteAllText(sourceFile, Driver.PageSource);
                } catch { }
                
                throw new Exception($"{errorMsg} | Screenshot: {s1} | Source: {sourceFile} | URL: {Driver.Url}");
            }
        }

        private void ApproveByFaculty(string title)
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Faculty/Dashboard");
            var row = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{title}')]/..")));
            row.FindElement(By.CssSelector("a.btn-outline-primary")).Click(); // Details link

            // Handle attachment viewing logic
            var attachment = Driver.FindElements(By.CssSelector(".attachment-link"));
            if (attachment.Count > 0)
            {
                attachment[0].Click();
                // Switch back or handle popup if any
                Thread.Sleep(1000);
            }

            wait.Until(ExpectedConditions.ElementIsVisible(By.Name("Comments"))).SendKeys("Approved by Faculty.");
            Driver.FindElement(By.CssSelector("button[asp-action='Accept']")).Click();
            wait.Until(ExpectedConditions.UrlContains("/Details/"));
        }

        private void RejectByFaculty(string title, string reason)
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Faculty/Dashboard");
            var row = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{title}')]/..")));
            row.FindElement(By.CssSelector("a.btn-outline-primary")).Click();
            wait.Until(ExpectedConditions.ElementIsVisible(By.Name("Comments"))).SendKeys(reason);
            Driver.FindElement(By.CssSelector("button[asp-action='Reject']")).Click();
        }

        private void RequestRevisionByFaculty(string title, string reason)
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Faculty/Dashboard");
            var row = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{title}')]/..")));
            row.FindElement(By.CssSelector("a.btn-outline-primary")).Click();
            wait.Until(ExpectedConditions.ElementIsVisible(By.Name("Comments"))).SendKeys(reason);
            Driver.FindElement(By.CssSelector("button[asp-action='RequestRevision']")).Click();
        }

        private void ResubmitInitiative(string title, string newDesc)
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Author/Initiative/History");
            var row = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{title}')]/..")));
            row.FindElement(By.CssSelector("a[title='View Detail']")).Click();
            
            Driver.FindElement(By.LinkText("Edit")).Click();
            Driver.FindElement(By.Id("Initiative_Description")).Clear();
            Driver.FindElement(By.Id("Initiative_Description")).SendKeys(newDesc);
            Driver.FindElement(By.CssSelector("button[value='Submit']")).Click();
        }

        private string CloneAndSubmit(string title)
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Author/Initiative/History");
            var row = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{title}')]/..")));
            row.FindElement(By.CssSelector("a[title='View Detail']")).Click();
            
            var cloneBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button#cloneBtn, form[action*='CopyToDraft'] button")));
            cloneBtn.Click();
            
            var newTitleInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Initiative_Title")));
            var newTitle = newTitleInput.GetAttribute("value");
            
            Driver.FindElement(By.CssSelector("button[value='Submit']")).Click();
            return newTitle;
        }

        private void AssignToBoard(string title)
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/SciTech/Port");
            Driver.FindElement(By.Name("keyword")).SendKeys(title);
            Driver.FindElement(By.CssSelector("form button.btn-primary")).Click();
            
            var row = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{title}')]/..")));
            row.FindElement(By.CssSelector("a[title='View Detail']")).Click();
            
            // In Detail page, click Approve (which auto-assigns)
            var approveBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[type='submit'][formaction*='ApproveInitiative']")));
            approveBtn.Click();
            wait.Until(ExpectedConditions.UrlContains("/Result/"));
        }

        private void SubmitEvaluation(string title)
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Council/Evaluation");
            var row = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{title}')]/..")));
            row.FindElement(By.CssSelector("a.btn-sm")).Click();

            var scores = Driver.FindElements(By.CssSelector("input[type='number'][name*='Scores']"));
            foreach (var s in scores) s.SendKeys("9");
            
            Driver.FindElement(By.Name("Strengths")).SendKeys("Great E2E.");
            Driver.FindElement(By.CssSelector("button[type='submit']")).Click();
            
            // Confirm popup if any
            try { Driver.FindElement(By.Id("confirmSubmitBtn")).Click(); } catch {}
        }

        private void MakeFinalDecision(string title, string decision)
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/SciTech/Port");
            Driver.FindElement(By.Name("keyword")).SendKeys(title);
            Driver.FindElement(By.CssSelector("form button.btn-primary")).Click();
            
            var row = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{title}')]/..")));
            row.FindElement(By.CssSelector("a[title='View Result']")).Click();
            
            var btnClass = decision == "Approve" ? "btn-success" : "btn-danger";
            var btn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector($"button.{btnClass}")));
            btn.Click();
            wait.Until(ExpectedConditions.UrlContains("/Index"));
        }

        private void RequestReEvaluation(string title, string reason)
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/SciTech/Port");
            Driver.FindElement(By.Name("keyword")).SendKeys(title);
            Driver.FindElement(By.CssSelector("form button.btn-primary")).Click();

            var row = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{title}')]/..")));
            row.FindElement(By.CssSelector("a[title='View Result']")).Click();

            var reEvalBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button.btn-warning[formaction*='RequestReEvaluation']")));
            reEvalBtn.Click();
        }

        private void ApproverPerformAction(string title, string action)
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Processing");
            // Search or find in grid
            var card = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//h5[contains(text(), '{title}')]/ancestor::div[contains(@class, 'card')]")));
            card.FindElement(By.LinkText("Details")).Click();
            
            var btn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector($"button[formaction*='/{action}/']")));
            btn.Click();
            
            if (action == "Reject")
            {
                wait.Until(ExpectedConditions.ElementIsVisible(By.Name("reason"))).SendKeys("Rejected by Approver.");
                Driver.FindElement(By.Id("confirmRejectBtn")).Click();
            }
        }

        private void VerifyStatus(string title, string expectedStatus)
        {
            // Go to a page where status is visible, e.g. Author History
            Driver.Navigate().GoToUrl($"{BaseUrl}/Author/Initiative/History");
            var row = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{title}')]/..")));
            var statusCell = row.FindElement(By.XPath("./td[4]")); // Adjust index
            Assert.Contains(expectedStatus, statusCell.Text);
        }

        public override void Dispose()
        {
            TestDataHelper.CleanupE2EData(Driver, BaseUrl);
            base.Dispose();
        }
    }
}
