using System;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Betlln.Mail
{
    public class MailClient : IMailClient
    {
        private IMailClient _client;

        private string Address { get; set; }
        private string UserName { get; set; }
        private string Password { get; set; }

        public void Connect(string address)
        {
            Address = address;
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
            _client.Connect(Address);
            _client.Login(UserName, Password);
        }

        public void Dispose()
        {
            _client?.Dispose();
            _client = null;
        }

        public static void Send(string address, string username, string password, MimeMessage message)
        {
            using (IMailClient client = new MailClient())
            {
                client.Connect(address);
                client.Login(username, password);
                client.Send(message);
            }
        }

        private class ImapMailClient : IMailClient
        {
            private readonly ImapClient _imapClient;

            public ImapMailClient()
            {
                _imapClient = new ImapClient();
            }

            public void Connect(string address)
            {
                _imapClient.Connect(address, 993, true);
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

            public void Connect(string address)
            {
                _smtpClient.Connect(address, 587, SecureSocketOptions.StartTls);
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