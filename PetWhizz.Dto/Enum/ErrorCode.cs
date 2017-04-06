using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetWhizz.Dto.Enum
{
    public enum ErrorCode
    {
        UNAUTHORIZED = -1,
        TOKENEXPIRED=-2,
        VALIDATIONFAILED=-3,
        PROCEESINGERROR=-4,
        EMAILERROR=-5,
        EMAILORUSERNAMEALREADYEXIST=-6,
        CRYPTOGRAPHICFAILED = -7,
        INVALIDCREDENTIALS = -8,
        LOGINFAILURE = -9,
        USERNOTFOUND = -10,
        NORECORDFOUND=-11,
        ALREADYEXIST = -12,
        NOTCONFIRMED = -13
    }
}
