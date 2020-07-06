using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CareerExpansionMod.CEM.Finances
{
    public class Payment : ISavedJsonData
    {
        public int TeamId { get; set; }
        public int PaymentValue { get; set; }

        public Debt LinkedDebt { get; set; }

        public Loan LinkedLoan { get; set; }

        public Transfer LinkedTransfer { get; set; }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}
