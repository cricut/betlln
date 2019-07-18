using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.Mail
{
    internal interface IEmailSender
    {
        IConnectionManager Connection { get; set; }
        string To { get; set; }
        string SenderDisplayName { get; set; }
        string Subject { get; set; }
        string Body { get; set; }
        void AddAttachment(string filePath);
        void AddAttachment(NamedStream namedStream);
    }
}