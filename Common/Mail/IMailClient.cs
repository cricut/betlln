using System;
using MailKit;
using MimeKit;

namespace Betlln.Mail
{
    public interface IMailClient : IDisposable
    {
        void Connect(string address, int imapPortNumber = MailClient.DefaultImapPort, int smtpPortNumber = MailClient.DefaultSmtpPort, bool requireSsl = true);
        void Login(string username, string password);
        IMailFolder GetFolder(string folderName);
        void Send(MimeMessage message);
    }
}