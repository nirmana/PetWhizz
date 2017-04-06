using Cryptography;
using PetWhizz.Api.Services;
using PetWhizz.Data;
using PetWhizz.Dto.Common;
using PetWhizz.Dto.CustomException;
using PetWhizz.Dto.Enum;
using PetWhizz.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace PetWhizz.Api.Security
{
    public class PetWhizzAuthorizeAttribute : AuthorizeAttribute
    {
        String EncryptionKey = String.Empty;
        SecurityService securityService = new SecurityService();
        private Decryptor Decryptor;
        public PetWhizzAuthorizeAttribute()
        {
            EncryptionKey = Common.Instance.EncryptionKey;
            Decryptor = new Decryptor();
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            try
            {
                if (Authorize(actionContext))
                {
                    return;
                }
                HandleUnauthorizedRequest(actionContext);
            }
            catch (Exception)
            {
                PetWhizzResponse IGResponse = new PetWhizzResponse()
                {
                    Message = "Token Expired",
                    Code = (Int32)ErrorCode.TOKENEXPIRED,
                    Object = null,
                    ObjectType = null
                };
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.OK,
                    IGResponse
                );
                return;
            }

        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            //create custom unauthorized exception
            PetWhizzResponse IGResponse = new PetWhizzResponse()
            {
                Message = "Unauthorized Call",
                Code = (Int32)ErrorCode.UNAUTHORIZED,
                Object = null,
                ObjectType = null
            };
            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.OK,
                IGResponse
            );
            return;
        }

        private bool Authorize(HttpActionContext actionContext)
        {
            try
            {
                string encryptedAuthHeader = "";
                IEnumerable<string> headers;
                if (actionContext.ControllerContext.Request.Headers.TryGetValues("Authorization", out headers))
                {
                    encryptedAuthHeader = headers.FirstOrDefault();
                    if (string.IsNullOrEmpty(encryptedAuthHeader))
                        throw new Exception("Unauthorized Call");

                    string decryptedHeader = Decryptor.Decrypt(encryptedAuthHeader).Split('|')[1];
                    if (!ValidateAuthToken(decryptedHeader))
                    {
                        throw new CustomException("Token Expired", (int)ErrorCode.TOKENEXPIRED);
                    }
                }
                else
                {
                    throw new Exception("Unauthorized Call");
                }
                return true;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool ValidateAuthToken(String Token)
        {
            return securityService.ValidateAuthToken(Token);
        }
    }
}