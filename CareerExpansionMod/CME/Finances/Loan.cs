using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CareerExpansionMod.CME.Finances
{
    public class Loan
    {
        public int TeamId { get; set; }
        public int LoanLength { get; set; }
        public int LoanAmount { get; set; }

        private int? paybackintervalindays;
        public int LoanPaybackIntervalInDays
        {
            get
            {
                if (paybackintervalindays == null)
                    return 28;

                return paybackintervalindays.Value;
            }
            set
            {
                paybackintervalindays = value;
            }
        }


    }
}
