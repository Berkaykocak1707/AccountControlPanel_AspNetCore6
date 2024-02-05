using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class EmailSettings
    {
        public string SmtpHost
        {
            get; set;
        }
        public int SmtpPort
        {
            get; set;
        }
        public string SmtpUser
        {
            get; set;
        }
        public string SmtpPass
        {
            get; set;
        }
        public string FromAddress
        {
            get; set;
        }
        public EmailSettings()
        {
            SmtpHost = "default.smtp.host";
            SmtpPort = 25;
            SmtpUser = "defaultUser";
            SmtpPass = "defaultPassword";
            FromAddress = "default@address.com";
        }
    }
}
