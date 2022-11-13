using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using v2k4FIFAModding.Career.CME.FIFA;

namespace CareerExpansionMod.CEM.FIFA
{
    public class CareerDB1
    {
        
        public DataSet ParentDataSet { get; set; }

        public static CareerDB1 Current;
        public static FIFAUsers FIFAUser { get; set; }
        public static FIFATeam UserTeam { get; set; }

        private static List<FIFAPlayer> _userPlayers;
        public static List<FIFAPlayer> UserPlayers 
        { 
            get 
            {
                if (UserTeam == null)
                    throw new Exception("User Team must be set to get User Players");

                return _userPlayers = UserTeam.GetPlayers();
            } 
            set
            {
                _userPlayers = value;
            }
        }
    }
}
