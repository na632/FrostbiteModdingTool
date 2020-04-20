using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CareerExpansionMod.CME
{
    public static class CMEUtilities
    {
        public static string FormatCurrencyNumber(int num)
        {
            if (num >= 1000000)
            {

                return (num / 1000000D).ToString("0.#") + "M";

            }
            else if (num >= 100000)
                return FormatCurrencyNumber(num / 1000) + "K";
            else if (num >= 10000)
            {
                return (num / 1000D).ToString("0.#") + "K";
            }
            return num.ToString("#,0");
        }
    }
}
