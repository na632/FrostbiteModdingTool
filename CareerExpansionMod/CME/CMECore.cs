using CareerExpansionMod.CME.FIFA;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using v2k4FIFAModding.Career.CME.FIFA;
using v2k4FIFAModdingCL.CGFE;
using v2k4FIFAModdingCL.MemHack.Career;
using v2k4FIFAModdingCL.MemHack.Core;

namespace CareerExpansionMod.CME
{
    public class CMECore
    {
        public static CMECore CMECoreInstance;
        public CMECore()
        {

            // Initialize FIFA Leagues CSV to Enumerable
            FIFALeague.GetFIFALeagues();


            // Setup Singleton
            CMECoreInstance = this;
        }

        public void CoreHack_EventGameDateChanged(DateTime oldDate, DateTime newDate)
        {
            GameDateChanged(oldDate, newDate);
        }

        public CoreHack CoreHack = new CoreHack();
        public Finances Finances = new Finances();
        public Manager Manager = new Manager();

        public CareerFile CareerFile = null;
       


        internal void GameDateChanged(DateTime oldDate, DateTime newDate)
        {
            if (CreateCopyOfSave())
            {
                if (SetupCareerFile())
                {


                    // Refresh
                    Finances.GetTransferBudget();
                }
            }

        }

        private bool SetupCareerFile()
        {
            var baseDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            var dataFolder = baseDir + "\\Data\\";
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            CareerFile = new CareerFile(dataFolder + CoreHack.SaveFileName, dataFolder + "fifa_ng_db-meta.XML");
            CareerFile.LoadXml(dataFolder + "fifa_ng_db-meta.XML");
            CareerFile.LoadEA(dataFolder + CoreHack.SaveFileName);
            // Setup Internal Career Entities
            CareerDB1.Current = new CareerDB1();
            CareerDB2.Current = new CareerDB2();


            var ds1 = CareerFile.ConvertToDataSet();
            var usersDt = CareerFile.Databases[0].Table[3].ConvertToDataTable();
            CareerDB1.Current.career_users = new List<FIFAUsers>() { CreateItemFromRow<FIFAUsers>(usersDt.Rows[0]) };
            return true;
        }

        static string tableNameToSearch = "career_users";

        // function that creates an object from the given data row
        public static T CreateItemFromRow<T>(DataRow row) where T : new()
        {
            // create a new object
            T item = new T();

            // set the item
            SetItemFromRow(item, row);

            // return 
            return item;
        }

        public static void SetItemFromRow<T>(T item, DataRow row) where T : new()
        {
            // go through each column
            foreach (DataColumn c in row.Table.Columns)
            {
                // find the property for the column
                //PropertyInfo p = item.GetType().GetProperty(c.ColumnName);
                PropertyInfo p = item.GetType().GetProperty(GetColumnLongNameFromShortName(c.ColumnName));


                // if exists, set the value
                if (p != null && row[c] != DBNull.Value)
                {
                    if(int.TryParse(row[c].ToString(), out int iV))
                        p.SetValue(item, iV, null);
                    else
                        p.SetValue(item, row[c], null);

                }
            }
        }

        public static string GetColumnLongNameFromShortName(string shortName)
        {
            var baseDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            var dataFolder = baseDir + "\\CME\\Data\\";
            var xmlMetaData = dataFolder + "fifa_ng_db-meta.XML";

            // Loading from a file, you can also load from a stream
            var xml = XDocument.Load(xmlMetaData);

            var table = from c in xml.Root.Descendants("table")
                               where (string)c.Attribute("name") == tableNameToSearch
                        select c;

            var columnName = from c in table.Descendants()
                              where (string)c.Attribute("shortname") == shortName
                              select (string)c.Attribute("name");

            // Query the data and write out a subset of contacts
            //var query = from c in tables
            //            where c.Descendants. (string)c.Attribute("shortname") == shortName
            //            select c;


            //return query.FirstOrDefault().ToString();
            return columnName.FirstOrDefault();
        }

        private bool CreateCopyOfSave()
        {
            var baseDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            var dataFolder = baseDir + "\\Data\\";
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);
            // backup save file
            var myDocuments = Microsoft.VisualBasic.FileIO.SpecialDirectories.MyDocuments + "\\FIFA 20\\settings\\";
#pragma warning disable CS0162 // Unreachable code detected
            for (int iAttempt = 0; iAttempt < 5; iAttempt++)
#pragma warning restore CS0162 // Unreachable code detected
            {
                try
                {
                    File.Copy(myDocuments + CoreHack.SaveFileName, dataFolder + CoreHack.SaveFileName, true);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
    }
}
