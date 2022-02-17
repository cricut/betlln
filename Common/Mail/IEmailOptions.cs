namespace Betlln.Mail
{
    public interface IEmailOptions : IEmailPorts
    {
        bool RequireSSL { get; set; }
    }
}