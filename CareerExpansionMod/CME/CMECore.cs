using CareerExpansionMod.CME.FIFA;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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
        public static string MyDocumentsDirectory
        {
            get
            {
                var myDocuments = Microsoft.VisualBasic.FileIO.SpecialDirectories.MyDocuments + "\\FIFA 20\\";
                return myDocuments;
            }
        }

        public static string MyDocumentsSavesDirectory
        {
            get
            {
                var myDocuments = Microsoft.VisualBasic.FileIO.SpecialDirectories.MyDocuments + "\\FIFA 20\\settings\\";
                return myDocuments;
            }
        }

        public static string CMEMyDocumentsDirectory
        {
            get
            {
                var myDocuments = Microsoft.VisualBasic.FileIO.SpecialDirectories.MyDocuments + "\\FIFA 20\\CME\\";
                Directory.CreateDirectory(myDocuments);
                return myDocuments;
            }
        }

        public static string CMEMyDocumentsDbSaveDirectory
        {
            get
            {
                var saveLocation = CMECore.CMECoreInstance != null && CMECore.CMECoreInstance.CoreHack != null && !string.IsNullOrEmpty(CMECore.CMECoreInstance.CoreHack.SaveName)
                    ? CMECore.CMECoreInstance.CoreHack.SaveName : "Unknown";
                var datalocation = CMECore.CMEMyDocumentsDirectory + $"\\Data\\CME\\DB\\{saveLocation}\\";
                Directory.CreateDirectory(datalocation);
                return datalocation;

            }
        }

        public static string CMEInternalDataDirectory
        {
            get
            {
                var baseDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
                var dataFolder = baseDir + "\\CME\\Data\\";
                if (!Directory.Exists(dataFolder))
                    Directory.CreateDirectory(dataFolder);

                return dataFolder;
            }
        }

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
        public v2k4FIFAModdingCL.MemHack.Career.Finances Finances = new v2k4FIFAModdingCL.MemHack.Career.Finances();
        public Manager Manager = new Manager();

        public CareerFile CareerFile = null;
       


        internal void GameDateChanged(DateTime oldDate, DateTime newDate)
        {
            if (CreateCopyOfSave())
            {
                if (SetupCareerFile())
                {


                    // Refresh
                    v2k4FIFAModdingCL.MemHack.Career.Finances.GetTransferBudget();
                }
            }

        }

        public string CurrentSaveFileName;

        private bool SetupCareerFile()
        {
            

            //var CareerFileThread = new Thread(() =>
            //{

            if (CareerFile == null)
            {
                CareerFile = new CareerFile(CMEInternalDataDirectory + CoreHack.SaveFileName, CMEInternalDataDirectory + "fifa_ng_db-meta.XML");
                //CareerFile.LoadXml(dataFolder + "fifa_ng_db-meta.XML");
                //CareerFile.LoadEA(dataFolder + CoreHack.SaveFileName);
                // Setup Internal Career Entities
                CareerDB1.Current = new CareerDB1();
                CareerDB1.Current.ParentDataSet = CareerFile.Databases[0].ConvertToDataSet();
                CareerDB2.Current = new CareerDB2();
                CareerDB2.Current.ParentDataSet = CareerFile.Databases[1].ConvertToDataSet();
                CurrentSaveFileName = CoreHack.SaveFileName;
            }
            //});
            //CareerFileThread.Start();
            //CareerFileThread.Join();

            if (CareerFile != null && CareerFile.Databases.Length > 0)
            {


                if (CareerDB1.FIFAUser == null)
                {
                    var usersDt = CareerFile.Databases[0].Table[3].ConvertToDataTable();
                    // Read User. Set ClubTeamId to 1 less. Don't know why I have to do this!
                    CareerDB1.FIFAUser = CreateItemFromRow<FIFAUsers>(usersDt.Rows[0]);
                }

                //Thread teamThread = new Thread(() =>
                //{


                if (CareerDB2.Current.teams == null || CareerDB1.UserTeam == null)
                {
                    var dbTeams = CareerFile.Databases[1].GetTable("teams");
                    CareerDB2.Current.teams = dbTeams.ConvertToDataTable();
                    var firstteam = CareerDB2.Current.teams.Rows[0];
                    var teams = (from myRow in CareerDB2.Current.teams.AsEnumerable()
                                 where myRow.Field<int>("teamid") == CareerDB1.FIFAUser.clubteamid
                                 select myRow);
                    var team = teams.FirstOrDefault();
                    if (team != null)
                        CareerDB1.UserTeam = CreateItemFromRow<FIFATeam>(team);

                }
                //});
                //teamThread.Start();

                if (CareerDB2.Current.leagueteamlinks == null)
                {
                    //tableNameToSearch = "leagueteamlinks";
                    CareerDB2.Current.leagueteamlinks = CareerFile.Databases[1].GetTable("leagueteamlinks").ConvertToDataTable().AsEnumerable();
                }

            }

            //teamThread.Join();

            return true;
        }

        static string tableNameToSearch = "career_users";

        // function that creates an object from the given data row
        public static T CreateItemFromRow<T>(DataRow row, string tableShortName = null) where T : new()
        {
            // create a new object
            T item = new T();

            // set the item
            SetItemFromRow(item, row, tableShortName);

            // return 
            return item;
        }

        public static void SetItemFromRow<T>(T item, DataRow row, string tableShortName = null) where T : new()
        {
            // go through each column
            foreach (DataColumn c in row.Table.Columns)
            {
                
                    PropertyInfo p = item.GetType().GetProperty(c.ColumnName);


                    // if exists, set the value
                    if (p != null && row[c] != DBNull.Value)
                    {
                       
                            p.SetValue(item, row[c], null);

                    }
            }
        }

        public static string GetColumnLongNameFromShortName(string tableShortName, string shortName)
        {
            var tableLongName = GetTableLongNameFromShortName(tableShortName);

            var baseDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            var dataFolder = baseDir + "\\CME\\Data\\";
            var xmlMetaData = dataFolder + "fifa_ng_db-meta.XML";

            // Loading from a file, you can also load from a stream
            var xml = XDocument.Load(xmlMetaData);

            var table = from c in xml.Root.Descendants("table")
                        where (string)c.Attribute("name") == (tableLongName != null ? tableLongName : tableNameToSearch)
                        select c;

            var columnName = from c in table.Descendants()
                             where (string)c.Attribute("shortname") == shortName
                             select (string)c.Attribute("name");

            
         return columnName.FirstOrDefault();

        }

        public static string GetTableLongNameFromShortName(string shortName)
        {
            var baseDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            var dataFolder = baseDir + "\\CME\\Data\\";
            var xmlMetaData = dataFolder + "fifa_ng_db-meta.XML";

            // Loading from a file, you can also load from a stream
            var xml = XDocument.Load(xmlMetaData);

            var table = from c in xml.Root.Descendants("table")
                        where (string)c.Attribute("shortname") == shortName
                        select (string)c.Attribute("name");


            return table.FirstOrDefault();
        }

        private bool CreateCopyOfSave()
        {
            
            // backup save file
            var myDocuments = Microsoft.VisualBasic.FileIO.SpecialDirectories.MyDocuments + "\\FIFA 20\\settings\\";
#pragma warning disable CS0162 // Unreachable code detected
            for (int iAttempt = 0; iAttempt < 5; iAttempt++)
#pragma warning restore CS0162 // Unreachable code detected
            {
                try
                {
                    File.Copy(myDocuments + CoreHack.SaveFileName, CMEInternalDataDirectory + CoreHack.SaveFileName, true);
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
