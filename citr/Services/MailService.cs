﻿using MailKit.Net.Smtp;
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
        Task SendEmailAsync(string email, string subject, string message);
    }

    public class MailService : IMailService
    {
        private readonly MailConfig config;

        public MailService(IOptions<MailConfig> cfg)
        {
            config = cfg.Value;           
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage()
            {
                Subject = subject,
                Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = message
                }
            };

            emailMessage.From.Add(new MailboxAddress(email));
            emailMessage.To.Add(new MailboxAddress(email));

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, d, e, f) => true;
                await client.ConnectAsync(config.SmtpHost, config.SmtpPort, false);
                await client.AuthenticateAsync(config.Email, config.Password);
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }
    }
}
