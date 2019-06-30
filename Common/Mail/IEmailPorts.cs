namespace Betlln.Mail
{
    public interface IEmailPorts
    {
        int ImapPortNumber { get; set; }
        int SmtpPortNumber { get; set; }
    }
}