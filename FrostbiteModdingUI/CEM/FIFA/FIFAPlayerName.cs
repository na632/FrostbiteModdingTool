using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using v2k4FIFAModding.Career.CME.FIFA;

namespace CareerExpansionMod.CEM.FIFA
{
    public class FIFAPlayerName
    {
        public string name { get; set; }
        public string nameid { get; set; }
        public string commentaryid { get; set; }

        public static IEnumerable<FIFAPlayerName> FIFAPlayerNames;
        public static IEnumerable<FIFAPlayerName> GetFIFAPlayerNames()
        {
            if (FIFAPlayerNames != null)
                return FIFAPlayerNames;

            var pnames = new List<FIFAPlayerName>();

            var dlllocation = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            var fulllocation = dlllocation + "\\CEM\\Data\\playernames.csv";
            using (var reader = new StreamReader(fulllocation))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                pnames = csv.GetRecords<FIFAPlayerName>().ToList();
            }

            // DLC / Squad File Names
            fulllocation = dlllocation + "\\CEM\\Data\\dcplayernames.csv";
            using (var reader = new StreamReader(fulllocation))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.HeaderValidated = null;
                csv.Configuration.MissingFieldFound = null;
                pnames.AddRange(csv.GetRecords<FIFAPlayerName>().ToList());
            }

            FIFAPlayerNames = pnames;

            return FIFAPlayerNames;


        }

        //public static IDictionary<int,string> NamesInMemory = new 

        public static string GetNameFromFIFAPlayer(FIFAPlayer player)
        {
            var lstNames = GetFIFAPlayerNames();

            var firstname = lstNames.FirstOrDefault(x => x.nameid == player.firstnameid.ToString());
            var lastname = lstNames.FirstOrDefault(x => x.nameid == player.lastnameid.ToString());
            if (firstname != null && !string.IsNullOrEmpty(firstname.name) && lastname != null && !string.IsNullOrEmpty(lastname.name))
                return firstname.name + " " + lastname.name;

            var editplayername = CareerDB2.Current.editedplayernames.FirstOrDefault(x => x["playerid"].ToString() == player.playerid.ToString());
            if (editplayername != null)
                return editplayername["firstname"] + " " + editplayername["surname"];

            return string.Empty;
        }

    }
}
