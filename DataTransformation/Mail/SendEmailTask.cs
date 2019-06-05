using System;
using Betlln.Data.Integration.Core;
using Betlln.Mail;
using MimeKit;

namespace Betlln.Data.Integration.Mail
{
    public class SendEmailTask : Task
    {
        internal SendEmailTask()
        {
        }

        public string To { get; set; }
        public string SenderDisplayName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public void AddAttachment(string filePath)
        {
            throw new NotImplementedException();
        }

        public IConnectionManager Connection { get; set; }

        protected override void ExecuteTasks()
        {
            if (string.IsNullOrWhiteSpace(To))
            {
                throw new ArgumentNullException(nameof(To));
            }
            if (string.IsNullOrWhiteSpace(Subject))
            {
                throw new ArgumentNullException(nameof(Subject));
            }

            EmailHostInfo emailHostInfo = (EmailHostInfo) Connection.GetConnection();
            SendEmail(emailHostInfo);
        }

        private void SendEmail(ConnectionInfo credentials)
        {
            MimeMessage message = new MimeMessage();
            message.Subject = Subject;

            string[] recipients = To.Split(',');
            foreach (string recipient in recipients)
            {
                message.To.Add(new MailboxAddress(recipient.Trim()));
            }

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = Body;
            message.Body = bodyBuilder.ToMessageBody();

            string displayName = !string.IsNullOrWhiteSpace(SenderDisplayName) ? SenderDisplayName : credentials.User;
            message.From.Add(new MailboxAddress(credentials.User) {Name = displayName});

            MailClient.Send(credentials.Destination, credentials.User, credentials.Password, message);
        }

        public static SendEmailTask GetUnattachedComponent()
        {
            return new SendEmailTask();
        }
    }
}