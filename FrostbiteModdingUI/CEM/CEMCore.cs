using CareerExpansionMod.CEM.FIFA;
using FifaLibrary;
using FrostbiteModdingUI.CEM;
using FrostbiteSdk;
using Frosty;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using v2k4FIFAModding.Career.CME.FIFA;
using v2k4FIFAModdingCL.MemHack.Core;

namespace CareerExpansionMod.CEM
{
    public class CEMCore : IDisposable
    {
        private ILogger logger;
        public ILogger Logger { get { if (logger == null) logger = new NullLogger(); return logger; } set { logger = value; } }

        private ICEMVisualizer visualizer;
        public ICEMVisualizer Visualizer { get { if (visualizer == null) visualizer = new NullCEMVisualizer(); return visualizer; } set { visualizer = value; } }

        public static string GetApplicationDirectory()
        {
            var runningLocation = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            return runningLocation;
        }

        public static string GetFIFAMyDocumentsDirectory()
        {
            var directory = Microsoft.VisualBasic.FileIO.SpecialDirectories.MyDocuments + "\\FIFA 21\\";
            switch (CoreHack.FIFAProcessName)
            {
                case "FIFA20":
                    directory = Microsoft.VisualBasic.FileIO.SpecialDirectories.MyDocuments + "\\FIFA 20\\";
                    break;
                case "FIFA21":
                    directory = Microsoft.VisualBasic.FileIO.SpecialDirectories.MyDocuments + "\\FIFA 21\\";
                    break;
            }
            return directory;
        }

        public static string MyDocumentsDirectory
        {
            get
            {
                //var myDocuments = Microsoft.VisualBasic.FileIO.SpecialDirectories.MyDocuments + "\\FIFA 20\\";
                var myDocuments = GetFIFAMyDocumentsDirectory();
                return myDocuments;
            }
        }

        public static string MyDocumentsSavesDirectory
        {
            get
            {
                var myDocuments = MyDocumentsDirectory + "\\settings\\";
                return myDocuments.Trim();
            }
        }

        public static string CEMMyDocumentsDirectory
        {
            get
            {
                var myDocuments = MyDocumentsDirectory + "\\CEM\\";
                Directory.CreateDirectory(myDocuments);
                return myDocuments.Trim();
            }
        }

        public static string CEMMyDocumentsDbSaveDirectory
        {
            get
            {
                var saveLocation = SaveFolder;
                var datalocation = CEMCore.CEMMyDocumentsDirectory + $"\\Data\\CEM\\DB\\{saveLocation}\\";
                Directory.CreateDirectory(datalocation);
                return datalocation.Trim();

            }
        }

        public static string CEMMyDocumentsDbSaveDirectory_RAW
        {
            get
            {
                var saveLocation = SaveFolderRaw;
                var datalocation = CEMCore.CEMMyDocumentsDirectory + $"\\Data\\CEM\\DB\\{saveLocation}\\";
                Directory.CreateDirectory(datalocation);
                return datalocation.Trim();

            }
        }

        /// <summary>
        /// Get the save folder before it is handled by the settings
        /// </summary>
        public static string SaveFolderRaw
        {
            get
            {
                string saveName = CEMCore.CEMCoreInstance != null && CEMCore.CEMCoreInstance.CoreHack != null ? CEMCore.CEMCoreInstance.CoreHack.GetSaveName() : null;
                var saveLocation = CEMCore.CEMCoreInstance != null && CEMCore.CEMCoreInstance.CoreHack != null && !string.IsNullOrEmpty(saveName)
                        ? saveName : "Unknown";

                saveLocation = saveLocation.Trim() + "\\";
                

                return saveLocation.Trim();
            }
        }

        public static string SaveFolder
        {
            get
            {
                var saveLocation = SaveFolderRaw;

                var settings = CEMCoreSettings.Load();
                if (!string.IsNullOrEmpty(settings.OtherSaveFolder))
                {
                    saveLocation = settings.OtherSaveFolder;
                }

                return saveLocation.Trim();
            }
        }

        public static string CEMInternalDataDirectory
        {
            get
            {
                var baseDir = GetApplicationDirectory();
                var dataFolder = baseDir + "\\CEM\\Data\\";
                if (!Directory.Exists(dataFolder))
                    Directory.CreateDirectory(dataFolder);

                return dataFolder.Trim();
            }
        }

        public static async Task<bool> InitialStartupOfCEM(ILogger logger = null, ICEMVisualizer visualizer = null, CancellationToken token = default(CancellationToken))
        {
            // Was cancellation already requested?
            if (token.IsCancellationRequested)
            {
                Debug.WriteLine($"Task InitialStartupOfCEM was cancelled before it got started.");
                token.ThrowIfCancellationRequested();
            }

            int Attempts = 0;
            // Wait for CM to start
            while (!CoreHack.IsInCM())
            {
                if (token.IsCancellationRequested)
                {
                    Debug.WriteLine($"Task InitialStartupOfCEM was cancelled.");
                    token.ThrowIfCancellationRequested();
                }

                await Task.Delay(1000);
                Debug.WriteLine("Waiting for CM to start up");
                if (Attempts > 120)
                    return false;

                Attempts++;

            }

            Debug.WriteLine("CM has started");
            await Task.Delay(1000);



            // ------------------------- //
            // Get the Career File saved //
            if (CEMCoreInstance == null)
            {
                CEMCoreInstance = new CEMCore(new CoreHack());
                if (logger == null) logger = new NullLogger();
                CEMCoreInstance.Logger = logger;
            }

            // ------------------------ //
            // Get the save name ready  //
            var saveName = CEMCore.CEMCoreInstance.CoreHack.GetSaveName();

           
            return true;

            // If needed copy sponsor options to the My Documents directory
            //if (File.Exists(CEMInternalDataDirectory + "BaseData\\Sponsors.zip"))
            //{
            //    var archive = ZipFile.Open(CEMInternalDataDirectory + "BaseData\\Sponsors.zip", ZipArchiveMode.Read);
            //    foreach (var ent in archive.Entries)
            //    {
            //        if (!File.Exists(Sponsor.CEMSponsorDirectory + ent.Name))
            //        {
            //            using (var f = ent.Open())
            //            {
            //                using (FileStream fileStream = new FileStream(Sponsor.CEMSponsorDirectory + ent.Name, FileMode.OpenOrCreate))
            //                {
            //                    //f.Position = 0;
            //                    f.CopyTo(fileStream);
            //                    f.Flush();
            //                    fileStream.Flush();
            //                }
            //            }
            //        }
            //    }
            //    ZipFile.ExtractToDirectory(CEMInternalDataDirectory + "BaseData\\Sponsors.zip"
            //        , Sponsor.CEMSponsorDirectory, true);
            //}

        }

        public static CEMCoreSettings CEMCoreSettings { get; set; }
        public static CEMCore CEMCoreInstance { get; set; }

        public CEMCore()
        {
            if(CEMCoreInstance != null)
            {
                throw new Exception("Cannot create more than one CEMCore instance!");
            }

            CareerDB1.Current = null;
            CareerDB2.Current = null;
            CareerDB1.FIFAUser = null;
            CareerDB1.UserTeam = null;

            DoSetup();

        }

        public void DoSetup()
        {
            // Initialize FIFA Leagues CSV to Enumerable
            //FIFALeague.GetFIFALeagues();

            // Setup Singleton
            if (CreateCopyOfSave(DateTime.Now))
            {
                if (SetupCareerFile(DateTime.Now))
                {

                    CEMCoreInstance = this;
                }
            }
        }

        public CEMCore(CoreHack coreHack)
        {
            if (CEMCoreInstance != null)
            {
                throw new Exception("Cannot create more than one CEMCore instance!");
            }
            CEMCoreInstance = this;
            this.CoreHack = coreHack;

            CareerDB1.Current = null;
            CareerDB2.Current = null;
            CareerDB1.FIFAUser = null;
            CareerDB1.UserTeam = null;

            DoSetup();
        }

        public CoreHack CoreHack { get; private set; }

        public CareerFile CareerFile = null;

        public static bool NeedsReloadOfSave { get; set; }
        public event EventGameSaveChangedHandler GameSaveChanged;
        public event EventHandler ThresholdReached;

        internal void GameDateChanged(DateTime oldDate, DateTime newDate)
        {
            var dayOfWeek = newDate.DayOfWeek;
            if (CreateCopyOfSave(newDate))
            {
                if (SetupCareerFile(newDate))
                {
                    CEMCoreSettings = CEMCoreSettings.Load();
                }
            }

        }

        public string CurrentSaveFileName;

        public static bool SetupCareerFileComplete = false;

        public static CareerFile SetupCareerFile(string inCareerFilePath)
        {
            //using (var fs = new FileStream(inCareerFilePath, FileMode.Open)) 
            //{
            //    using (var xmlFS = new FileStream(CEMInternalDataDirectory + "fifa_ng_db-meta.XML", FileMode.Open)) 
            //    { 
                    var CareerFile = new CareerFile(inCareerFilePath, CEMInternalDataDirectory + "fifa_ng_db-meta.XML");

                // Setup Internal Career Entities
                CareerDB1.Current = new CareerDB1();
                CareerDB1.Current.ParentDataSet = CareerFile.Databases[0].ConvertToDataSet();
                CareerDB2.Current = new CareerDB2();
                CareerDB2.Current.ParentDataSet = CareerFile.Databases[1].ConvertToDataSet();
                    //});
                    //CareerFileThread.Start();
                    //CareerFileThread.Join();

                    if (CareerFile.Databases.Length > 0)
                    {


                        if (CareerDB1.FIFAUser == null)
                        {
                            Stopwatch swTeams = new Stopwatch();
                            swTeams.Start();

                            var usersDt = CareerFile.Databases[0].Table[3].ConvertToDataTable();
                            // Read User. Set ClubTeamId to 1 less. Don't know why I have to do this!
                            CareerDB1.FIFAUser = CreateItemFromRow<FIFAUsers>(usersDt.Rows[0]);

                            swTeams.Stop();
                            Debug.WriteLine("User took: " + swTeams.Elapsed + " to build");
                            Trace.WriteLine("User took: " + swTeams.Elapsed + " to build");
                        }

                        if (CareerDB2.Current != null && (CareerDB2.Current.teams == null || CareerDB1.UserTeam == null))
                        {
                            Stopwatch swTeams = new Stopwatch();
                            swTeams.Start();

                            var dbTeams = CareerFile.Databases[1].GetTable("teams");
                            CareerDB2.Current.teams = dbTeams.ConvertToDataTable();
                            var firstteam = CareerDB2.Current.teams.Rows[0];
                            var teams = (from myRow in CareerDB2.Current.teams.AsEnumerable()
                                         where myRow.Field<int>("teamid") == CareerDB1.FIFAUser.clubteamid
                                         select myRow);
                            var team = teams.FirstOrDefault();
                            if (team != null)
                                CareerDB1.UserTeam = CreateItemFromRow<FIFATeam>(team);

                            swTeams.Stop();
                            Debug.WriteLine("Teams took: " + swTeams.Elapsed + " to build");
                        }

                        if (CareerDB2.Current.players == null)
                        {

                            Stopwatch swTeams = new Stopwatch();
                            swTeams.Start();

                            CareerDB2.Current.players = CareerFile.Databases[1].GetTable("players").ConvertToDataTable().AsEnumerable();
                            CareerDB2.Current.teamplayerlinks = CareerFile.Databases[1].GetTable("teamplayerlinks").ConvertToDataTable().AsEnumerable();
                            CareerDB2.Current.editedplayernames = CareerFile.Databases[1].GetTable("editedplayernames").ConvertToDataTable().AsEnumerable();
                            CareerDB2.Current.leagues = CareerFile.Databases[1].GetTable("leagues").ConvertToDataTable().AsEnumerable();
                            CareerDB2.Current.leagueteamlinks = CareerFile.Databases[1].GetTable("leagueteamlinks").ConvertToDataTable().AsEnumerable();
                            CareerDB2.Current.teamsEnumerable = CareerFile.Databases[1].GetTable("teams").ConvertToDataTable().AsEnumerable();


                            swTeams.Stop();
                            Debug.WriteLine("Team Player Links took: " + swTeams.Elapsed + " to build");
                        }

                        if (CareerDB2.Current.leagueteamlinks == null)
                        {
                            CareerDB2.Current.leagueteamlinks = CareerFile.Databases[1].GetTable("leagueteamlinks").ConvertToDataTable().AsEnumerable();
                        }

                    }
                    return CareerFile;

            //    }
            //}
            //return null;
        }

        public bool SetupCareerFile(DateTime currentGameDate)
        {

            SetupCareerFileComplete = false;

            //var CareerFileThread = new Thread(() =>
            //{

            if (CareerFile == null || (CareerFile != null && currentGameDate.DayOfWeek == DayOfWeek.Monday))
            {
                SetupCareerFile(GetMyDocsSaveFileName());
            }

            SetupCareerFileComplete = true;
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

            var baseDir = GetApplicationDirectory();
            var dataFolder = baseDir + "\\CEM\\Data\\";
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
            var baseDir = GetApplicationDirectory();
            var dataFolder = baseDir + "\\CEM\\Data\\";
            var xmlMetaData = dataFolder + "fifa_ng_db-meta.XML";

            // Loading from a file, you can also load from a stream
            var xml = XDocument.Load(xmlMetaData);

            var table = from c in xml.Root.Descendants("table")
                        where (string)c.Attribute("shortname") == shortName
                        select (string)c.Attribute("name");


            return table.FirstOrDefault();
        }

        public string GetMyDocsSaveFileName()
        {
            var myDocuments = GetFIFAMyDocumentsDirectory() + "\\settings\\";
            var saveFileLocation = string.Empty;
            foreach (var f in Directory.GetFiles(myDocuments))
            {
                using (var nr = new NativeReader(new FileStream(f, FileMode.Open)))
                {
                    nr.Position = 18;
                    var saveName = nr.ReadNullTerminatedString();
                    if (saveName == CoreHack.GetSaveName())
                    {
                        saveFileLocation = f;
                        break;
                    }


                }
            }
            return saveFileLocation;

        }

        private bool CreateCopyOfSave(DateTime gameDate)
        {
            var internalSaveFileLocation = CEMInternalDataDirectory + CoreHack.GetSaveName();
            if (!File.Exists(internalSaveFileLocation) || gameDate.DayOfWeek == DayOfWeek.Monday)
            {
                // backup save file
                var myDocuments = GetFIFAMyDocumentsDirectory() + "\\settings\\";

                var saveFileLocation = GetMyDocsSaveFileName();

#pragma warning disable CS0162 // Unreachable code detected
                for (int iAttempt = 0; iAttempt < 5; iAttempt++)
#pragma warning restore CS0162 // Unreachable code detected
                {
                    try
                    {
                        File.Copy(saveFileLocation, internalSaveFileLocation, true);
                        Debug.WriteLine("Created copy of " + CoreHack.GetSaveName());
                        //Trace.WriteLine("Created copy of " + CoreHack.SaveFileName);
                        return true;
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(e.ToString());
                        return false;
                    }
                }
                return false;
            }

            return true;
        }

        ~CEMCore() => Dispose(false);
        bool _disposed = false;

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            // Managed Resources
            if (disposing)
            {
                CoreHack = null;
            }
        }
    }

}
