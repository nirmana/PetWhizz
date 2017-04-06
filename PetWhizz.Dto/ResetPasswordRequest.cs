using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetWhizz.Dto
{
    public class ResetPasswordRequest
    {
        public string token;
        public string newPassowrd;
    }
}
