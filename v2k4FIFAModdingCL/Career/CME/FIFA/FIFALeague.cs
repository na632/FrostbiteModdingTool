using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace v2k4FIFAModding.Career.CME.FIFA
{
    public class FIFALeague
    {
        public int countryid{ get; set; }
        public int leaguename { get; set; }
        public int leaguetype { get; set; }
        public int level { get; set; }
        public int iscompetitionscarfenabled { get; set; }
        public int isbannerenabled { get; set; }
        public int leagueid { get; set; }
        public int iscompetitionpoleflagenabled { get; set; }
        public int iscompetitioncrowdcardsenabled { get; set; }
        public int leaguetimeslice { get; set; }
        public int iswithintransferwindow { get; set; }
        public static IEnumerable<FIFALeague> GetFIFALeagues()
        {
            var dlllocation = Directory.GetParent(Assembly.GetExecutingAssembly().Location);

            using (var reader = new StreamReader(dlllocation + "\\Career\\CME\\Data\\leagues.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<FIFALeague>();
                return records;
            }

        }
    }
}
