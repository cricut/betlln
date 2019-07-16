using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.Mail
{
    public class SendEmailTask : Task, IEmailSender
    {
        private readonly EmailSender _emailSender;

        internal SendEmailTask()
        {
            _emailSender = new EmailSender();
        }

        public string To
        {
            get { return _emailSender.To; }
            set { _emailSender.To = value; }
        }

        public string SenderDisplayName
        {
            get { return _emailSender.SenderDisplayName; }
            set { _emailSender.SenderDisplayName = value; }
        }

        public string Subject
        {
            get { return _emailSender.Subject; }
            set { _emailSender.Subject = value; }
        }

        public string Body
        {
            get { return _emailSender.Body; }
            set { _emailSender.Body = value; }
        }

        public void AddAttachment(string filePath)
        {
            _emailSender.AddAttachment(filePath);
        }

        public void AddAttachment(NamedStream namedStream)
        {
            _emailSender.AddAttachment(namedStream);
        }

        public IConnectionManager Connection
        {
            get { return _emailSender.Connection; }
            set { _emailSender.Connection = value; }
        }

        protected override void ExecuteTasks()
        {
            _emailSender.Send();
        }
    }
}