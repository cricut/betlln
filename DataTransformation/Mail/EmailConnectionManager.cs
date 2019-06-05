using System;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.Mail
{
    public class EmailConnectionManager : IConnectionManager
    {
        public EmailConnectionManager()
        {
        }

        public EmailConnectionManager(string hostAddress, ConnectionInfo loginInfo)
        {
            throw new NotImplementedException();
        }

        public IDisposable GetConnection()
        {
            throw new NotImplementedException();
        }
    }
}