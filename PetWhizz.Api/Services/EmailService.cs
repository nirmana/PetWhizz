using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;

namespace PetWhizz.Api.Services
{
    public class EmailService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        String SmtpServer, EmailFrom, EmailPassword = "";
        bool EnableSsl;
        int Port;
        public EmailService()
        {
            SmtpServer = Common.Instance.SmtpServer;
            Port = Common.Instance.Port;
            EmailFrom = Common.Instance.EmailFrom;
            EmailPassword = Common.Instance.EmailPassword;
            EnableSsl = Common.Instance.EnableSsl;
        }
      

        public bool SendEmail(MailMessage email)
        {
            logger.Debug("Recived send email request for address - " + email.To + " Subject - " + email.Subject +
                " content - " + email.Body);
            bool status = false;
            try
            {
                using (SmtpClient smtp = new SmtpClient(SmtpServer, Port))
                {
                    smtp.Credentials = new NetworkCredential(EmailFrom, EmailPassword);
                    smtp.EnableSsl = EnableSsl;
                    smtp.Send(email);
                }
            }
            catch (Exception)
            {
                logger.Error("Email sending failed for address - " + email.To + " Subject - " + email.Subject +
                " content - " + email.Body);
                throw;
            }
            logger.Debug("Email successfully sent for address - " + email.To);
            return status;
        }

        public AlternateView EmbedLogosForEmailBody(String BodyHtml)
        {
            LinkedResource PetwhizzLogo = new LinkedResource(HttpContext.Current.Server.MapPath(Common.Instance.ImagesPath+"PetWhizzLogo_25X25.png"));
            PetwhizzLogo.ContentId = Guid.NewGuid().ToString();
            BodyHtml = BodyHtml.Replace("{PetWhizzLogo}", "cid:" + PetwhizzLogo.ContentId);
            var view = AlternateView.CreateAlternateViewFromString(BodyHtml, null, "text/html");
            view.LinkedResources.Add(PetwhizzLogo);
            return view;
        }

        internal string GetBodyHtml(string EmailType)
        {
            switch (EmailType)
            {
                case "EMAILVERIFICATION":
                    return File.ReadAllText(HttpContext.Current.Server.MapPath(Common.Instance.EmailTemplatesPath+"EmailVerification.html"));
                case "PASSWORDRESET":
                    return File.ReadAllText(HttpContext.Current.Server.MapPath(Common.Instance.EmailTemplatesPath + "ResetPassword.html"));
                case "PETSHARING":
                    return File.ReadAllText(HttpContext.Current.Server.MapPath(Common.Instance.EmailTemplatesPath + "SharePet.html"));
                case "INVITEUSER":
                    return File.ReadAllText(HttpContext.Current.Server.MapPath(Common.Instance.EmailTemplatesPath + "UserInvitation.html"));
                    
                default:
                    return "";
            }
        }
    }
}