using System;

namespace Betlln.Mail
{
    [Obsolete("Use " + nameof(IEmailOptions) + " instead.")]
    public interface IEmailPorts
    {
        int ImapPortNumber { get; set; }
        int SmtpPortNumber { get; set; }
    }
}