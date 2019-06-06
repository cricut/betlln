using System;

namespace Betlln.Data.Integration.Mail
{
    internal class EmailHostInfo : ConnectionInfo, IDisposable
    {
        public EmailHostInfo(string address, string user, string password)
        {
            Destination = address;
            User = user;
            Password = password;
        }

        public void Dispose()
        {
        }
    }
}