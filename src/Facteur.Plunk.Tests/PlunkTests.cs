using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Facteur;
using Facteur.Plunk;
using Facteur.TemplateProviders.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

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
        public void Plunk_SendMail_WithComposer_ShouldConstruct()
        {
            EmailComposer composer = new();
            IMailer mailer = new PlunkMailer("test-key", composer);
            Assert.IsNotNull(mailer);
        }

        [TestMethod]
        public void Plunk_SendMail_WithHttpClient_ShouldConstruct()
        {
            HttpClient httpClient = new();
            IMailer mailer = new PlunkMailer("test-key", null, httpClient);
            Assert.IsNotNull(mailer);
        }

        [TestMethod]
        public void Plunk_SendMail_WithBaseUrl_ShouldConstruct()
        {
            IMailer mailer = new PlunkMailer("test-key", null, null, "https://custom.plunk.com");
            Assert.IsNotNull(mailer);
        }

        [TestMethod]
        public async Task Plunk_SendMailAsync_WithBasicRequest_ShouldSend()
        {
            TestableHttpMessageHandler testHandler = new();
            HttpResponseMessage response = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\":true}")
            };
            testHandler.SetResponse(response);

            HttpRequestMessage? capturedRequest = null;
            testHandler.SetHandler(async (req, ct) =>
            {
                capturedRequest = req;
                return response;
            });

            HttpClient httpClient = new(testHandler);
            PlunkMailer mailer = new("test-api-key", null, httpClient);

            EmailRequest request = new()
            {
                Subject = "Test Subject",
                From = new Sender("from@example.com", "From Name"),
                To = ["to@example.com"],
                Body = "Test body"
            };

            await mailer.SendMailAsync(request);

            Assert.IsNotNull(capturedRequest);
            Assert.AreEqual(HttpMethod.Post, capturedRequest.Method);
            Assert.IsNotNull(capturedRequest.Headers.Authorization);
            Assert.AreEqual("Bearer", capturedRequest.Headers.Authorization.Scheme);
            Assert.AreEqual("test-api-key", capturedRequest.Headers.Authorization.Parameter);
        }

        [TestMethod]
        public async Task Plunk_SendMailAsync_WithCcAndBcc_ShouldCombineRecipients()
        {
            TestableHttpMessageHandler testHandler = new();
            string capturedContent = null;
            testHandler.SetHandler(async (req, ct) =>
            {
                capturedContent = await req.Content.ReadAsStringAsync(ct);
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"success\":true}")
                };
            });

            HttpClient httpClient = new(testHandler);
            PlunkMailer mailer = new("test-api-key", null, httpClient);

            EmailRequest request = new()
            {
                Subject = "Test Subject",
                From = new Sender("from@example.com", "From Name"),
                To = ["to@example.com"],
                Cc = ["cc@example.com"],
                Bcc = ["bcc@example.com"],
                Body = "Test body"
            };

            await mailer.SendMailAsync(request);

            Assert.IsNotNull(capturedContent);
            JsonDocument json = JsonDocument.Parse(capturedContent);
            JsonElement root = json.RootElement;
            
            // Plunk combines all recipients into "to" field
            Assert.IsTrue(root.TryGetProperty("to", out JsonElement toElement));
            // Should be an array with 3 recipients
            Assert.AreEqual(JsonValueKind.Array, toElement.ValueKind);
            Assert.AreEqual(3, toElement.GetArrayLength());
        }

        [TestMethod]
        public async Task Plunk_SendMailAsync_WithSingleRecipient_ShouldSendAsString()
        {
            TestableHttpMessageHandler testHandler = new();
            string capturedContent = null;
            testHandler.SetHandler(async (req, ct) =>
            {
                capturedContent = await req.Content.ReadAsStringAsync(ct);
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"success\":true}")
                };
            });

            HttpClient httpClient = new(testHandler);
            PlunkMailer mailer = new("test-api-key", null, httpClient);

            EmailRequest request = new()
            {
                Subject = "Test Subject",
                From = new Sender("from@example.com", "From Name"),
                To = ["to@example.com"],
                Body = "Test body"
            };

            await mailer.SendMailAsync(request);

            Assert.IsNotNull(capturedContent);
            JsonDocument json = JsonDocument.Parse(capturedContent);
            JsonElement root = json.RootElement;
            
            // Single recipient should be sent as string, not array
            Assert.IsTrue(root.TryGetProperty("to", out JsonElement toElement));
            Assert.AreEqual(JsonValueKind.String, toElement.ValueKind);
            Assert.AreEqual("to@example.com", toElement.GetString());
        }

        [TestMethod]
        public async Task Plunk_SendMailAsync_WithAttachments_ShouldIncludeAttachments()
        {
            TestableHttpMessageHandler testHandler = new();
            string capturedContent = null;
            testHandler.SetHandler(async (req, ct) =>
            {
                capturedContent = await req.Content.ReadAsStringAsync(ct);
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"success\":true}")
                };
            });

            HttpClient httpClient = new(testHandler);
            PlunkMailer mailer = new("test-api-key", null, httpClient);

            EmailRequest request = new()
            {
                Subject = "Test Subject",
                From = new Sender("from@example.com", "From Name"),
                To = ["to@example.com"],
                Body = "Test body",
                Attachments =
                [
                    new("test.txt", [1, 2, 3]),
                    new("test.pdf", [4, 5, 6])
                ]
            };

            await mailer.SendMailAsync(request);

            Assert.IsNotNull(capturedContent);
            JsonDocument json = JsonDocument.Parse(capturedContent);
            JsonElement root = json.RootElement;
            
            Assert.IsTrue(root.TryGetProperty("attachments", out JsonElement attachmentsElement));
            Assert.AreEqual(JsonValueKind.Array, attachmentsElement.ValueKind);
            Assert.AreEqual(2, attachmentsElement.GetArrayLength());
            
            JsonElement firstAttachment = attachmentsElement[0];
            Assert.AreEqual("test.txt", firstAttachment.GetProperty("filename").GetString());
            Assert.AreEqual("text/plain", firstAttachment.GetProperty("contentType").GetString());
        }

        [TestMethod]
        public async Task Plunk_SendMailAsync_WithComposer_ShouldCallComposer()
        {
            IEmailComposer mockComposer = Substitute.For<IEmailComposer>();
            EmailRequest expectedRequest = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"],
                Body = "Test body"
            };

            mockComposer.Subject(Arg.Any<string>()).Returns(mockComposer);
            mockComposer.From(Arg.Any<string>()).Returns(mockComposer);
            mockComposer.To(Arg.Any<string>()).Returns(mockComposer);
            mockComposer.BuildAsync().Returns(expectedRequest);

            TestableHttpMessageHandler testHandler = new();
            testHandler.SetResponse(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\":true}")
            });

            HttpClient httpClient = new(testHandler);
            PlunkMailer mailer = new("test-api-key", mockComposer, httpClient);

            await mailer.SendMailAsync(async composer => await composer
                .Subject("Test")
                .From("test@example.com")
                .To("recipient@example.com")
                .BuildAsync());

            await mockComposer.Received(1).BuildAsync();
        }

        [TestMethod]
        public async Task Plunk_SendMailAsync_WithCustomBaseUrl_ShouldUseCustomUrl()
        {
            TestableHttpMessageHandler testHandler = new();
            Uri? capturedUri = null;
            testHandler.SetHandler((req, ct) =>
            {
                capturedUri = req.RequestUri;
                return Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"success\":true}")
                });
            });

            HttpClient httpClient = new(testHandler);
            PlunkMailer mailer = new("test-api-key", null, httpClient, "https://custom.plunk.com/api");

            EmailRequest request = new()
            {
                Subject = "Test Subject",
                From = new Sender("from@example.com", "From Name"),
                To = ["to@example.com"],
                Body = "Test body"
            };

            await mailer.SendMailAsync(request);

            Assert.IsNotNull(capturedUri);
            Assert.AreEqual("https://custom.plunk.com/api/v1/send", capturedUri.ToString());
        }

        [TestMethod]
        public async Task Plunk_SendMailAsync_WithBaseUrlTrailingSlash_ShouldTrimSlash()
        {
            TestableHttpMessageHandler testHandler = new();
            Uri? capturedUri = null;
            testHandler.SetHandler((req, ct) =>
            {
                capturedUri = req.RequestUri;
                return Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"success\":true}")
                });
            });

            HttpClient httpClient = new(testHandler);
            PlunkMailer mailer = new("test-api-key", null, httpClient, "https://custom.plunk.com/api/");

            EmailRequest request = new()
            {
                Subject = "Test Subject",
                From = new Sender("from@example.com", "From Name"),
                To = ["to@example.com"],
                Body = "Test body"
            };

            await mailer.SendMailAsync(request);

            Assert.IsNotNull(capturedUri);
            Assert.AreEqual("https://custom.plunk.com/api/v1/send", capturedUri.ToString());
        }

        [TestMethod]
        [DataRow("test.pdf", "application/pdf")]
        [DataRow("test.txt", "text/plain")]
        [DataRow("test.html", "text/html")]
        [DataRow("test.htm", "text/html")]
        [DataRow("test.jpg", "image/jpeg")]
        [DataRow("test.jpeg", "image/jpeg")]
        [DataRow("test.png", "image/png")]
        [DataRow("test.gif", "image/gif")]
        [DataRow("test.doc", "application/msword")]
        [DataRow("test.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
        [DataRow("test.xls", "application/vnd.ms-excel")]
        [DataRow("test.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [DataRow("test.zip", "application/zip")]
        [DataRow("test.json", "application/json")]
        [DataRow("test.xml", "application/xml")]
        [DataRow("test.csv", "text/csv")]
        [DataRow("test.unknown", "application/octet-stream")]
        [DataRow("", "application/octet-stream")]
        public async Task Plunk_SendMailAsync_WithDifferentAttachmentTypes_ShouldSetCorrectContentType(string fileName, string expectedContentType)
        {
            TestableHttpMessageHandler testHandler = new();
            string? capturedContent = null;
            testHandler.SetHandler(async (req, ct) =>
            {
                capturedContent = await req.Content.ReadAsStringAsync(ct);
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"success\":true}")
                };
            });

            HttpClient httpClient = new(testHandler);
            PlunkMailer mailer = new("test-api-key", null, httpClient);

            EmailRequest request = new()
            {
                Subject = "Test Subject",
                From = new Sender("from@example.com", "From Name"),
                To = ["to@example.com"],
                Body = "Test body",
                Attachments = [new(fileName, [1, 2, 3])]
            };

            await mailer.SendMailAsync(request);

            Assert.IsNotNull(capturedContent);
            JsonDocument json = JsonDocument.Parse(capturedContent);
            JsonElement root = json.RootElement;
            
            Assert.IsTrue(root.TryGetProperty("attachments", out JsonElement attachmentsElement));
            Assert.AreEqual(JsonValueKind.Array, attachmentsElement.ValueKind);
            Assert.AreEqual(1, attachmentsElement.GetArrayLength());
            
            JsonElement attachment = attachmentsElement[0];
            Assert.AreEqual(expectedContentType, attachment.GetProperty("contentType").GetString());
        }

        [TestMethod]
        public async Task Plunk_SendMailAsync_WithNullFileName_ShouldSetDefaultContentType()
        {
            TestableHttpMessageHandler testHandler = new();
            string? capturedContent = null;
            testHandler.SetHandler(async (req, ct) =>
            {
                capturedContent = await req.Content.ReadAsStringAsync(ct);
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"success\":true}")
                };
            });

            HttpClient httpClient = new(testHandler);
            PlunkMailer mailer = new("test-api-key", null, httpClient);

            EmailRequest request = new()
            {
                Subject = "Test Subject",
                From = new Sender("from@example.com", "From Name"),
                To = ["to@example.com"],
                Body = "Test body",
                Attachments = [new(null, [1, 2, 3])]
            };

            await mailer.SendMailAsync(request);

            Assert.IsNotNull(capturedContent);
            JsonDocument json = JsonDocument.Parse(capturedContent);
            JsonElement root = json.RootElement;
            
            Assert.IsTrue(root.TryGetProperty("attachments", out JsonElement attachmentsElement));
            Assert.AreEqual(JsonValueKind.Array, attachmentsElement.ValueKind);
            Assert.AreEqual(1, attachmentsElement.GetArrayLength());
            
            JsonElement attachment = attachmentsElement[0];
            Assert.AreEqual("application/octet-stream", attachment.GetProperty("contentType").GetString());
        }
    }
}

