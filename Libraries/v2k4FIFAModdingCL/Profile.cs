using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace v2k4FIFAModding
{
    public class Profile
    {
        public string ProfileName;

        public Profile(string profileName)
        {
            this.ProfileName = profileName;
        }
    }

    public class ProfileManager
    {
        public static Profile CurrentProfile = new Profile("Default");

        public List<Profile> GetProfiles()
        {
            return null;
        }
    }
}
