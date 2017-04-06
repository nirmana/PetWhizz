using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace PetWhizz.Api
{
    public sealed class Common
    {
        private static readonly Lazy<Common> lazy = new Lazy<Common>(() => new Common());
        public static Common Instance { get { return lazy.Value; } }

        public String EncryptionKey { get; set; }
        public Double TokenExpiryTime { get; set; }
        public Double VerificationCodeExpiryTime { get; set; }
        public String SmtpServer { get; set; }
        public int Port { get; set; }
        public String EmailFrom { get; set; }
        public String EmailPassword { get; set; }
        public Boolean EnableSsl { get; set; }
        public String EmailTemplatesPath { get; set; }
        public String ImagesPath { get; set; }
        public Double ResetPasswordExpiryTime { get; set; }
        public String HostedBaseUrl { get; set; }
        private Common()
        {
            EncryptionKey=ConfigurationManager.AppSettings["EncryptionKey"];
            TokenExpiryTime = Double.Parse(ConfigurationManager.AppSettings["TokenExpiryTime"]);
            VerificationCodeExpiryTime = Double.Parse(ConfigurationManager.AppSettings["VerificationCodeExpiryTime"]);
            SmtpServer = ConfigurationManager.AppSettings["SmtpServer"];
            Port = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            EmailFrom = ConfigurationManager.AppSettings["EmailFrom"];
            EmailPassword = ConfigurationManager.AppSettings["EmailPassword"];
            EnableSsl = Boolean.Parse(ConfigurationManager.AppSettings["EnableSsl"]);
            EmailTemplatesPath = ConfigurationManager.AppSettings["EmailTemplatesPath"];
            ImagesPath = ConfigurationManager.AppSettings["ImagesPath"];
            ResetPasswordExpiryTime = Double.Parse(ConfigurationManager.AppSettings["ResetPasswordExpiryTime"]);
            HostedBaseUrl = ConfigurationManager.AppSettings["HostedBaseUrl"];

        }
    }
}