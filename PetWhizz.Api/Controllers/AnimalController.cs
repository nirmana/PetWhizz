using PetWhizz.Api.Security;
using PetWhizz.Api.Services;
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
using System.Web.Http;

namespace PetWhizz.Api.Controllers
{
    public class AnimalController : ApiController
    {
        AnimalService animalService;
        public AnimalController()
        {
            animalService = new AnimalService();
        }

        [Route("animal/breed/filter")]
        [HttpPost]
        [PetWhizzAuthorize]
        public PetWhizzResponse AnimalBreedFilter(AnimalBreedFilterRequest AnimalBreedFilterRequest)
        {
            PetWhizzResponse _oResponse;
            try
            {
                AnimalBreedFilterResponse AnimalBreedFilterResponse = animalService.AnimalBreedFilter(AnimalBreedFilterRequest);
                _oResponse = Utils.CreateSuccessResponse(AnimalBreedFilterResponse);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }

        [Route("pet/enroll")]
        [HttpPost]
        [PetWhizzAuthorize]
        public PetWhizzResponse PetEnrollment(PetEnrollmentRequest PetEnrollmentRequest)
        {
            PetWhizzResponse _oResponse;
            try
            {
                animalService.PetEnrollment(PetEnrollmentRequest);
                _oResponse = Utils.CreateSuccessResponse(null);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }

        [Route("pets")]
        [HttpGet]
        [PetWhizzAuthorize]
        public PetWhizzResponse Pets()
        {
            PetWhizzResponse _oResponse;
            try
            {
                PetsResponse PetsResponse=animalService.Pets();
                _oResponse = Utils.CreateSuccessResponse(PetsResponse);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }

        [Route("pet/update")]
        [HttpPut]
        [PetWhizzAuthorize]
        public PetWhizzResponse UpdatePet(PetUpdateRequest PetUpdateRequest)
        {
            PetWhizzResponse _oResponse;
            try
            {
                animalService.UpdatePet(PetUpdateRequest);
                _oResponse = Utils.CreateSuccessResponse(null);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }

        [Route("pet/delete")]
        [HttpDelete]
        [PetWhizzAuthorize]
        public PetWhizzResponse DeletePet(PetDeleteRequest PetDeleteRequest)
        {
            PetWhizzResponse _oResponse;
            try
            {
                animalService.DeletePet(PetDeleteRequest);
                _oResponse = Utils.CreateSuccessResponse(null);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }

        [Route("pet/share")]
        [HttpPost]
        [PetWhizzAuthorize]
        public PetWhizzResponse SharePet(PetShareRequest PetShareRequest)
        {
            PetWhizzResponse _oResponse;
            try
            {
                animalService.SharePet(PetShareRequest);
                _oResponse = Utils.CreateSuccessResponse(null);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }

        [Route("pet/confirmpetsharingrequest")]
        [HttpGet]
        public HttpResponseMessage ConfirmPetSharingRequest(string token)
        {
            var response = new HttpResponseMessage();
            try
            {
                animalService.ConfirmPetSharingRequest(token);
                response.Content = new StringContent("Pet sharing request confirmed Successful!");
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

        [Route("pet/users")]
        [HttpPost]
        [PetWhizzAuthorize]
        public PetWhizzResponse GetUsersByPet(PetUserRequest PetUserRequest)
        {
            PetWhizzResponse _oResponse;
            try
            {
                PetUserResponse PetUserResponse= animalService.GetUsersByPet(PetUserRequest);
                _oResponse = Utils.CreateSuccessResponse(PetUserResponse);
            }
            catch (Exception ex)
            {
                _oResponse = Utils.CreateErrorResponse(ex);
            }
            return _oResponse;
        }
    }
}
