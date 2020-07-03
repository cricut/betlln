namespace Betlln.Data.Integration
{
    internal interface IDatabaseConnection
    {
        string ServerName { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        string DatabaseName { get; set; }
        string ApplicationName { get; set; }
    }
}