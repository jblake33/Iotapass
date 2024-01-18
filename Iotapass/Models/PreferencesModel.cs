using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iotapass.Models
{
    public class PreferencesModel
    {
        /// <summary>
        /// The email of the profile that has these saved preferences.
        /// </summary>
        public string email { get; set; }
        /// <summary>
        /// The display resolution. Format is "600x480" for a 600 width, 480 height window.
        /// </summary>
        public string resolution { get; set; }
        /// <summary>
        /// The font size. 1 = small, 2 = normal, 3 = large
        /// </summary>
        public int fontsize { get; set; }
        /// <summary>
        /// The color theme. 1 = light, 2 = neutral, 3 = dark
        /// </summary>
        public int theme { get; set; }
    }
}
