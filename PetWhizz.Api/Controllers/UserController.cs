using NLog;
using PetWhizz.Api.Security;
using PetWhizz.Api.Services;
using PetWhizz.Dto;
using PetWhizz.Dto.Common;
using PetWhizz.Dto.CustomException;
using PetWhizz.Dto.Request;
using PetWhizz.Dto.Response;
using PetWhizz.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace PetWhizz.Api.Controllers
{
    public class UserController : ApiController
    {
        UserServices userServices;

        public UserController()
        {
            userServices = new UserServices();
        }

        [Route("user/enroll")]
        [HttpPost]
        public PetWhizzResponse EnrollUser(EnrollUserRequest EnrollUserRequest)
        {
            PetWhizzResponse _oResponse;
            try
            {
                EnrollUserResponse EnrollUserResponse = userServices.EnrollUser(EnrollUserRequest);
                _oResponse = Utils.CreateSuccessResponse(EnrollUserResponse);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }

        [Route("user/validate")]
        [HttpPost]
        public PetWhizzResponse ValidateUser(ValidateUserRequest ValidateUserRequest)
        {
            PetWhizzResponse _oResponse;
            try
            {
                ValidateUserResponse ValidateUserResponse = userServices.ValidatUser(ValidateUserRequest);
                _oResponse = Utils.CreateSuccessResponse(ValidateUserResponse);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }

        [Route("user/verifyemail")]
        [HttpGet]
        public HttpResponseMessage VerifyEmail(string token)
        {
            var response = new HttpResponseMessage();
            try
            {
                userServices.VerifyEmail(token);
                response.Content = new StringContent("Email Verification Successful!");
            }
            catch (CustomException)
            {
                response.Content = new StringContent("Link might be expired or already activated");
            }
            catch (Exception)
            {
                response.Content = new StringContent("Error Occured");
            }
            return response;
        }

        [Route("user/login")]
        [HttpPost]
        public PetWhizzResponse UserLogin(UserLoginRequest UserLoginRequest)
        {
            PetWhizzResponse _oResponse;
            try
            {
                UserLoginResponse UserLoginResponse = userServices.UserLogin(UserLoginRequest);
                _oResponse = Utils.CreateSuccessResponse(UserLoginResponse);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }

        [Route("user/delete")]
        [HttpDelete]
        public PetWhizzResponse DeleteUser(DeleteUserRequest DeleteUserRequest)
        {
            PetWhizzResponse _oResponse;
            try
            {
                userServices.DeleteUser(DeleteUserRequest.username);
                _oResponse = Utils.CreateSuccessResponse(null);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }

        [Route("user/forgotpassword")]
        [HttpPost]
        public PetWhizzResponse ForgotPassword(ForgotPasswordRequest ForgotPasswordRequest)
        {
            PetWhizzResponse _oResponse;
            try
            {
                userServices.ForgotPassword(ForgotPasswordRequest);
                _oResponse = Utils.CreateSuccessResponse(null);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }

        [Route("user/validateresetpasswordtoken")]
        [HttpPost]
        public String ValidateResetPasswordToken(ValidateResetPasswordTokenRequest ValidateResetPasswordTokenRequest)
        {
            String _oResponse;
            try
            {
                _oResponse= userServices.ValidateResetPasswordToken(ValidateResetPasswordTokenRequest.token);
            }
            catch (Exception)
            {
                _oResponse = "failed";
            }
            return _oResponse;
        }

        [Route("user/resetpassword")]
        [HttpPost]
        public String ResetPassword(ResetPasswordRequest ResetPasswordRequest)
        {
            String _oResponse;
            try
            {
                _oResponse = userServices.ResetPassword(ResetPasswordRequest);

            }
            catch (Exception)
            {
                _oResponse = "failed";
            }
            return _oResponse;
        }

        [Route("user/tokenlogin")]
        [PetWhizzAuthorize]
        [HttpPost]
        public PetWhizzResponse TokenLogin()
        {
            PetWhizzResponse _oResponse;
            try
            {
                UserLoginResponse UserLoginResponse = userServices.TokenLogin();
                _oResponse = Utils.CreateSuccessResponse(UserLoginResponse);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }

        [Route("user/update")]
        [PetWhizzAuthorize]
        [HttpPut]
        public PetWhizzResponse UpdateUser(UserUpdateRequest UserUpdateRequest)
        {
            PetWhizzResponse _oResponse;
            try
            {
                 userServices.UpdateUser(UserUpdateRequest);
                _oResponse = Utils.CreateSuccessResponse(null);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }

        [Route("user/logout")]
        [PetWhizzAuthorize]
        [HttpPost]
        public PetWhizzResponse LogoutUser()
        {
            PetWhizzResponse _oResponse;
            try
            {
                userServices.LogoutUser();
                _oResponse = Utils.CreateSuccessResponse(null);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }

        [Route("user/invite")]
        [PetWhizzAuthorize]
        [HttpPost]
        public PetWhizzResponse InviteUser(InviteUserRequest InviteUserRequest)
        {
            PetWhizzResponse _oResponse;
            try
            {
                userServices.InviteUser(InviteUserRequest);
                _oResponse = Utils.CreateSuccessResponse(null);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }

        [Route("user/verifyemail")]
        [PetWhizzAuthorize]
        [HttpPost]
        public PetWhizzResponse SendVerifyEmail()
        {
            PetWhizzResponse _oResponse;
            try
            {
                userServices.SendVerifyEmail();
                _oResponse = Utils.CreateSuccessResponse(null);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }

        [Route("user/refreshtoken")]
        [HttpPost]
        public PetWhizzResponse RefreshToken(RefreshTokenRequest RefreshTokenRequest)
        {
            PetWhizzResponse _oResponse;
            try
            {
                RefreshTokenResponse RefreshTokenResponse = userServices.RefreshToken(RefreshTokenRequest);
                _oResponse = Utils.CreateSuccessResponse(RefreshTokenResponse);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }
    }
}
