using System.Net;
using System.Net.Mail;

namespace Kiss.Bff.Config
{
    public static class EmailConfig
    {
        public static IServiceCollection AddSmtpClient(this IServiceCollection services, string host, int port, string username, string password, bool enableSsl) => services.AddSingleton(new SmtpClient
        {
            Host = host,
            Port = port,
            Credentials = new NetworkCredential(username, password),
            EnableSsl = enableSsl
        });
    }
}
