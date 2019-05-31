using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.Email
{
    public class SendEmailTask : Task
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public void AddAttachment(string filePath)
        {
            throw new System.NotImplementedException();
        }

        public IConnectionManager Connection { get; set; }

        protected override void ExecuteTasks()
        {
            throw new System.NotImplementedException();
        }
    }
}