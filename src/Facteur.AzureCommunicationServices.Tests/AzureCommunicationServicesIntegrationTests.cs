using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using DotNetEnv;
using Facteur;
using Facteur.AzureCommunicationServices;
using Facteur.TemplateProviders.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Tests
{
    /// <summary>
    /// Integration tests for Azure Communication Services.
    /// These tests require a valid connection string and sender address.
    /// Create a .env file in the test project directory with the required variables.
    /// See .env.example for the list of required variables.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    //[Ignore("Integration tests - run manually with valid connection string")]
    public class AzureCommunicationServicesIntegrationTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            // Try to load .env file from the test project directory
            string envPath = Path.Combine(AppContext.BaseDirectory, ".env");
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
            }
            else
            {
                // Try to load from parent directories
                string currentDir = Directory.GetCurrentDirectory();
                string[] searchPaths = new[]
                {
                    currentDir,
                    Path.GetDirectoryName(currentDir),
                    Path.GetDirectoryName(Path.GetDirectoryName(currentDir))
                };

                foreach (string searchPath in searchPaths)
                {
                    envPath = Path.Combine(searchPath, ".env");
                    if (File.Exists(envPath))
                    {
                        Env.Load(envPath);
                        break;
                    }
                }
            }
        }

        private string GetConnectionString()
        {
            string connectionString = Environment.GetEnvironmentVariable("COMMUNICATION_SERVICES_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                Assert.Inconclusive("COMMUNICATION_SERVICES_CONNECTION_STRING environment variable not set. " +
                    "Set it to your Azure Communication Services connection string to run integration tests.");
            }
            return connectionString;
        }

        private string GetSenderAddress()
        {
            string senderAddress = Environment.GetEnvironmentVariable("ACS_SENDER_ADDRESS");
            if (string.IsNullOrEmpty(senderAddress))
            {
                Assert.Inconclusive("ACS_SENDER_ADDRESS environment variable not set. " +
                    "Set it to your verified sender address (e.g., donotreply@xxxxxxxx.azurecomm.net) to run integration tests.");
            }
            return senderAddress;
        }

        private string GetRecipientAddress()
        {
            string recipientAddress = Environment.GetEnvironmentVariable("ACS_RECIPIENT_ADDRESS");
            if (string.IsNullOrEmpty(recipientAddress))
            {
                Assert.Inconclusive("ACS_RECIPIENT_ADDRESS environment variable not set. " +
                    "Set it to a valid recipient email address to run integration tests.");
            }
            return recipientAddress;
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SendSimpleHtmlEmail_ShouldSucceed()
        {
            // Arrange
            string connectionString = GetConnectionString();
            string senderAddress = GetSenderAddress();
            string recipientAddress = GetRecipientAddress();

            IMailer mailer = new AzureCommunicationServicesMailer(connectionString);

            EmailRequest request = new()
            {
                Subject = "Test Email from Facteur - Azure Communication Services",
                From = new Sender(senderAddress, "Facteur Test"),
                To = [recipientAddress],
                Body = "<html><body><h1>Hello from Facteur!</h1><p>This is a test email sent using the Facteur Azure Communication Services endpoint.</p></body></html>"
            };

            // Act
            await mailer.SendMailAsync(request);

            // Assert - if we get here without exception, the test passed
            Assert.IsTrue(true, "Email sent successfully");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SendPlainTextEmail_ShouldSucceed()
        {
            // Arrange
            string connectionString = GetConnectionString();
            string senderAddress = GetSenderAddress();
            string recipientAddress = GetRecipientAddress();

            IMailer mailer = new AzureCommunicationServicesPlainTextMailer(connectionString);

            EmailRequest request = new()
            {
                Subject = "Test Plain Text Email from Facteur - Azure Communication Services",
                From = new Sender(senderAddress, "Facteur Test"),
                To = [recipientAddress],
                Body = "Hello from Facteur!\n\nThis is a plain text test email sent using the Facteur Azure Communication Services endpoint."
            };

            // Act
            await mailer.SendMailAsync(request);

            // Assert
            Assert.IsTrue(true, "Plain text email sent successfully");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SendEmailWithCcAndBcc_ShouldSucceed()
        {
            // Arrange
            string connectionString = GetConnectionString();
            string senderAddress = GetSenderAddress();
            string recipientAddress = GetRecipientAddress();

            IMailer mailer = new AzureCommunicationServicesMailer(connectionString);

            EmailRequest request = new()
            {
                Subject = "Test Email with CC and BCC - Azure Communication Services",
                From = new Sender(senderAddress, "Facteur Test"),
                To = [recipientAddress],
                Cc = [Environment.GetEnvironmentVariable("ACS_CC_ADDRESS") ?? recipientAddress],
                Bcc = [Environment.GetEnvironmentVariable("ACS_BCC_ADDRESS") ?? recipientAddress],
                Body = "<html><body><h1>Hello from Facteur!</h1><p>This email includes CC and BCC recipients.</p></body></html>"
            };

            // Act
            await mailer.SendMailAsync(request);

            // Assert
            Assert.IsTrue(true, "Email with CC and BCC sent successfully");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SendEmailWithAttachment_ShouldSucceed()
        {
            // Arrange
            string connectionString = GetConnectionString();
            string senderAddress = GetSenderAddress();
            string recipientAddress = GetRecipientAddress();

            IMailer mailer = new AzureCommunicationServicesMailer(connectionString);

            // Create a simple text file as attachment
            byte[] attachmentContent = System.Text.Encoding.UTF8.GetBytes("This is a test attachment file.\nCreated by Facteur integration test.");

            EmailRequest request = new()
            {
                Subject = "Test Email with Attachment - Azure Communication Services",
                From = new Sender(senderAddress, "Facteur Test"),
                To = [recipientAddress],
                Body = "<html><body><h1>Hello from Facteur!</h1><p>This email includes an attachment.</p></body></html>",
                Attachments =
                [
                    new Attachment("test-document.txt", attachmentContent)
                ]
            };

            // Act
            await mailer.SendMailAsync(request);

            // Assert
            Assert.IsTrue(true, "Email with attachment sent successfully");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SendEmailWithMultipleAttachments_ShouldSucceed()
        {
            // Arrange
            string connectionString = GetConnectionString();
            string senderAddress = GetSenderAddress();
            string recipientAddress = GetRecipientAddress();

            IMailer mailer = new AzureCommunicationServicesMailer(connectionString);

            // Create multiple attachments
            byte[] attachment1 = System.Text.Encoding.UTF8.GetBytes("First attachment content");
            byte[] attachment2 = System.Text.Encoding.UTF8.GetBytes("Second attachment content");

            EmailRequest request = new()
            {
                Subject = "Test Email with Multiple Attachments - Azure Communication Services",
                From = new Sender(senderAddress, "Facteur Test"),
                To = [recipientAddress],
                Body = "<html><body><h1>Hello from Facteur!</h1><p>This email includes multiple attachments.</p></body></html>",
                Attachments =
                [
                    new Attachment("document1.txt", attachment1),
                    new Attachment("document2.txt", attachment2)
                ]
            };

            // Act
            await mailer.SendMailAsync(request);

            // Assert
            Assert.IsTrue(true, "Email with multiple attachments sent successfully");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SendEmailUsingComposer_ShouldSucceed()
        {
            // Arrange
            string connectionString = GetConnectionString();
            string senderAddress = GetSenderAddress();
            string recipientAddress = GetRecipientAddress();

            IMailer mailer = new AzureCommunicationServicesMailer(connectionString);

            // Act
            await mailer.SendMailAsync(async composer => await composer
                .Subject("Test Email using Composer - Azure Communication Services")
                .From(senderAddress)
                .To(recipientAddress)
                .Body("<html><body><h1>Hello from Facteur!</h1><p>This email was sent using the composer pattern.</p></body></html>")
                .BuildAsync());
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SendEmailWithMultipleRecipients_ShouldSucceed()
        {
            // Arrange
            string connectionString = GetConnectionString();
            string senderAddress = GetSenderAddress();
            string recipientAddress = GetRecipientAddress();

            IMailer mailer = new AzureCommunicationServicesMailer(connectionString);

            EmailRequest request = new()
            {
                Subject = "Test Email with Multiple Recipients - Azure Communication Services",
                From = new Sender(senderAddress, "Facteur Test"),
                To = [recipientAddress, Environment.GetEnvironmentVariable("ACS_RECIPIENT_ADDRESS_2") ?? recipientAddress],
                Body = "<html><body><h1>Hello from Facteur!</h1><p>This email is sent to multiple recipients.</p></body></html>"
            };

            // Act
            await mailer.SendMailAsync(request);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SendEmailWithAdvancedScribanTemplate_ShouldSucceed()
        {
            // Arrange
            string connectionString = GetConnectionString();
            string senderAddress = GetSenderAddress();
            string recipientAddress = GetRecipientAddress();

            // Create a rich view model with sample order data
            OrderConfirmationMailModel model = new()
            {
                CustomerName = "John Doe",
                CustomerEmail = recipientAddress,
                OrderNumber = "ORD-2025-001234",
                OrderDate = DateTime.Now.AddHours(-2),
                EstimatedDeliveryDate = DateTime.Now.AddDays(5),
                Subtotal = 299.97m,
                Tax = 24.00m,
                ShippingCost = 0m, // Free shipping
                Total = 323.97m,
                ShippingAddress = "123 Main Street, Apt 4B",
                ShippingCity = "San Francisco",
                ShippingState = "CA",
                ShippingZipCode = "94102",
                ShippingCountry = "United States",
                TrackingUrl = "https://example.com/track/ORD-2025-001234",
                IsPriorityShipping = true,
                DiscountCode = "WELCOME20",
                DiscountAmount = 59.99m,
                Items = new()
                {
                    new OrderItem
                    {
                        ProductName = "Wireless Bluetooth Headphones",
                        ProductSku = "WBH-001",
                        Quantity = 1,
                        UnitPrice = 129.99m,
                        TotalPrice = 129.99m,
                        ImageUrl = "https://via.placeholder.com/80"
                    },
                    new OrderItem
                    {
                        ProductName = "USB-C Fast Charger",
                        ProductSku = "USBC-CHRG-45W",
                        Quantity = 2,
                        UnitPrice = 34.99m,
                        TotalPrice = 69.98m,
                        ImageUrl = "https://via.placeholder.com/80"
                    },
                    new OrderItem
                    {
                        ProductName = "Phone Case - Premium Leather",
                        ProductSku = "PC-LTHR-BLK",
                        Quantity = 1,
                        UnitPrice = 49.99m,
                        TotalPrice = 49.99m,
                        ImageUrl = "https://via.placeholder.com/80"
                    },
                    new OrderItem
                    {
                        ProductName = "Screen Protector (3-Pack)",
                        ProductSku = "SP-GLASS-3PK",
                        Quantity = 1,
                        UnitPrice = 19.99m,
                        TotalPrice = 19.99m
                        // No image URL to test fallback
                    }
                }
            };

            // Create email composer with Scriban compiler and template provider
            EmailComposer composer = new(
                new ScribanCompiler(),
                new AppDirectoryTemplateProvider("Templates", ".sbnhtml"),
                new ViewModelTemplateResolver());

            IMailer mailer = new AzureCommunicationServicesMailer(connectionString, composer);

            // Act
            await mailer.SendMailAsync(async c => await c
                .Subject($"Order Confirmation - {model.OrderNumber}")
                .From(senderAddress, "Your Store Name")
                .To(recipientAddress)
                .BuildAsync(model));
        }
    }
}