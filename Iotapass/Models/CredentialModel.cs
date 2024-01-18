using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iotapass.Models
{
    public class CredentialModel
    {

        /// <summary>
        /// The email of the profile that has saved this credential.
        /// </summary>
        public string email { get; set; }
        /// <summary>
        /// The credential's unique internal ID.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The credential's username.
        /// </summary>
        public string username { get; set; }
        /// <summary>
        /// The credential's password.
        /// </summary>
        public string passwd { get; set; }
        /// <summary>
        /// The credential's website.
        /// </summary>
        public string website { get; set; }
        /// <summary>
        /// True (1) if the credential has been found in a breach. Set to false (0) if updated.
        /// </summary>
        public int isBreached { get; set; }
        /// <summary>
        /// The credential's optional notes.
        /// </summary>
        public string notes { get; set; }

    }
}
