using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace MediBook.Playwright
{
    public class MediBookTests : PageTest
    {
        private string BaseUrl = "https://medibook.health";
        public override BrowserNewContextOptions ContextOptions()
        {
            return new BrowserNewContextOptions
            {
                IgnoreHTTPSErrors = true
            };
        }

        // Test 1 - Login page loads
        [Test]
        public async Task LoginPage_Loads_Successfully()
        {
            await Page.GotoAsync($"{BaseUrl}/Account/Login");
            await Expect(Page).ToHaveTitleAsync("Login - MediBook");
            var heading = Page.Locator("h4");
            await Expect(heading).ToContainTextAsync("Login");
        }

        // Test 2 - Register page loads
        [Test]
        public async Task RegisterPage_Loads_Successfully()
        {
            await Page.GotoAsync($"{BaseUrl}/Account/Register");
            var heading = Page.Locator("h4");
            await Expect(heading).ToContainTextAsync("Create Account");
        }

        // Test 3 - Patient can login successfully
        [Test]
        public async Task Patient_CanLogin_Successfully()
        {
            await Page.GotoAsync($"{BaseUrl}/Account/Login");
            await Page.FillAsync("input[name='Email']", "Janedoe@gmail.com");
            await Page.FillAsync("input[name='Password']", "Patient456");
            await Page.ClickAsync("button[type='submit']");
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/Patient/Index");
        }

        // Test 4 - Invalid login shows error
        [Test]
        public async Task InvalidLogin_Shows_ErrorMessage()
        {
            await Page.GotoAsync($"{BaseUrl}/Account/Login");
            await Page.FillAsync("input[name='Email']", "wrong@test.com");
            await Page.FillAsync("input[name='Password']", "wrongpassword");
            await Page.ClickAsync("button[type='submit']");
            var error = Page.Locator("div.text-danger");
            await Expect(error).ToContainTextAsync("Invalid email or password");
        }

        // Test 5 - Patient dashboard loads after login
        [Test]
        public async Task PatientDashboard_Loads_AfterLogin()
        {
            await Page.GotoAsync($"{BaseUrl}/Account/Login");
            await Page.FillAsync("input[name='Email']", "delrosan@tcd.ie");
            await Page.FillAsync("input[name='Password']", "Patient123");
            await Page.ClickAsync("button[type='submit']");
            var heading = Page.Locator("h2");
            await Expect(heading).ToContainTextAsync("Nicole Del Rosario!");
        }
    }
}