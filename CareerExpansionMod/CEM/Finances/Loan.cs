using CareerExpansionMod.CEM.FIFA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using v2k4FIFAModding.Career.CME.FIFA;

namespace CareerExpansionMod.CEM.Finances
{
    public class Loan
    {
        public int TeamId { get; set; }
        public int LoanLength { get; set; }
        public int LoanAmount { get; set; }

        public double InterestRate { get; set; }

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

        public static int CalculateLoanAmountForTeam(int? TeamId = null)
        {
            var loanAmount = 0;
            if (TeamId.HasValue && TeamId.Value != v2k4FIFAModdingCL.MemHack.Career.Finances.TeamId)
            {
                return 0;
            }

            // Assume is user
            var seasonalStartingBudget = v2k4FIFAModdingCL.MemHack.Career.Finances.StartingBudget;
            // > 100m
            if (seasonalStartingBudget > 100000000)
                loanAmount = 100000000;
            // > 50m
            else if(seasonalStartingBudget > 50000000)
                loanAmount = 50000000;
            // > 25m
            else if (seasonalStartingBudget > 25000000)
                loanAmount = 25000000;
            // > 10m
            else if (seasonalStartingBudget > 10000000)
                loanAmount = 10000000;
            // > 5m
            else if (seasonalStartingBudget > 5000000)
                loanAmount = 5000000;
            // > 1m
            else if (seasonalStartingBudget > 1000000)
                loanAmount = 1000000;
            // > 750k
            else if (seasonalStartingBudget > 750000)
                loanAmount = 750000;
            // > 500k
            else if (seasonalStartingBudget > 500000)
                loanAmount = 500000;
            // > 250k
            else if (seasonalStartingBudget > 250000)
                loanAmount = 250000;
            // > 100k
            else
                loanAmount = 100000;


            return loanAmount;
        }

        public static int CalculateLoanCostPerMonth(int? TeamId, int loanAmount, double interestRate, int loanPeriod)
        {
            var rateOfInterest = interestRate / 1000;
            //var answer = (loanAmount * interestRate) / (1.0 - Math.Pow(1.0 / 1.0 + interestRate, loanPeriod));
            var paymentAmount = (rateOfInterest * loanAmount) / (1 - Math.Pow(1 + rateOfInterest, loanPeriod * -1));
            return Convert.ToInt32(Math.Round(paymentAmount));
        }

        public IEnumerable<Loan> GetLoansForTeam(int? TeamId)
        {
            if (!TeamId.HasValue)
                return TeamFinance.Load().Loans.ToList();


            return null;

        }


    }
}
