using Facteur.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Facteur;
using Facteur.TemplateProviders.IO;
using Facteur.Tests;
using System.Threading.Tasks;

namespace Facteur.Extensions.DependencyInjection.Tests
{
    [TestClass]
    public class ServiceCollectionTests
    {
        [TestMethod]
        public async Task ServiceCollection_DI_ShouldConstructAndSendMail()
        {
            EmailComposer<TestMailModel> composer = new EmailComposer<TestMailModel>();
            EmailRequest<TestMailModel> request = composer
                .SetModel(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" })
                .SetSubject("Hello world")
                .SetFrom("info@facteur.com")
                .SetTo("tibipi@getnada.com")
                .SetCc("tibipi@getnada.com")
                .SetBcc("tibipi@getnada.com")
                .Build();

            IMailer mailer = GetMailer();
            await mailer.SendMailAsync(request);
        }

        private static IMailer GetMailer()
        {
            SmtpCredentials credentials = new("smtp.mailtrap.io", "587", "false", "true", "d3538ae47a016d", "d4add3690c408c");

            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddMailer<SmtpMailer, ScribanCompiler, AppDirectoryTemplateProvider, ViewModelTemplateResolver>(
                mailerFactory: x => new SmtpMailer(credentials),
                templateProviderFactory: x => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"));

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider.GetService<IMailer>();
        }
    }
}
