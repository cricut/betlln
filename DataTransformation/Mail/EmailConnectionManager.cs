using System;
using Betlln.Data.Integration.Core;
using Betlln.Mail;

namespace Betlln.Data.Integration.Mail
{
    public class EmailConnectionManager : IConnectionManager, IEmailOptions
    {
        public EmailConnectionManager()
        {
            ImapPortNumber = MailClient.DefaultImapPort;
            SmtpPortNumber = MailClient.DefaultSmtpPort;
        }

        public EmailConnectionManager(ConnectionInfo connectionInfo) : this()
        {
            Host = connectionInfo.Destination;
            User = connectionInfo.User;
            Password = connectionInfo.Password;
        }

        public string Host { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public int ImapPortNumber { get; set; }
        public int SmtpPortNumber { get; set; }
        public bool RequireSSL { get; set; }

        public IDisposable GetConnection()
        {
            EmailHostInfo emailHostInfo = new EmailHostInfo(Host, User, Password);
            emailHostInfo.ImapPortNumber = ImapPortNumber;
            emailHostInfo.SmtpPortNumber = SmtpPortNumber;
            emailHostInfo.RequireSSL = RequireSSL;
            return emailHostInfo;
        }

        public Type GetDataAdapterType()
        {
            throw new NotSupportedException();
        }
    }
}