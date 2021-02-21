using System;
using System.Threading.Tasks;
using Facteur.Smtp;
using Facteur.TemplateProviders.IO;

namespace Facteur.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Sending welcome mail using a GMAIL account");

            // Edit these parameters to send an e-mail
            const string from = "mygmail@gmail.com";
            const string fromPassword = "mygmailpassword";
            const string to = "newemployee@mycompany.com";
            const string name = "John Doe";

            await SendConfirmationMail(from, fromPassword, to, name);

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        private static async Task SendConfirmationMail(string from, string fromPassword, string customerMail, string customerName)
        {
            // Construct the e-mail model
            EmailComposer<WelcomeMailModel> composer = new EmailComposer<WelcomeMailModel>();
            EmailRequest<WelcomeMailModel> request = composer
                .SetModel(new WelcomeMailModel { Email = customerMail, Name = customerName })
                .SetSubject("Welcome to the company!")
                .SetFrom("info@facteur.com")
                .SetTo(customerMail)
                .Build();

            // Creating the body using the following components:
            // * Use Scriban as the template syntax
            // * Store templates in the project under the 'Templates' application directory
            // * Map e-mail templates by the mail model name (WelcomeMailModel -> Welcome.sbnhtml)
            IMailBodyBuilder builder = new MailBodyBuilder(
                new ScribanCompiler(),
                new AppDirectoryTemplateProvider("Templates", ".sbnhtml"),
                new ViewModelTemplateResolver());

            EmailRequest populatedRequest = await builder.BuildAsync(request);

            // When using a gmail account: https://stackoverflow.com/a/25215834/1842261
            SmtpCredentials credentials = new("smtp.gmail.com", "587", "false", "true", from, fromPassword);
            IMailer mailer = new SmtpMailer(credentials);
            await mailer.SendMailAsync(populatedRequest);
        }
    }
}
