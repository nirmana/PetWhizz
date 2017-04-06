using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PetWhizz.Dto.Request;
using PetWhizz.Dto.Response;
using NLog;
using PetWhizz.Dto.CustomException;
using PetWhizz.Dto.Enum;
using System.Reflection;
using PetWhizz.Data;
using PetWhizz.Dto.Common;
using System.Data.Entity.Core.Objects;
using Cryptography;
using System.Net.Mail;

namespace PetWhizz.Api.Services
{
    public class AnimalService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Decryptor Decryptor;
        private Encryptor Encryptor;
        private string HostedBaseUrl;
        public AnimalService()
        {
            Decryptor = new Decryptor();
            Encryptor = new Encryptor();
            HostedBaseUrl = Common.Instance.HostedBaseUrl;
        }

        internal AnimalBreedFilterResponse AnimalBreedFilter(AnimalBreedFilterRequest animalBreedFilterRequest)
        {
            logger.Trace("Recived AnimalBreedFilter request");
            AnimalBreedFilterResponse AnimalBreedFilterResponse = new AnimalBreedFilterResponse();
            try
            {
                if (String.IsNullOrEmpty(animalBreedFilterRequest.animalType))
                {
                    logger.Error("Recived Animal Breed Filter request Animal Type is empty");
                    throw new CustomException("Animal type is empty", (int)ErrorCode.VALIDATIONFAILED);
                }

                using (var ctx = new PetWhizzEntities())
                {
                    var animals = ctx.animals.Where(a => a.animalName.ToLower() == animalBreedFilterRequest.animalType.ToLower() && a.isActive == true).FirstOrDefault();
                    if (animals != null)
                    {

                        if (!String.IsNullOrEmpty(animalBreedFilterRequest.filterQuery))
                        {
                            AnimalBreedFilterResponse.AnimalBreedList = animals.animalBreeds.Where(a =>
                            a.breedName.ToLower().Contains(animalBreedFilterRequest.filterQuery.ToLower()) &&
                            a.isActive == true)
                                .Take(10)
                                .Select(a => new
                                {
                                    a.id,
                                    a.breedName
                                }).ToArray();
                        }
                    }
                    else
                    {
                        logger.Error("No animal found for type - " + animalBreedFilterRequest.animalType);
                        throw new CustomException("No animal found for type", (int)ErrorCode.NORECORDFOUND);
                    }
                }

            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                logger.Error(MethodBase.GetCurrentMethod().Name + ": exception: " + ex.Message + ", " + ex.InnerException);
                throw new CustomException("SystemError", ex, (int)ErrorCode.PROCEESINGERROR);
            }
            return AnimalBreedFilterResponse;

        }
        internal void UpdatePet(PetUpdateRequest PetUpdateRequest)
        {
            logger.Trace("Recived pet update request");
            try
            {
                CurrentUser currentUser = (CurrentUser)HttpContext.Current.User;
                if (PetUpdateRequest == null || PetUpdateRequest.id <= 0 ||
                    String.IsNullOrEmpty(PetUpdateRequest.petName))
                {
                    logger.Error("Pet update Request body is empty or required fields not found");
                    throw new CustomException("Pet update request invalid", (int)ErrorCode.VALIDATIONFAILED);
                }
                using (var ctx = new PetWhizzEntities())
                {
                    pet Pet = ctx.pets.Where(a => a.id == PetUpdateRequest.id).FirstOrDefault();
                    if (Pet == null)
                    {
                        logger.Error("Pet record is not fount for petId - " + PetUpdateRequest.id.ToString());
                        throw new CustomException("Pet object not found", (int)ErrorCode.NORECORDFOUND);
                    }
                    //looking for authorization
                    if (Pet.petOwners.Where(a => a.userId == currentUser.userId).FirstOrDefault() == null)
                    {
                        logger.Error("Pet record is not fount for petId - " + PetUpdateRequest.id.ToString() + " and userId - " + currentUser.userId);
                        throw new CustomException("Pet object not found for user", (int)ErrorCode.UNAUTHORIZED);
                    }

                    //update pet
                    ctx.pets.Attach(Pet);
                    Pet.breedId = PetUpdateRequest.breedId;
                    Pet.birthDay = PetUpdateRequest.birthDay;
                    Pet.coverImage = PetUpdateRequest.coverImage;
                    Pet.isActive = PetUpdateRequest.isActive;
                    Pet.lastUpdatedBy = currentUser.username;
                    Pet.lastUpdatedTime = DateTime.Now;
                    Pet.petName = PetUpdateRequest.petName;
                    Pet.profileImage = PetUpdateRequest.profileImage;
                    Pet.sex = PetUpdateRequest.sex;
                    ctx.SaveChanges();
                    logger.Trace("Successfully updated Pet id - " + Pet.id + " by - " + currentUser.username);
                }
            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                logger.Error(MethodBase.GetCurrentMethod().Name + ": exception: " + ex.Message + ", " + ex.InnerException);
                throw new CustomException("SystemError", ex, (int)ErrorCode.PROCEESINGERROR);
            }
        }

        internal PetUserResponse GetUsersByPet(PetUserRequest petUserRequest)
        {
            logger.Trace("Recived Get Users By Pet request");
            PetUserResponse PetUserResponse = new PetUserResponse();
            try
            {
                CurrentUser currentUser = (CurrentUser)HttpContext.Current.User;
                if (petUserRequest.petId <= 0)
                {
                    logger.Error("Get users by pet petId is invalid");
                    throw new CustomException("Get users by pet petId is invalid", (int)ErrorCode.VALIDATIONFAILED);
                }
                using (var ctx = new PetWhizzEntities())
                {
                    pet Pet = ctx.pets.Where(a => a.id == petUserRequest.petId).FirstOrDefault();
                    if (Pet == null)
                    {
                        logger.Error("No pet found for given Id");
                        throw new CustomException("No pet found for given Id", (int)ErrorCode.NORECORDFOUND);
                    }
                    List<petOwner> petOwnerList = Pet.petOwners.Where(a => a.userId != currentUser.userId).ToList();
                    PetUserResponse.PetUserList = new List<PetUser>();
                    foreach (petOwner owner in petOwnerList)
                    {
                        PetUserResponse.userCount++;
                        PetUser PetUser = new PetUser()
                        {
                            isConfirmed = owner.isActive,
                            profilePic = owner.user.profilePic,
                            userId = owner.userId,
                            userName = owner.user.userName
                        };
                        PetUserResponse.PetUserList.Add(PetUser);
                    }
                }
            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                logger.Error(MethodBase.GetCurrentMethod().Name + ": exception: " + ex.Message + ", " + ex.InnerException);
                throw new CustomException("SystemError", ex, (int)ErrorCode.PROCEESINGERROR);
            }
            return PetUserResponse;
        }

        internal void ConfirmPetSharingRequest(string token)
        {
            logger.Trace("Recived Confirm Pet share request");
            try
            {
                if (String.IsNullOrEmpty(token))
                {
                    logger.Error("Recived Confirm Pet share request token is empty");
                    throw new CustomException("Recived Confirm Pet share request token is empty", (int)ErrorCode.VALIDATIONFAILED);
                }
                var userId = Decryptor.Decrypt(token).Split('|')[0];
                var petId = Decryptor.Decrypt(token).Split('|')[1];
                var sharedUserId = Decryptor.Decrypt(token).Split('|')[2];

                if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(petId) || String.IsNullOrEmpty(sharedUserId))
                {
                    logger.Error("Recived Confirm Pet share request some of the properties empty");
                    throw new CustomException("Recived Confirm Pet share request some of the properties empty", (int)ErrorCode.VALIDATIONFAILED);
                }

                int iUserId = int.Parse(userId);
                Int64 ipetId = Int64.Parse(petId);
                int isharedUserId = int.Parse(sharedUserId);

                using (var ctx = new PetWhizzEntities())
                {
                    petOwner PetOwner = ctx.petOwners.Where(a => a.userId == iUserId && a.petId == ipetId && a.sharedUserId == isharedUserId).FirstOrDefault();
                    if (PetOwner == null)
                    {
                        logger.Error("No Owner request found for userId -"+ userId + " & petId -"+ petId + " by userId- "+ sharedUserId);
                        throw new CustomException("Recived Confirm Pet share request some of the properties empty", (int)ErrorCode.NORECORDFOUND);
                    }

                    ctx.petOwners.Attach(PetOwner);
                    PetOwner.isActive = true;
                    PetOwner.acceptedTime = DateTime.Now;
                    PetOwner.isMainOwner = false;
                    ctx.SaveChanges();
                    logger.Trace("Confirm Pet share request completed successfully");
                }

            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                logger.Error(MethodBase.GetCurrentMethod().Name + ": exception: " + ex.Message + ", " + ex.InnerException);
                throw new CustomException("SystemError", ex, (int)ErrorCode.PROCEESINGERROR);
            }
        }

    internal void SharePet(PetShareRequest petShareRequest)
        {
            logger.Trace("Recived Pet share request");
            try
            {
                if (String.IsNullOrEmpty(petShareRequest.username) || String.IsNullOrEmpty(petShareRequest.email) || petShareRequest.petId <= 0)
                {
                    logger.Error("Recived Pet share request");
                    throw new CustomException("Pet share request dosent have required fields", (int)ErrorCode.VALIDATIONFAILED);
                }
                CurrentUser currentUser = (CurrentUser)HttpContext.Current.User;
                using (var ctx = new PetWhizzEntities())
                {
                    user User = ctx.users.Where(a => a.eMail.ToLower() == petShareRequest.email.ToLower()).FirstOrDefault();
                    if (User==null)
                    {
                        logger.Error("Requested user details not matching for Petwhizz user email - "+ petShareRequest.email);
                        throw new CustomException("Requested user details not matching for Petwhizz user", (int)ErrorCode.USERNOTFOUND);
                    }
                    if (User.userName.ToLower() != petShareRequest.username.ToLower())
                    {
                        logger.Error("Requested user details not matching for Petwhizz user username - " + petShareRequest.username);
                        throw new CustomException("Requested user details not matching for Petwhizz user username", (int)ErrorCode.VALIDATIONFAILED);
                    }
                    pet Pet = ctx.pets.Where(a => a.id == petShareRequest.petId).FirstOrDefault();
                    if (Pet.petOwners.Where(a => a.userId == currentUser.userId && a.isActive == true && a.isMainOwner == true).FirstOrDefault() == null)
                    {
                        logger.Error("Requested user not belongs to the pet or not the main owner");
                        throw new CustomException("Requested user not belongs to the pet or not the main owner", (int)ErrorCode.UNAUTHORIZED);
                    }

                    petOwner PetOwner = ctx.petOwners.Where(a => a.petId == petShareRequest.petId && a.userId == User.id).FirstOrDefault();
                    if (PetOwner != null && PetOwner.isActive == true)
                    {
                        logger.Error("Pet is already shared with requestd user");
                        throw new CustomException("Pet is already shared with requestd user", (int)ErrorCode.ALREADYEXIST);
                    }
                    else if(PetOwner!=null && PetOwner.isActive==false)
                    {
                        logger.Error("Pet is already shared with this user but not yet confirmed.");
                        throw new CustomException("Pet is already shared with this user but not yet confirmed.", (int)ErrorCode.NOTCONFIRMED);
                    }
                    else
                    {
                        //share pet
                        PetOwner = new petOwner()
                        {
                            enteryDate = DateTime.Now,
                            isActive = false,
                            isMainOwner = false,
                            petId = petShareRequest.petId,
                            sharedTime = DateTime.Now,
                            sharedUserId = currentUser.userId,
                            acceptedTime = null,
                            userId = User.id
                        };
                        ctx.petOwners.Add(PetOwner);
                        ctx.SaveChanges();
                        // var EmailService = new EmailService();
                        SendPetSharingEmail(PetOwner);
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

        private void SendPetSharingEmail(petOwner PetOwner)
        {
            CurrentUser currentUser = (CurrentUser)HttpContext.Current.User;
            EmailService emailService = new EmailService();
            String emailBodyHtml = emailService.GetBodyHtml("PETSHARING");
            if (String.IsNullOrEmpty(emailBodyHtml))
            {
                logger.Error("Email template not found for type - EMAILVERIFICATION");
                throw new CustomException("Email type not found", (int)ErrorCode.EMAILERROR);
            }

            String VerificationToken = PetOwner.userId + "|" + PetOwner.petId+"|"+ PetOwner.sharedUserId+"|"+DateTime.Now.ToLongDateString();
            emailBodyHtml = emailBodyHtml.Replace("{UserName}", char.ToUpper(PetOwner.user.userName[0]) + PetOwner.user.userName.Substring(1))
                .Replace("{petName}", PetOwner.pet.petName)
                .Replace("{requestedUser}", char.ToUpper(currentUser.username[0]) + currentUser.username.Substring(1))
                .Replace("{VerifyLink}", HostedBaseUrl + "pet/confirmpetsharingrequest?token=" + Encryptor.Encrypt(VerificationToken).Replace("+", "%2b"));
            MailMessage email = new MailMessage()
            {
                From = new MailAddress("noreply@petwhizz.com"),
                Subject = "Pet sharing request on Petwhizz",
                Body = emailBodyHtml,
                IsBodyHtml = true,
            };
            email.To.Add(PetOwner.user.eMail);
            AlternateView view = emailService.EmbedLogosForEmailBody(email.Body);
            email.AlternateViews.Add(view);
            emailService.SendEmail(email);
        }

        internal void DeletePet(PetDeleteRequest petDeleteRequest)
        {
            logger.Trace("Recived get pets delete request");
            try
            {
                CurrentUser currentUser = (CurrentUser)HttpContext.Current.User;
                petDeleteRequest.petId = Decryptor.Decrypt(petDeleteRequest.petId).Split('|')[1];
                Int64 petId = Convert.ToInt64(petDeleteRequest.petId);
                if (String.IsNullOrEmpty(petDeleteRequest.petId))
                {
                    logger.Error("Pet id not found on request");
                    throw new CustomException("Pet id not found on request", (int)ErrorCode.VALIDATIONFAILED);
                }
                using (var ctx = new PetWhizzEntities())
                {
                    pet Pet = ctx.pets.Where(a => a.id == petId).FirstOrDefault();
                    if (Pet.petOwners.Where(a => a.userId == currentUser.userId && a.isActive == true && a.isMainOwner == true).FirstOrDefault() == null)
                    {
                        logger.Error("Requested user not belongs to the pet or not the main owner");
                        throw new CustomException("Requested user not belongs to the pet or not the main owner", (int)ErrorCode.UNAUTHORIZED);
                    }
                    //delete pet
                    ctx.pets.Attach(Pet);
                    Pet.isActive = false;
                    Pet.isDeleted = true;
                    ctx.SaveChanges();
                    logger.Trace("Pet successfully deleted");
                }
            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                logger.Error(MethodBase.GetCurrentMethod().Name + ": exception: " + ex.Message + ", " + ex.InnerException);
                throw new CustomException("SystemError", ex, (int)ErrorCode.PROCEESINGERROR);
            }
        }
        internal PetsResponse Pets()
        {
            PetsResponse PetsResponse = new PetsResponse();
            logger.Trace("Recived get pets request");
            try
            {
                CurrentUser currentUser = (CurrentUser)HttpContext.Current.User;

                using (var ctx = new PetWhizzEntities())
                {

                    PetsResponse.petList = ctx.petOwners
                        .Where(a => a.userId == currentUser.userId && a.isActive==true)
                        .Select(b => new
                        {
                            b.pet.id,
                            b.pet.petName,
                            birthDay = b.pet.birthDay.ToString(),
                            b.pet.breedId,
                            b.pet.sex,
                            b.pet.animalBreed.breedName,
                            b.pet.animalBreed.animal.animalName,
                            b.pet.profileImage,
                            b.pet.coverImage,
                            b.pet.isActive,
                            b.pet.isDeleted,
                            userCount = b.pet.petOwners.Count

                        }).ToArray();
                };
            }
            catch (CustomException) { throw; }
            catch (Exception ex)
            {
                logger.Error(MethodBase.GetCurrentMethod().Name + ": exception: " + ex.Message + ", " + ex.InnerException);
                throw new CustomException("SystemError", ex, (int)ErrorCode.PROCEESINGERROR);
            }
            return PetsResponse;
        }
        internal void PetEnrollment(PetEnrollmentRequest petEnrollmentRequest)
        {
            logger.Trace("Recived pet enroll request");
            try
            {
                CurrentUser currentUser = (CurrentUser)HttpContext.Current.User;
                if (String.IsNullOrEmpty(petEnrollmentRequest.petName) || petEnrollmentRequest.breedId == 0)
                {
                    logger.Error("Required fiedls not found on pet emrollment reuest");
                    throw new CustomException("Breed or Pet Name is empty", (int)ErrorCode.VALIDATIONFAILED);
                }
                //creating pet object
                pet Pet = new pet()
                {
                    profileImage = petEnrollmentRequest.profileImage,
                    coverImage = petEnrollmentRequest.coverImage,
                    sex = petEnrollmentRequest.sex,
                    birthDay = petEnrollmentRequest.birthDay,
                    breedId = petEnrollmentRequest.breedId,
                    entryDate = DateTime.Now,
                    isActive = true,
                    isDeleted = false,
                    petName = petEnrollmentRequest.petName
                };
                using (var ctx = new PetWhizzEntities())
                {
                    using (var dbContextTransaction = ctx.Database.BeginTransaction())
                    {
                        try
                        {
                            ctx.pets.Add(Pet);
                            ctx.SaveChanges();

                            petOwner PetOwner = new petOwner()
                            {
                                enteryDate = DateTime.Now,
                                petId = Pet.id,
                                userId = currentUser.userId,
                                isActive = true,
                                isMainOwner = true

                            };
                            ctx.petOwners.Add(PetOwner);
                            ctx.SaveChanges();
                            dbContextTransaction.Commit();
                        }
                        catch (Exception)
                        {
                            dbContextTransaction.Rollback();
                            logger.Error("DB error occure when enrolling Pet");
                            throw new CustomException("Pet Enrolling failed", (int)ErrorCode.PROCEESINGERROR);
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

    }
}