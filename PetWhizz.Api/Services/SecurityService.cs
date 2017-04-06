using NLog;
using PetWhizz.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

namespace PetWhizz.Api.Services
{
    public class SecurityService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public SecurityService()
        {

        }

        internal bool ValidateAuthToken(string Token)
        {
            bool status = false;
            try
            {

                using (var ctx = new PetWhizzEntities())
                {
                    var userToken = ctx.userTokens.Where(a => a.token.Equals(Token)).FirstOrDefault();
                    if (userToken != null && userToken.expiryTime >= DateTime.Now)
                    {
                        status = true;
                        HttpContext.Current.User = new CurrentUser(userToken.userDevice.user, Token);
                    }
                }
                logger.Debug("Successfully validated the token with DB token -" + Token);
            }
            catch (Exception ex)
            {
                logger.Error("Error occured while validating the token with DB tokens -" + Token);
                throw;
            }
            return status;

        }

       
    }

    public class CurrentUser : IPrincipal
    {
        public int userId { get; set; }
        public string username { get; set; }
        public string token { get; set; }
        public CurrentUser(user user,string _token)
        {
            userId = user.id;
            username = user.userName;
            token = _token;
        }

        public IIdentity Identity
        {
            get; private set;
        }

        public bool IsInRole(string role)
        {
            return true;
        }
    }
}