using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CareerExpansionMod.CEM
{
    public class Transfer
    {
        public int PlayerId { get; set; }
        public int TeamIdFrom { get; set; }
        public int TeamIdTo { get; set; }
        public int TransferAmount { get; set; }
        public int Date { get; set; }
    }
}
