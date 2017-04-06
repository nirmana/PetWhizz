using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetWhizz.Dto.Request
{
    public class UserUpdateRequest
    {
        public string email;
        public string mobileNumber;
        public string addressLine1;
        public string addressLine2;
        public string firstName;
        public string lastName;
        public string middleName;
        public DateTime dateOfBirth;
        public string profilePic;
    }
}
