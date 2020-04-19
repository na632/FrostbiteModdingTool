using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using v2k4FIFAModding.Career.CME.FIFA;

namespace CareerExpansionMod.CME.FIFA
{
    public class CareerDB1
    {
        
        public DataSet ParentDataSet { get; set; }

        public static CareerDB1 Current;
        public static FIFAUsers FIFAUser { get; set; }
        public static FIFATeam UserTeam { get; set; }
    }
}
