using System.Net.Mail;

namespace Facteur.Smtp
{
    internal static class ToMailAddressConverter
    {
        internal static MailAddress ToMailAddress(this Sender sender)
            => new(sender.Email, sender.Name);
    }
}