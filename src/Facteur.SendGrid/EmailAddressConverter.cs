using SendGrid.Helpers.Mail;

namespace Facteur.SendGrid
{
    internal static class EmailAddressConverter
    {
        internal static EmailAddress ToEmailAddress(this Sender sender)
        => new(sender.Email, sender.Name);
    }
}