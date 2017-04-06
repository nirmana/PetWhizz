using PetWhizz.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetWhizz.Dto.Response
{
    public class UserLoginResponse
    {
        public string email;
        public string status;
        public string username;
        public string token;
        public string userId;
    }
}
