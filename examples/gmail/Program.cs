using System;
using System.Threading.Tasks;
using Facteur.Smtp;
using Facteur.TemplateProviders.IO;

namespace Facteur.Examples
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Sending welcome mail using a GMAIL account");

            // Edit these parameters to send an email
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
            // When using a gmail account: https://stackoverflow.com/a/25215834/1842261
            SmtpCredentials credentials = new("smtp.gmail.com", "587", "false", "true", from, fromPassword);

            // The composition:
            // - Compiler: Scriban template syntax,
            // - Provider files in this project's directory
            // - Template finder: finding templates in the app directory via the name of the view models
            EmailComposer composer = new(new ScribanCompiler(), new AppDirectoryTemplateProvider("Templates", ".sbnhtml"), new ViewModelTemplateResolver());

            IMailer mailer = new SmtpMailer(credentials, composer);
            await mailer.SendMailAsync(x => x
                .SetSubject("Welcome to the company!")
                .SetFrom("info@facteur.com")
                .SetTo(customerMail)
                .BuildAsync(new WelcomeMailModel { Email = customerMail, Name = customerName }));
        }
    }
}