using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RequestsAccess.Services
{
    public class MailConfig
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string SmtpHost { get; set; }

        public int SmtpPort { get; set; }
    }
}
