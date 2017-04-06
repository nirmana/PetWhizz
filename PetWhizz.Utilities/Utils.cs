using Newtonsoft.Json;
using NLog;
using PetWhizz.Dto.Common;
using PetWhizz.Dto.CustomException;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace PetWhizz.Utilities
{
    public class Utils
    {
        private static Logger m_Logger = LogManager.GetCurrentClassLogger();


        #region JSONSerialize

        /// <summary>
        /// Deserialize the response
        /// </summary>
        /// <param name="_oResponse">Response</param>
        /// <returns>Deserialized xml</returns>
        public static String JSONSerialize(Object oResponse)
        {
            String strReturn = "";

            try
            {
                strReturn = JsonConvert.SerializeObject(oResponse);
            }
            catch (Exception ex)
            {
                m_Logger.Error(MethodBase.GetCurrentMethod().Name + ":" + " exception: " + ex.Message + ", " + ex.InnerException);
                strReturn = ex.Message;
            }

            return (strReturn);
        }

        #endregion

        #region JSONDeserialize

        /// <summary>
        /// Deserialize the response
        /// </summary>
        /// <param name="_oResponse">Response</param>
        /// <returns>Deserialized xml</returns>
        public static T JsonDeserialize<T>(string jsonString)
        {
            T obj = default(T);

            try
            {
                obj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch (Exception ex)
            {
                String strReturn = ex.Message;
                m_Logger.Error(MethodBase.GetCurrentMethod().Name + ":" + " exception: " + ex.Message + ", " + ex.InnerException);
            }

            return obj;
        }

        #endregion JSONDeserialize


        public static PetWhizzResponse CreateSuccessResponse(Object response)
        {
            return new PetWhizzResponse()
            {
                Message = "Success",
                Code = 0,
                Object = response == null ? "null" : JSONSerialize(response),
                ObjectType = response == null ? "null" : response.GetType().Name
            };
        }
        public static PetWhizzResponse CreateErrorResponse(Exception ex)
        {
            string message = "";
            int code = -1;
            var t = ex.GetType().FullName;
            bool isSystemException = (t.StartsWith("System."));

            if (!isSystemException)
            {
                code = ((CustomException)ex).errorCode;
                if (ex.Message.Equals("SystemError"))
                {
                    message = ex.Message + ":" + ex.InnerException.Message;
                }
                else
                {
                    message = ex.Message;
                }
            }
            else
            {
                message = ex.Message;
            }

            return new PetWhizzResponse()
            {
                Message = message,
                Code = code,
                Object = null,
                ObjectType = null
            };
        }
    }


}