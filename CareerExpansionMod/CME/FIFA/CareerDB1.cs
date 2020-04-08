using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CareerExpansionMod.CME.FIFA
{
    public class CareerDB1
    {
        public static CareerDB1 Current;
        public IEnumerable<FIFAUsers> career_users { get; set; }
    }
}
