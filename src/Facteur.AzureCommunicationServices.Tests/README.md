# Facteur.AzureCommunicationServices Tests

This project contains unit and integration tests for the Facteur Azure Communication Services endpoint.

## Unit Tests

Unit tests can be run without any setup and will execute automatically:

```bash
dotnet test
```

These tests verify the behavior of the mailer classes without actually sending emails.

## Integration Tests

Integration tests are marked with `[Ignore]` and require a valid Azure Communication Services setup to run.

### Prerequisites

1. An Azure Communication Services resource with Email enabled
2. A verified domain and sender address
3. Connection string from your Azure Communication Services resource

### Setup

#### Option 1: Using .env file (Recommended)

1. Copy the `.env.example` file to `.env`:
   ```bash
   cp .env.example .env
   ```

2. Edit the `.env` file and fill in your actual values:
   ```env
   # Required
   COMMUNICATION_SERVICES_CONNECTION_STRING=endpoint=https://your-resource.communication.azure.com/;accesskey=your-key
   ACS_SENDER_ADDRESS=donotreply@xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx.azurecomm.net
   ACS_RECIPIENT_ADDRESS=your-test-email@example.com

   # Optional (for CC/BCC tests)
   ACS_CC_ADDRESS=cc-recipient@example.com
   ACS_BCC_ADDRESS=bcc-recipient@example.com
   ACS_RECIPIENT_ADDRESS_2=second-recipient@example.com
   ```

3. The `.env` file is automatically loaded when running tests and is ignored by git to keep your secrets safe.

#### Option 2: Using Environment Variables

Set the following environment variables before running integration tests:

```bash
# Required
export COMMUNICATION_SERVICES_CONNECTION_STRING="endpoint=https://your-resource.communication.azure.com/;accesskey=your-key"
export ACS_SENDER_ADDRESS="donotreply@xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx.azurecomm.net"
export ACS_RECIPIENT_ADDRESS="your-test-email@example.com"

# Optional (for CC/BCC tests)
export ACS_CC_ADDRESS="cc-recipient@example.com"
export ACS_BCC_ADDRESS="bcc-recipient@example.com"
export ACS_RECIPIENT_ADDRESS_2="second-recipient@example.com"
```

### Running Integration Tests

To run integration tests, remove the `[Ignore]` attribute from the `AzureCommunicationServicesIntegrationTests` class, or run specific tests using:

```bash
# Run all tests including integration tests
dotnet test --filter "TestCategory=Integration"

# Run a specific integration test
dotnet test --filter "FullyQualifiedName~SendSimpleHtmlEmail_ShouldSucceed"
```

Alternatively, in Visual Studio or Rider, right-click on a specific test method and select "Run Test".

### What the Integration Tests Cover

- **SendSimpleHtmlEmail_ShouldSucceed**: Sends a basic HTML email
- **SendPlainTextEmail_ShouldSucceed**: Sends a plain text email
- **SendEmailWithCcAndBcc_ShouldSucceed**: Tests CC and BCC functionality
- **SendEmailWithAttachment_ShouldSucceed**: Tests single attachment
- **SendEmailWithMultipleAttachments_ShouldSucceed**: Tests multiple attachments
- **SendEmailUsingComposer_ShouldSucceed**: Tests the composer pattern
- **SendEmailWithMultipleRecipients_ShouldSucceed**: Tests multiple To recipients

## Notes

- Integration tests will actually send emails to the configured addresses
- Make sure to use test email addresses that you control
- Azure Communication Services may have rate limits for sending emails
- Check your Azure portal for email delivery status and logs
