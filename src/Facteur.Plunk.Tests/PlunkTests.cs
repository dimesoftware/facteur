using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Facteur;
using Facteur.Plunk;
using Facteur.TemplateProviders.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PlunkTests
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            // Set environment variable from runsettings if not already set
            // This ensures the environment variable is available even if runsettings isn't picked up
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEST_PLUNK_API_KEY")))
            {
                // Try to get from TestRunParameters first
                string apiKey = null;
                if (TestContext?.Properties != null)
                {
                    if (TestContext.Properties.TryGetValue("TEST_PLUNK_API_KEY", out object value))
                    {
                        apiKey = value?.ToString();
                    }
                }
                
                // If still not found, use default from runsettings
                if (string.IsNullOrEmpty(apiKey))
                {
                    apiKey = "sk_1a5f5ac65716c3f9393c0fa9d46d44ff8fd97ee4d00e1bad";
                }
                
                Environment.SetEnvironmentVariable("TEST_PLUNK_API_KEY", apiKey);
            }
        }

        private string GetApiKey()
        {
            string apiKey = null;
            
            // Try TestRunParameters first (from runsettings)
            if (TestContext?.Properties != null)
            {
                if (TestContext.Properties.TryGetValue("TEST_PLUNK_API_KEY", out object value))
                {
                    apiKey = value?.ToString();
                }
            }
            
            // Fallback to environment variable
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable("TEST_PLUNK_API_KEY");
            }
            
            return apiKey;
        }
        [TestMethod]
        public void Plunk_SendMail_KeyIsNull_ShouldThrowException()
        {
            EmailComposer composer = new();
            EmailRequest request = composer
                .Subject("Hello world")
                .From("info@facteur.com")
                .To("majev@getnada.com")
                .Build();

            Assert.Throws<ArgumentNullException>(() => new PlunkMailer(null));
        }

        [TestMethod]
        public void Plunk_SendMail_KeyIsEmpty_ShouldThrowException()
        {
            EmailComposer composer = new();
            EmailRequest request = composer
                .Subject("Hello world")
                .From("info@facteur.com")
                .To("majev@getnada.com")
                .Build();

            Assert.Throws<ArgumentNullException>(() => new PlunkMailer(""));
        }

        [TestMethod]
        public void Plunk_SendMail_KeyIsNotEmpty_ShouldConstruct()
        {
            EmailComposer composer = new();
            EmailRequest request = composer
                .Subject("Hello world")
                .From("info@facteur.com")
                .To("majev@getnada.com")
                .Build();

            string apiKey = GetApiKey();

            IMailer mailer = new PlunkMailer(apiKey ?? "dummy-key-for-construction-test");
            Assert.IsNotNull(mailer);
        }

        [TestMethod]
        public async Task Plunk_SendMail_ShouldSendActualEmail()
        {
            string apiKey = GetApiKey();
            
            if (string.IsNullOrEmpty(apiKey))
            {
                Assert.Inconclusive("TEST_PLUNK_API_KEY is not set in TestRunParameters or environment variables. Skipping integration test.");
                return;
            }

            EmailComposer composer = new();
            EmailRequest request = composer
                .Subject("Plunk Integration Test - Facteur")
                .From("info@facteur.com")
                .To("majev@getnada.com")
                .Body("<h1>Hello from Plunk!</h1><p>This is a test email sent from the Facteur Plunk integration test.</p>")
                .Build();

            IMailer mailer = new PlunkMailer(apiKey);
            await mailer.SendMailAsync(request);
        }

        [TestMethod]
        public async Task Plunk_SendTemplateMail_ShouldSendActualEmail()
        {
            string apiKey = GetApiKey();
            
            if (string.IsNullOrEmpty(apiKey))
            {
                Assert.Inconclusive("TEST_PLUNK_API_KEY is not set in TestRunParameters or environment variables. Skipping integration test.");
                return;
            }

            IEmailComposer composer = new EmailComposer(
                new ScribanCompiler(),
                new AppDirectoryTemplateProvider("Templates", ".sbnhtml"),
                new ViewModelTemplateResolver());

            EmailRequest request = await composer
                .Subject("Plunk Integration Test - Facteur (Template)")
                .From("info@facteur.com")
                .To("majev@getnada.com")
                .BuildAsync(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" });

            IMailer mailer = new PlunkMailer(apiKey);
            await mailer.SendMailAsync(request);
        }
    }
}

