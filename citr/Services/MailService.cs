using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace citr.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(string from, string to, string subject, string message);
    }

    public class MailService : IMailService
    {
        private readonly MailConfig config;
        ILogger<LdapService> logger;

        public MailService(IOptions<MailConfig> cfg, ILogger<LdapService> logger)
        {
            config = cfg.Value;
            this.logger = logger;
        }


        public async Task SendEmailAsync(string from, string to, string subject, string message)
        {
            from = config.FromEmail;        // TEMP
            try
            {
                var emailMessage = new MimeMessage()
                {
                    Subject = subject,
                    Body = new TextPart(MimeKit.Text.TextFormat.Html)
                    {
                        Text = message
                    }
                };

                emailMessage.From.Add(new MailboxAddress(from));
                emailMessage.To.Add(new MailboxAddress(to));

                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, d, e, f) => true;
                    await client.ConnectAsync(config.SmtpHost, config.SmtpPort, false);
                    await client.AuthenticateAsync(config.Email, config.Password);
                    await client.SendAsync(emailMessage);

                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }
    }
}
