using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PetWhizz.Dto.CustomException
{
    public class CustomException : Exception
    {
        public int errorCode { get; set; }
        public CustomException(String messege, int errorCode) : base(messege)
        {
            this.errorCode = errorCode;
        }
        public CustomException(String messege, Exception innerException, int errorCode)
            : base(messege, innerException)
        {
            this.errorCode = errorCode;
        }
    }
}
