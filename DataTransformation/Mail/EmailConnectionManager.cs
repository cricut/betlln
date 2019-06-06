using System;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.Mail
{
    public class EmailConnectionManager : IConnectionManager
    {
        public EmailConnectionManager()
        {
        }

        public EmailConnectionManager(ConnectionInfo connectionInfo)
        {
            Host = connectionInfo.Destination;
            User = connectionInfo.User;
            Password = connectionInfo.Password;
        }

        public string Host { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public IDisposable GetConnection()
        {
            return new EmailHostInfo(Host, User, Password);
        }
    }
}