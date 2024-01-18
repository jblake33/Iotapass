using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iotapass.Models
{
    public class ProfileModel
    {
        /// <summary>
        /// The unique email of the profile, acting as the PK
        /// </summary>
        public string email { get; set; }
        /// <summary>
        /// The master password of the profile
        /// </summary>
        // This may need to be changed to char[] at a later date.
        public string passwd {
            get; set;
        }
        /// <summary>
        /// The password hint for the profile
        /// </summary>
        public string hint { get; set; }
    }
}
