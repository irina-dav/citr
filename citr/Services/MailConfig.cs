namespace citr.Services
{
    public class MailConfig
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string SmtpHost { get; set; }

        public int SmtpPort { get; set; }

        public string FromEmail { get; set; }

        public string OTRSEmail { get; set; }
    }
}
