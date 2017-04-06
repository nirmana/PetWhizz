using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetWhizz.Dto.Response
{
    public class PetUserResponse
    {
        public int userCount=0;
        public List<PetUser> PetUserList;
    }

    public class PetUser
    {
        public int? userId;
        public String userName;
        public String profilePic;
        public bool? isConfirmed;
    }
}
