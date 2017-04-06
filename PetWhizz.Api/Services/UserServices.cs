using Cryptography;
using NLog;
using PetWhizz.Data;
using PetWhizz.Dto.CustomException;
using PetWhizz.Dto.Enum;
using PetWhizz.Dto.Request;
using PetWhizz.Dto.Response;
using PetWhizz.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Web;
using PetWhizz.Dto;
using System.Data.SqlTypes;

namespace PetWhizz.Api.Services
{
    public class UserServices
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Encryptor Encryptor;
        private Decryptor Decryptor;
        String EncryptionKey, HostedBaseUrl = String.Empty;
        double TokenExpiryTime, VerificationCodeExpiryTime, ResetPasswordExpiryTime = 0;

        public UserServices()
        {
            EncryptionKey = Common.Instance.EncryptionKey;
            Decryptor = new Decryptor();
            Encryptor = new Encryptor();
            TokenExpiryTime = Common.Instance.TokenExpiryTime;
            VerificationCodeExpiryTime = Common.Instance.VerificationCodeExpiryTime;
            ResetPasswordExpiryTime = Common.Instance.ResetPasswordExpiryTime;
            HostedBaseUrl = Common.Instance.HostedBaseUrl;
        }

        internal EnrollUserResponse EnrollUser(EnrollUserRequest EnrollUserRequest)
        {
            logger.Debug("Recived enroll user request");
            EnrollUserResponse EnrollUserResponse;
            try
            {
                ValidateUserRequest ValidateUserRequest = new ValidateUserRequest()
                {
                    email = EnrollUserRequest.email,
                    username = EnrollUserRequest.username
                };
                ValidatUser(ValidateUserRequest);

                EnrollUserRequest.username = Decryptor.Decrypt(EnrollUserRequest.username).Split('|')[1];
                EnrollUserRequest.password = Decryptor.Decrypt(EnrollUserRequest.password).Split('|')[1];
                EnrollUserRequest.email = Decryptor.Decrypt(EnrollUserRequest.email).Split('|')[1];
                EnrollUserRequest.deviceId = Decryptor.Decrypt(EnrollUserRequest.deviceId).Split('|')[1];

                String GeneratedToken = Guid.NewGuid().ToString();
                int GeneratedCode = new Random().Next(100000, 999999);

                logger.Debug("Decrypted enroll user request details userName - " + EnrollUserRequest.username +
                    " password - " + EnrollUserRequest.password + " email - " + EnrollUserRequest.email +
                    " deviceId - " + EnrollUserRequest.deviceId);

                //validating details
                if (!String.IsNullOrEmpty(EnrollUserRequest.username)
                    && !String.IsNullOrEmpty(EnrollUserRequest.password)
                    && !String.IsNullOrEmpty(EnrollUserRequest.email)
                    && !String.IsNullOrEmpty(EnrollUserRequest.deviceId))
                {
                    //setting up user details
                    var user = new user()
                    {
                        createdDate = DateTime.Now,
                        lastUpdatedDate = DateTime.Now,
                        userName = EnrollUserRequest.username,
                        password = EnrollUserRequest.password,
                        eMail = EnrollUserRequest.email,
                        status = "EMAILVERIFY"
                    };
                    using (var ctx = new PetWhizzEntities())
                    {
                        //saving user
                        ctx.users.Add(user);
                        ctx.SaveChanges();
                        //saving user device
                        var userDevice = new userDevice()
                        {
                            deviceId = EnrollUserRequest.deviceId,
                            // deviceName = EnrollUserRequest.deviceName,
                            userId = user.id,
                        };
                        ctx.userDevices.Add(userDevice);
                        ctx.SaveChanges();
                        //saving user token
                        var userToken = new userToken()
                        {
                            tokenType = "AUTHTOKEN",
                            useCount = 0,
                            generatedTime = DateTime.Now,
                            userDeviceId = userDevice.id,
                            expiryTime = DateTime.Now.AddSeconds(TokenExpiryTime),
                            token = GeneratedToken,
                        };
                        ctx.userTokens.Add(userToken);
                        ctx.SaveChanges();
                        //user verification data
                        var userVerificationInfo = new userVerification()
                        {
                            code = GeneratedCode.ToString(),
                            generatedTime = DateTime.Now,
                            expiryTime = DateTime.Now.AddSeconds(VerificationCodeExpiryTime),
                            isValid = true,
                            userId = user.id,
                            verificationType = "EMAILVERIFY",
                        };
                        ctx.userVerifications.Add(userVerificationInfo);
                        ctx.SaveChanges();
                    }
                    SendEmailVerification(user.eMail, user.userName, user.id, GeneratedCode.ToString());
                    EnrollUserResponse = new EnrollUserResponse()
                    {
                        token = Encryptor.Encrypt(DateTime.Now.ToString("M/d/yyyy h:mm:ss tt") + "|" + GeneratedToken),
                        username = Encryptor.Encrypt(DateTime.Now.ToString("M/d/yyyy h:mm:ss tt") + "|" + user.userName),
                        email = Encryptor.Encrypt(DateTime.Now.ToString("M/d/yyyy h:mm:ss tt") + "|" + user.eMail),
                        status = Encryptor.Encrypt(DateTime.Now.ToString("M/d/yyyy h:mm:ss tt") + "|" + user.status),
                        userId = Encryptor.Encrypt(DateTime.Now.ToString("M/d/yyyy h:mm:ss tt") + "|" + user.id)
                    };
                }
                else
                {
                    logger.Error("Some of the properties in EnrollUserRequest is null or empty");
                    throw new CustomException("All propreties should contains a value", (int)ErrorCode.VALIDATIONFAILED);
                }

            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                logger.Error(MethodBase.GetCurrentMethod().Name + ": exception: " + ex.Message + ", " + ex.InnerException);
                throw new CustomException("SystemError", ex, (int)ErrorCode.PROCEESINGERROR);
            }
            return EnrollUserResponse;
        }

        internal string ResetPassword(ResetPasswordRequest resetPasswordRequest)
        {
            logger.Debug("Recived ResetPassword request for token - " + resetPasswordRequest.token);
            String Status = String.Empty;
            try
            {
                if (!String.IsNullOrEmpty(resetPasswordRequest.token) && !String.IsNullOrEmpty(resetPasswordRequest.newPassowrd))
                {
                    int UserId = Convert.ToInt32(Decryptor.Decrypt(resetPasswordRequest.token).Split('|')[1]);
                    int RandomCode = Convert.ToInt32(Decryptor.Decrypt(resetPasswordRequest.token).Split('|')[0]);
                    logger.Debug("Recived ResetPassword request for userId - " + UserId.ToString() + " and code - " + RandomCode.ToString());
                    userVerification _currentVerification;
                    using (var ctx = new PetWhizzEntities())
                    {
                        _currentVerification = ctx.userVerifications
                            .Where(a => a.userId == UserId && a.code == RandomCode.ToString() && a.verificationType == "RESETPASSWORD" &&
                             a.expiryTime >= DateTime.Now && a.isValid == true).FirstOrDefault();

                        if (_currentVerification != null)
                        {
                            using (var dbContextTransaction = ctx.Database.BeginTransaction())
                            {
                                try
                                {
                                    ctx.userVerifications.Attach(_currentVerification);
                                    _currentVerification.isValid = false;

                                    ctx.users.Attach(_currentVerification.user);
                                    _currentVerification.user.password = resetPasswordRequest.newPassowrd;

                                    ctx.SaveChanges();
                                    dbContextTransaction.Commit();
                                    Status = "success";
                                }
                                catch (Exception)
                                {
                                    dbContextTransaction.Rollback();
                                    dbContextTransaction.Dispose();
                                    logger.Error("verification details update failed for userId - " + UserId.ToString() + " and code - " + RandomCode.ToString());
                                    throw new CustomException("verification details update failed", (int)ErrorCode.PROCEESINGERROR);
                                }

                            }
                        }
                        else
                        {
                            ctx.Dispose();
                            logger.Error("No ResetPassword found for userId - " + UserId.ToString() + " and code - " + RandomCode.ToString());
                            throw new CustomException("ResetPassword details not found", (int)ErrorCode.PROCEESINGERROR);
                        }
                    }
                }
                else
                {
                    logger.Error("Recived ResetPassword request token or new password empty");
                    throw new CustomException("Recived ResetPassword request token or new password empty", (int)ErrorCode.VALIDATIONFAILED);
                }
            }
            catch (CustomException) { throw; }
            catch (Exception)
            {
                logger.Error("ResetPassword failed");
                throw;
            }
            return Status;
        }

        internal RefreshTokenResponse RefreshToken(RefreshTokenRequest refreshTokenRequest)
        {
            logger.Debug("Recived RefreshToken request");
            RefreshTokenResponse RefreshTokenResponse;
            try
            {
                if (String.IsNullOrEmpty(refreshTokenRequest.token))
                {
                    logger.Error("Refresh token validation failed");
                    throw new CustomException("Refresh token validation failed.Token is null", (int)ErrorCode.VALIDATIONFAILED);
                }
                refreshTokenRequest.token = Decryptor.Decrypt(refreshTokenRequest.token).Split('|')[1];
                String NewToken = Guid.NewGuid().ToString();
                using (var ctx = new PetWhizzEntities())
                {
                    userToken UserToken = ctx.userTokens.Where(a => a.token == refreshTokenRequest.token && a.tokenType == "AUTHTOKEN").FirstOrDefault();
                    if (UserToken == null)
                    {
                        logger.Error("token is invalid");
                        throw new CustomException(" token is invalid", (int)ErrorCode.UNAUTHORIZED);
                    }
                    //update existing token
                    ctx.userTokens.Attach(UserToken);
                    UserToken.generatedTime = DateTime.Now;
                    UserToken.token = NewToken;
                    UserToken.expiryTime = DateTime.Now.AddSeconds(TokenExpiryTime);
                    ctx.SaveChanges();

                    RefreshTokenResponse = new RefreshTokenResponse()
                    {
                        token = Encryptor.Encrypt(DateTime.Now.ToString("M/d/yyyy h:mm:ss tt") + "|" + NewToken),
                    };
                }
            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                logger.Error(MethodBase.GetCurrentMethod().Name + ": exception: " + ex.Message + ", " + ex.InnerException);
                throw new CustomException("SystemError", ex, (int)ErrorCode.PROCEESINGERROR);
            }
            return RefreshTokenResponse;
        }


        internal void SendVerifyEmail()
        {
            logger.Debug("Recived SendVerifyEmail request");
            try
            {
                CurrentUser currentUser = (CurrentUser)HttpContext.Current.User;
                if (String.IsNullOrEmpty(currentUser.token))
                {
                    logger.Error("Verify email token is invalid");
                    throw new CustomException(" token is invalid", (int)ErrorCode.UNAUTHORIZED);
                }

                using (var ctx = new PetWhizzEntities())
                {
                    using (var dbContextTransaction = ctx.Database.BeginTransaction())
                    {
                        try
                        {
                            user User = ctx.users.Where(a => a.id == currentUser.userId).FirstOrDefault();
                            if (User == null)
                            {
                                logger.Error("Verify email user is invalid");
                                throw new CustomException("user is invalid", (int)ErrorCode.UNAUTHORIZED);
                            }
                            //inactive existing verifications for user
                            List<userVerification> userVerificationList = ctx.userVerifications.Where(a => a.userId == User.id
                                    && a.verificationType == "EMAILVERIFY" && a.isValid == true).ToList();
                            foreach (userVerification userVerification in userVerificationList)
                            {
                                ctx.userVerifications.Attach(userVerification);
                                userVerification.isValid = false;
                                ctx.SaveChanges();
                            }

                            int GeneratedCode = new Random().Next(100000, 999999);
                            var userVerificationInfo = new userVerification()
                            {
                                code = GeneratedCode.ToString(),
                                generatedTime = DateTime.Now,
                                expiryTime = DateTime.Now.AddSeconds(VerificationCodeExpiryTime),
                                isValid = true,
                                userId = User.id,
                                verificationType = "EMAILVERIFY",
                            };
                            ctx.userVerifications.Add(userVerificationInfo);
                            ctx.SaveChanges();
                            SendEmailVerification(User.eMail, User.userName, User.id, GeneratedCode.ToString());
                            dbContextTransaction.Commit();
                        }
                        catch (Exception)
                        {
                            dbContextTransaction.Rollback();
                            dbContextTransaction.Dispose();
                            logger.Error("verification details update failed for userId - " + currentUser.userId.ToString());
                            throw new CustomException("verification details update failed", (int)ErrorCode.PROCEESINGERROR);
                        }
                    }

                }
            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                logger.Error(MethodBase.GetCurrentMethod().Name + ": exception: " + ex.Message + ", " + ex.InnerException);
                throw new CustomException("SystemError", ex, (int)ErrorCode.PROCEESINGERROR);
            }
        }

        internal string ValidateResetPasswordToken(string token)
        {
            logger.Debug("Recived Validate ResetPassword Token request for token - " + token);
            String UserName = String.Empty;
            try
            {
                int UserId = Convert.ToInt32(Decryptor.Decrypt(token).Split('|')[1]);
                int RandomCode = Convert.ToInt32(Decryptor.Decrypt(token).Split('|')[0]);
                logger.Debug("Recived Validate ResetPassword Token request for userId - " + UserId.ToString() + " and code - " + RandomCode.ToString());
                userVerification _currentVerification;
                using (var ctx = new PetWhizzEntities())
                {
                    _currentVerification = ctx.userVerifications
                        .Where(a => a.userId == UserId && a.code == RandomCode.ToString() && a.verificationType == "RESETPASSWORD" &&
                         a.expiryTime >= DateTime.Now && a.isValid == true).FirstOrDefault();

                    if (_currentVerification != null)
                    {
                        UserName = _currentVerification.user.userName;
                        UserName = char.ToUpper(UserName[0]) + UserName.Substring(1);
                    }
                    else
                    {
                        ctx.Dispose();
                        logger.Error("no verification details found for userId - " + UserId.ToString() + " and code - " + RandomCode.ToString());
                        throw new CustomException("Verification details not found", (int)ErrorCode.PROCEESINGERROR);
                    }
                }
            }
            catch (CustomException) { throw; }
            catch (Exception)
            {
                logger.Error("Error occured while user verification token - " + token);
                throw;
            }
            return UserName;
        }

        internal void ForgotPassword(ForgotPasswordRequest changePasswordRequest)
        {
            logger.Debug("Recived forgotPassword request for encrypted string - " + changePasswordRequest.email);
            try
            {
                changePasswordRequest.email = Decryptor.Decrypt(changePasswordRequest.email).Split('|')[1];
                logger.Debug("Recived forgotPassword request for email = " + changePasswordRequest.email);
                if (!String.IsNullOrEmpty(changePasswordRequest.email))
                {
                    user User;
                    using (var ctx = new PetWhizzEntities())
                    {
                        User = ctx.users.Where(a => a.eMail.ToLower() == changePasswordRequest.email.ToLower()).FirstOrDefault();
                    }
                    if (User == null)
                    {
                        logger.Error("User not found for given email - " + changePasswordRequest.email);
                        throw new CustomException("User not found for given email", (int)ErrorCode.USERNOTFOUND);
                    }
                    int GeneratedCode = new Random().Next(100000, 999999);
                    var userVerificationInfo = new userVerification()
                    {
                        code = GeneratedCode.ToString(),
                        generatedTime = DateTime.Now,
                        expiryTime = DateTime.Now.AddSeconds(VerificationCodeExpiryTime),
                        isValid = true,
                        userId = User.id,
                        verificationType = "RESETPASSWORD",
                    };
                    using (var ctx = new PetWhizzEntities())
                    {
                        ctx.userVerifications.Add(userVerificationInfo);
                        ctx.SaveChanges();
                    }

                    SendResetPasswordEmail(User.eMail, User.id, User.userName, GeneratedCode);

                }
                else
                {
                    logger.Error("ForgotPassword request email is empty");
                    throw new CustomException("Email is empty", (int)ErrorCode.VALIDATIONFAILED);
                }
            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                logger.Error(MethodBase.GetCurrentMethod().Name + ": exception: " + ex.Message + ", " + ex.InnerException);
                throw new CustomException("SystemError", ex, (int)ErrorCode.PROCEESINGERROR);
            }
        }

        private void SendResetPasswordEmail(string eMail, int id, string userName, int generatedCode)
        {
            EmailService emailService = new EmailService();
            String emailBodyHtml = emailService.GetBodyHtml("PASSWORDRESET");
            if (String.IsNullOrEmpty(emailBodyHtml))
            {
                logger.Error("Email template not found for type - PASSWORDRESET");
                throw new CustomException("Email type not found", (int)ErrorCode.EMAILERROR);
            }

            String VerificationToken = generatedCode + "|" + id;
            emailBodyHtml = emailBodyHtml.Replace("{UserName}", char.ToUpper(userName[0]) + userName.Substring(1)).Replace("{VerifyLink}", HostedBaseUrl + "Web/resetpassword.html?token=" + Encryptor.Encrypt(VerificationToken));
            MailMessage email = new MailMessage()
            {
                From = new MailAddress("noreply@petwhizz.com"),
                Subject = "Petwhizz Account Reset",
                Body = emailBodyHtml,
                IsBodyHtml = true,

            };
            email.To.Add(eMail);
            AlternateView view = emailService.EmbedLogosForEmailBody(email.Body);
            email.AlternateViews.Add(view);
            emailService.SendEmail(email);
        }

        internal void DeleteUser(string userName)
        {
            try
            {
                userName = Decryptor.Decrypt(userName).Split('|')[1];
                using (var ctx = new PetWhizzEntities())
                {
                    user User = ctx.users.Where(a => a.userName == userName).FirstOrDefault();
                    List<userDevice> UserDevices = ctx.userDevices.Where(a => a.userId == User.id).ToList();
                    foreach (userDevice device in UserDevices)
                    {
                        ctx.userTokens.RemoveRange(device.userTokens);
                    }
                    ctx.userDevices.RemoveRange(UserDevices);
                    ctx.userVerifications.RemoveRange(User.userVerifications);
                    ctx.users.Remove(User);
                    ctx.SaveChanges();
                }
            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                logger.Error(MethodBase.GetCurrentMethod().Name + ": exception: " + ex.Message + ", " + ex.InnerException);
                throw new CustomException("SystemError", ex, (int)ErrorCode.PROCEESINGERROR);
            }
        }

        internal void VerifyEmail(string token)
        {
            logger.Debug("Recived verify email request for token - " + token);
            try
            {
                int UserId = Convert.ToInt32(Decryptor.Decrypt(token).Split('|')[1]);
                int RandomCode = Convert.ToInt32(Decryptor.Decrypt(token).Split('|')[0]);
                logger.Debug("Recived verify email request for userId - " + UserId.ToString() + " and code - " + RandomCode.ToString());
                userVerification _currentVerification;
                using (var ctx = new PetWhizzEntities())
                {
                    _currentVerification = ctx.userVerifications
                        .Where(a => a.userId == UserId && a.code == RandomCode.ToString() && a.verificationType == "EMAILVERIFY" &&
                         a.expiryTime >= DateTime.Now && a.isValid == true).FirstOrDefault();

                    if (_currentVerification != null)
                    {
                        using (var dbContextTransaction = ctx.Database.BeginTransaction())
                        {
                            try
                            {
                                ctx.userVerifications.Attach(_currentVerification);
                                _currentVerification.isValid = false;

                                ctx.users.Attach(_currentVerification.user);
                                _currentVerification.user.status = "VERIFIED";

                                ctx.SaveChanges();
                                dbContextTransaction.Commit();
                            }
                            catch (Exception)
                            {
                                dbContextTransaction.Rollback();
                                dbContextTransaction.Dispose();
                                logger.Error("verification details update failed for userId - " + UserId.ToString() + " and code - " + RandomCode.ToString());
                                throw new CustomException("verification details update failed", (int)ErrorCode.PROCEESINGERROR);
                            }

                        }
                    }
                    else
                    {
                        ctx.Dispose();
                        logger.Error("verification details not for userId - " + UserId.ToString() + " and code - " + RandomCode.ToString());
                        throw new CustomException("Verification details not found", (int)ErrorCode.PROCEESINGERROR);
                    }
                }
            }
            catch (CustomException) { throw; }
            catch (Exception)
            {
                logger.Error("Error occured while user verification token - " + token);
                throw;
            }


        }

        internal UserLoginResponse UserLogin(UserLoginRequest userLoginRequest)
        {
            logger.Debug("Recived user login request");
            UserLoginResponse UserLoginResponse;
            try
            {
                String GeneratedToken = Guid.NewGuid().ToString();

                userLoginRequest.deviceId = Decryptor.Decrypt(userLoginRequest.deviceId).Split('|')[1];
                userLoginRequest.username = Decryptor.Decrypt(userLoginRequest.username).Split('|')[1];
                userLoginRequest.password = Decryptor.Decrypt(userLoginRequest.password).Split('|')[1];
                logger.Debug("Recived user login request with username - " + userLoginRequest.username + " password - " + userLoginRequest.password + " deviceId - " + userLoginRequest.deviceId);
                if (!String.IsNullOrEmpty(userLoginRequest.deviceId)
                    && !String.IsNullOrEmpty(userLoginRequest.username)
                    && !String.IsNullOrEmpty(userLoginRequest.password))
                {

                    using (var ctx = new PetWhizzEntities())
                    {
                        //checking for user
                        user User = ctx.users.Where(a => a.userName.ToLower().Equals(userLoginRequest.username.ToLower())
                        && a.password == userLoginRequest.password).FirstOrDefault();
                        if (User == null)
                        {
                            logger.Error("Login failed for user - " + userLoginRequest.username);
                            throw new CustomException("Username or Password Invalid", (int)ErrorCode.LOGINFAILURE);
                        }
                        UserLoginResponse = new UserLoginResponse()
                        {
                            email = Encryptor.Encrypt(DateTime.Now.ToString("M/d/yyyy h:mm:ss tt") + "|" + User.eMail),
                            status = Encryptor.Encrypt(DateTime.Now.ToString("M/d/yyyy h:mm:ss tt") + "|" + User.status),
                            token = Encryptor.Encrypt(DateTime.Now.ToString("M/d/yyyy h:mm:ss tt") + "|" + GeneratedToken),
                            username = Encryptor.Encrypt(DateTime.Now.ToString("M/d/yyyy h:mm:ss tt") + "|" + User.userName),
                            userId = Encryptor.Encrypt(DateTime.Now.ToString("M/d/yyyy h:mm:ss tt") + "|" + User.id.ToString())
                        };
                        //checking for device
                        userDevice UserDevice = ctx.userDevices.Where(a => a.userId == User.id && a.deviceId == userLoginRequest.deviceId).FirstOrDefault();
                        if (UserDevice == null)
                        {
                            //new device
                            var userDevice = new userDevice()
                            {
                                deviceId = userLoginRequest.deviceId,
                                deviceName = "",
                                userId = User.id,
                            };
                            ctx.userDevices.Add(userDevice);
                            ctx.SaveChanges();

                            //saving user token
                            var userToken = new userToken()
                            {
                                tokenType = "AUTHTOKEN",
                                useCount = 0,
                                generatedTime = DateTime.Now,
                                userDeviceId = userDevice.id,
                                expiryTime = DateTime.Now.AddSeconds(TokenExpiryTime),
                                token = GeneratedToken,
                            };
                            ctx.userTokens.Add(userToken);
                            ctx.SaveChanges();
                        }
                        else
                        {
                            userToken userDBToken = ctx.userTokens.Where(a => a.userDeviceId == UserDevice.id).FirstOrDefault();
                            if (userDBToken == null)
                            {
                                var userToken = new userToken()
                                {
                                    tokenType = "AUTHTOKEN",
                                    useCount = 0,
                                    generatedTime = DateTime.Now,
                                    userDeviceId = UserDevice.id,
                                    expiryTime = DateTime.Now.AddSeconds(TokenExpiryTime),
                                    token = GeneratedToken,
                                };
                                ctx.userTokens.Add(userToken);
                                ctx.SaveChanges();
                            }
                            else
                            {

                                ctx.userTokens.Attach(userDBToken);
                                userDBToken.expiryTime = DateTime.Now.AddSeconds(TokenExpiryTime);
                                ctx.SaveChanges();
                                UserLoginResponse.token = Encryptor.Encrypt(DateTime.Now.ToString("M/d/yyyy h:mm:ss tt") + "|" + userDBToken.token);
                            }
                        }
                    }
                }
                else
                {
                    logger.Error("Some of the properties in userLoginRequest is null or empty");
                    throw new CustomException("All propreties should contains a value", (int)ErrorCode.VALIDATIONFAILED);
                }
            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                logger.Error(MethodBase.GetCurrentMethod().Name + ": exception: " + ex.Message + ", " + ex.InnerException);
                throw new CustomException("SystemError", ex, (int)ErrorCode.PROCEESINGERROR);
            }
            return UserLoginResponse;
        }

        internal ValidateUserResponse ValidatUser(ValidateUserRequest validateUserRequest)
        {
            logger.Debug("Recived validate user request");
            ValidateUserResponse ValidateUserResponse;
            try
            {
                validateUserRequest.email = Decryptor.Decrypt(validateUserRequest.email).Split('|')[1];
                validateUserRequest.username = Decryptor.Decrypt(validateUserRequest.username).Split('|')[1];
                int usersWithEmail, UsersWithUserName = 0;
                using (var ctx = new PetWhizzEntities())
                {
                    usersWithEmail = ctx.users.Where(a => a.eMail.ToLower().Equals(validateUserRequest.email.ToLower())).Count();
                    UsersWithUserName = ctx.users.Where(a => a.userName.ToLower().Equals(validateUserRequest.username.ToLower())).Count();
                }
                if (usersWithEmail > 0 || UsersWithUserName > 0)
                {
                    logger.Error("Email or Username Already Exist userName - " + validateUserRequest.username + " email - " + validateUserRequest.email);
                    throw new CustomException("Email or Username Already Exist", (int)ErrorCode.EMAILORUSERNAMEALREADYEXIST);
                }
                ValidateUserResponse = new ValidateUserResponse() { messege = "Success" };

            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                logger.Error(MethodBase.GetCurrentMethod().Name + ": exception: " + ex.Message + ", " + ex.InnerException);
                throw new CustomException("SystemError", ex, (int)ErrorCode.PROCEESINGERROR); throw;
            }
            return ValidateUserResponse;
        }

        private void SendEmailVerification(string emailAddress, string userName, int userId, string code)
        {
            EmailService emailService = new EmailService();
            String emailBodyHtml = emailService.GetBodyHtml("EMAILVERIFICATION");
            if (String.IsNullOrEmpty(emailBodyHtml))
            {
                logger.Error("Email template not found for type - EMAILVERIFICATION");
                throw new CustomException("Email type not found", (int)ErrorCode.EMAILERROR);
            }
            String VerificationToken = code + "|" + userId;
            emailBodyHtml = emailBodyHtml.Replace("{UserName}", char.ToUpper(userName[0]) + userName.Substring(1)).Replace("{UserEmail}", emailAddress).Replace("{VerifyLink}", HostedBaseUrl + "user/verifyEmail?token=" + Encryptor.Encrypt(VerificationToken).Replace("+", "%2b"));
            MailMessage email = new MailMessage()
            {
                From = new MailAddress("noreply@petwhizz.com"),
                Subject = "Registration Successfull for PETWHIZZ",
                Body = emailBodyHtml,
                IsBodyHtml = true,

            };
            email.To.Add(emailAddress);
            AlternateView view = emailService.EmbedLogosForEmailBody(email.Body);
            email.AlternateViews.Add(view);
            emailService.SendEmail(email);

        }

        internal UserLoginResponse TokenLogin()
        {
            logger.Debug("Recived token login request");
            UserLoginResponse UserLoginResponse;
            try
            {
                CurrentUser currentUser = (CurrentUser)HttpContext.Current.User;
                if (String.IsNullOrEmpty(currentUser.token))
                {
                    logger.Error("token is empty for user");
                    throw new CustomException("token is empty for user", (int)ErrorCode.UNAUTHORIZED);
                }
                user User = new user();
                using (var ctx = new PetWhizzEntities())
                {
                    User = ctx.users.Where(a => a.id == currentUser.userId).FirstOrDefault();
                    if (User == null)
                    {
                        logger.Error("User record is not found for userId - " + currentUser.userId);
                        throw new CustomException("User record is not found", (int)ErrorCode.UNAUTHORIZED);
                    }
                    var currentToken = ctx.userTokens.Where(a => a.token.Equals(currentUser.token)).FirstOrDefault();
                    if (currentToken != null && currentToken.expiryTime >= DateTime.Now)
                    {
                        ctx.userTokens.Attach(currentToken);
                        currentToken.expiryTime = DateTime.Now.AddSeconds(TokenExpiryTime);
                        ctx.SaveChanges();
                    }
                    else
                    {
                        logger.Error("token is invalid or expired");
                        throw new CustomException("token is invalid or expired", (int)ErrorCode.UNAUTHORIZED);
                    }
                }

                UserLoginResponse = new UserLoginResponse()
                {
                    email = Encryptor.Encrypt(DateTime.Now.ToString("M/d/yyyy h:mm:ss tt") + "|" + User.eMail),
                    status = Encryptor.Encrypt(DateTime.Now.ToString("M/d/yyyy h:mm:ss tt") + "|" + User.status),
                    token = Encryptor.Encrypt(DateTime.Now.ToString("M/d/yyyy h:mm:ss tt") + "|" + currentUser.token),
                    username = Encryptor.Encrypt(DateTime.Now.ToString("M/d/yyyy h:mm:ss tt") + "|" + User.userName),
                    userId = Encryptor.Encrypt(DateTime.Now.ToString("M/d/yyyy h:mm:ss tt") + "|" + User.id.ToString())
                };
                logger.Debug("Token login successfull for token - " + currentUser.token);

            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                logger.Error(MethodBase.GetCurrentMethod().Name + ": exception: " + ex.Message + ", " + ex.InnerException);
                throw new CustomException("SystemError", ex, (int)ErrorCode.PROCEESINGERROR); throw;
            }
            return UserLoginResponse;
        }

        internal void UpdateUser(UserUpdateRequest UserUpdateRequest)
        {
            logger.Debug("Recived update user request");
            try
            {
                UserUpdateRequest.email = Decryptor.Decrypt(UserUpdateRequest.email);
                if(String.IsNullOrEmpty(UserUpdateRequest.email))
                {
                    logger.Error("User update required user email");
                    throw new CustomException("User update required user email", (int)ErrorCode.VALIDATIONFAILED);
                }
                CurrentUser currentUser = (CurrentUser)HttpContext.Current.User;
                if (currentUser.userId >= 0)
                {
                    logger.Error("User is not found on the session");
                    throw new CustomException("User is not found on the session", (int)ErrorCode.UNAUTHORIZED);
                }
                using (var ctx = new PetWhizzEntities())
                {
                    user User = ctx.users.Where(a => a.id == currentUser.userId).FirstOrDefault();
                    ctx.users.Attach(User);
                    User.addressLine1 = UserUpdateRequest.addressLine1;
                    User.addressLine2 = UserUpdateRequest.addressLine2;
                    User.dateOfBirth = UserUpdateRequest.dateOfBirth;
                    User.eMail = UserUpdateRequest.email;
                    User.firstName = UserUpdateRequest.firstName;
                    User.lastName = UserUpdateRequest.lastName;
                    User.lastUpdatedDate = DateTime.Now;
                    User.middleName = UserUpdateRequest.middleName;
                    User.mobileNumber = UserUpdateRequest.mobileNumber;
                    User.profilePic = UserUpdateRequest.profilePic;
                    ctx.SaveChanges();
                }
                logger.Debug("Successfully updated user - " + currentUser.userId);
            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                logger.Error(MethodBase.GetCurrentMethod().Name + ": exception: " + ex.Message + ", " + ex.InnerException);
                throw new CustomException("SystemError", ex, (int)ErrorCode.PROCEESINGERROR); throw;
            }
        }

        internal void LogoutUser()
        {
            logger.Debug("Recived log out request");
            try
            {
                CurrentUser currentUser = (CurrentUser)HttpContext.Current.User;
                if (String.IsNullOrEmpty(currentUser.token))
                {
                    logger.Error("token is empty for user");
                    throw new CustomException("token is empty for user", (int)ErrorCode.UNAUTHORIZED);
                }

                using (var ctx = new PetWhizzEntities())
                {
                    userToken currentToken = ctx.userTokens.Where(a => a.token.Equals(currentUser.token)).FirstOrDefault();
                    if (currentToken == null)
                    {
                        logger.Error("token is not found on DB");
                        throw new CustomException("token is not found on DB", (int)ErrorCode.UNAUTHORIZED);
                    }

                    ctx.userTokens.Attach(currentToken);
                    currentToken.expiryTime = null;
                    ctx.SaveChanges();
                    logger.Debug("successfully logout user - " + currentUser.username);
                }
            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                logger.Error(MethodBase.GetCurrentMethod().Name + ": exception: " + ex.Message + ", " + ex.InnerException);
                throw new CustomException("SystemError", ex, (int)ErrorCode.PROCEESINGERROR); throw;
            }
        }

        internal void InviteUser(InviteUserRequest InviteUserRequest)
        {
            logger.Debug("Recived Invite user request");
            try
            {
                if (String.IsNullOrEmpty(InviteUserRequest.email))
                {
                    logger.Error("User Invite required an email");
                    throw new CustomException("User Invite required an email", (int)ErrorCode.VALIDATIONFAILED);
                }
                CurrentUser currentUser = (CurrentUser)HttpContext.Current.User;
                SendInvitationEmail(currentUser.username, InviteUserRequest.email, InviteUserRequest.message);
            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                logger.Error(MethodBase.GetCurrentMethod().Name + ": exception: " + ex.Message + ", " + ex.InnerException);
                throw new CustomException("SystemError", ex, (int)ErrorCode.PROCEESINGERROR); throw;
            }
        }

        private void SendInvitationEmail(string username, string Reciveremail, string message)
        {
            EmailService emailService = new EmailService();
            String emailBodyHtml = emailService.GetBodyHtml("INVITEUSER");
            if (String.IsNullOrEmpty(emailBodyHtml))
            {
                logger.Error("Email template not found for type - INVITEUSER");
                throw new CustomException("Email type not found", (int)ErrorCode.EMAILERROR);
            }
            emailBodyHtml = emailBodyHtml
                .Replace("{UserName}", char.ToUpper(username[0]) + username.Substring(1))
                .Replace("{note}", message)
                .Replace("{VerifyLink}", "google.lk")
                .Replace("+", "%2b");

            MailMessage email = new MailMessage()
            {
                From = new MailAddress("noreply@petwhizz.com"),
                Subject = "Invitation to PetWhizz ",
                Body = emailBodyHtml,
                IsBodyHtml = true,

            };
            email.To.Add(Reciveremail);
            AlternateView view = emailService.EmbedLogosForEmailBody(email.Body);
            email.AlternateViews.Add(view);
            emailService.SendEmail(email);
        }
    }
}