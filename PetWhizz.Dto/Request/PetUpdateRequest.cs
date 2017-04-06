using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetWhizz.Dto.Request
{
    public class PetUpdateRequest
    {
        public int id;
        public int breedId;
        public string petName;
        public DateTime birthDay;
        public string profileImage;
        public string coverImage;
        public string sex;
        public bool isActive;
    }
}
