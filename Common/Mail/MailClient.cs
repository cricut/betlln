using System;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Betlln.Mail
{
    public class MailClient : IMailClient, IEmailOptions
    {
        public const int DefaultSmtpPort = 587;
        public const int DefaultImapPort = 993;

        private IMailClient _client;

        private string Address { get; set; }
        public int ImapPortNumber { get; set; }
        public int SmtpPortNumber { get; set; }
        private string UserName { get; set; }
        private string Password { get; set; }
        public bool RequireSSL { get; set; }

        public void Connect(string address, int imapPortNumber = DefaultImapPort, int smtpPortNumber = DefaultSmtpPort, bool requireSsl = true)
        {
            Address = address;
            ImapPortNumber = imapPortNumber;
            SmtpPortNumber = smtpPortNumber;
            RequireSSL = requireSsl;
        }

        public void Login(string username, string password)
        {
            UserName = username;
            Password = password;
        }

        public IMailFolder GetFolder(string folderName)
        {
            return GetClient<ImapMailClient>().GetFolder(folderName);
        }

        public void Send(MimeMessage message)
        {
            GetClient<SmtpMailClient>().Send(message);
        }

        private IMailClient GetClient<T>()
            where T : IMailClient, new()
        {
            if (_client == null)
            {
                PrepareClient<T>();
            }
            else if (_client.GetType() != typeof(T))
            {
                Dispose();
                PrepareClient<T>();
            }

            return _client;
        }

        private void PrepareClient<T>() where T : IMailClient, new()
        {
            _client = new T();
            _client.Connect(Address, ImapPortNumber, SmtpPortNumber, RequireSSL);
            _client.Login(UserName, Password);
        }

        public void Dispose()
        {
            _client?.Dispose();
            _client = null;
        }

        private class ImapMailClient : IMailClient
        {
            private readonly ImapClient _imapClient;

            public ImapMailClient()
            {
                _imapClient = new ImapClient();
            }

            public void Connect(string address, int imapPortNumber = DefaultImapPort, int smtpPortNumber = DefaultSmtpPort, bool requireSsl = true)
            {
                _imapClient.Connect(address, imapPortNumber, requireSsl);
            }

            public void Login(string username, string password)
            {
                // ReSharper disable once StringLiteralTypo
                _imapClient.AuthenticationMechanisms.Remove("XOAUTH2");
                _imapClient.Authenticate(username, password);
            }

            public IMailFolder GetFolder(string folderName)
            {
                return _imapClient.GetFolder(folderName);
            }

            public void Send(MimeMessage message)
            {
                throw new NotSupportedException();
            }

            public void Dispose()
            {
                _imapClient.Disconnect(true);
            }
        }

        private class SmtpMailClient : IMailClient
        {
            private readonly SmtpClient _smtpClient;

            public SmtpMailClient()
            {
                _smtpClient = new SmtpClient();
            }

            public void Connect(string address, int imapPortNumber = DefaultImapPort, int smtpPortNumber = DefaultSmtpPort, bool requireSsl = true)
            {
                SecureSocketOptions secureSocketOptions = SecureSocketOptions.StartTls;
                if (!requireSsl)
                {
                    secureSocketOptions = SecureSocketOptions.Auto;
                    _smtpClient.ServerCertificateValidationCallback = (s,c,h,e) => true;
                }
                
                _smtpClient.Connect(address, smtpPortNumber, secureSocketOptions);
            }

            public void Login(string username, string password)
            {
                // ReSharper disable once StringLiteralTypo
                _smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
                _smtpClient.Authenticate(username, password);
            }

            public IMailFolder GetFolder(string folderName)
            {
                throw new NotSupportedException();
            }

            public void Send(MimeMessage message)
            {
                _smtpClient.Send(message);
            }

            public void Dispose()
            {
                _smtpClient.Disconnect(true);
            }
        }

    }
}