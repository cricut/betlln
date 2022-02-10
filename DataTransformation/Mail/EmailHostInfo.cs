using System;
using Betlln.Mail;

namespace Betlln.Data.Integration.Mail
{
    internal class EmailHostInfo : ConnectionInfo, IDisposable, IEmailOptions
    {
        public EmailHostInfo(string address, string user, string password)
        {
            Destination = address;
            User = user;
            Password = password;
        }

        public int ImapPortNumber { get; set; }
        public int SmtpPortNumber { get; set; }
        public bool RequireSSL { get; set; }

        public void Dispose()
        {
        }
    }
}