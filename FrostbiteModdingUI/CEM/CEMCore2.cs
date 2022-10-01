using CareerExpansionMod.CEM;
using CareerExpansionMod.CEM.FIFA;
using FifaLibrary;
using FrostbiteSdk;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using v2k4FIFAModding.Career.CME.FIFA;

namespace FrostbiteModdingUI.CEM
{
    public class CEMCore2 : IDisposable
    {
        private string Game;

        public string GetFIFAMyDocumentsSaveDirectory()
        {
            var directory = Microsoft.VisualBasic.FileIO.SpecialDirectories.MyDocuments + "\\FIFA 21\\settings\\";
            switch (Game)
            {
                case "FIFA20":
                    directory = Microsoft.VisualBasic.FileIO.SpecialDirectories.MyDocuments + "\\FIFA 20\\settings\\";
                    break;
                case "FIFA21":
                    directory = Microsoft.VisualBasic.FileIO.SpecialDirectories.MyDocuments + "\\FIFA 21\\settings\\";
                    break;
            }
            return directory;
        }

        public static string GetApplicationDirectory()
        {
            var runningLocation = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            return runningLocation;
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

        public FileSystemWatcher FileSystemWatcher;

        public static CEMCore2 CEMCoreInstance = null;

        public class Finances
        {
            public long ClubWorth { get; set; }
            private int startingBudget;

            public int StartingBudget
            {
                get { return startingBudget; }
                set { startingBudget = value; }
            }


            private int transferBudget;

            public int TransferBudget
            {
                get { return transferBudget; }
                set { transferBudget = value;  }
            }

        }

        private Finances userFinances;

        public Finances UserFinances
        {
            get { return userFinances; }
            set { userFinances = value; }
        }



        public CEMCore2(string inGame)
        {
            if (CEMCoreInstance != null)
                throw new Exception("Cannot create another instance of CEMCore");

            CEMCoreInstance = this;
            Game = inGame;
            MonitorSaveDirectory();
            InitialSetup();
        }

        public static async Task<CareerFile> SetupCareerFileAsync(string inCareerFilePath)
        {
            return await Task.Run(() => { return SetupCareerFile(inCareerFilePath); });

        }

        static FileStream fsDB;
        static FileStream fsXml;

        public static CareerFile SetupCareerFile(string inCareerFilePath)
        {
            if(fsDB != null)
            {
                fsDB.Close();
                fsDB.Dispose(); 
            }

            if (fsXml != null)
                fsXml.Position = 0;
            else
                fsXml = new FileStream(CEMInternalDataDirectory + "fifa_ng_db-meta.XML", FileMode.Open);

            fsDB = new FileStream(inCareerFilePath, FileMode.Open);
            {
                {
                    //CurrentCareerFile = new CareerFile(fsDB, fsXml);
                    var CareerFile = CurrentCareerFile;

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


                        //if (CareerDB1.FIFAUser == null)
                        //{
                        //Stopwatch swTeams = new Stopwatch();
                        //swTeams.Start();

                        var usersDt = CareerFile.Databases[0].Table[3].ConvertToDataTable();
                        // Read User. Set ClubTeamId to 1 less. Don't know why I have to do this!
                        CareerDB1.FIFAUser = CreateItemFromRow<FIFAUsers>(usersDt.Rows[0]);

                        //swTeams.Stop();
                        //Debug.WriteLine("User took: " + swTeams.Elapsed + " to build");
                        //Trace.WriteLine("User took: " + swTeams.Elapsed + " to build");
                        //}

                        //if (CareerDB2.Current != null && (CareerDB2.Current.teams == null || CareerDB1.UserTeam == null))
                        //{
                        //Stopwatch swTeams = new Stopwatch();
                        //swTeams.Start();

                        var dbTeams = CareerFile.Databases[1].GetTable("teams");
                        CareerDB2.Current.teams = dbTeams.ConvertToDataTable();
                        var firstteam = CareerDB2.Current.teams.Rows[0];
                        var teams = (from myRow in CareerDB2.Current.teams.AsEnumerable()
                                     where myRow.Field<int>("teamid") == CareerDB1.FIFAUser.clubteamid
                                     select myRow);
                        var team = teams.FirstOrDefault();
                        if (team != null)
                        {
                            CareerDB1.UserTeam = CreateItemFromRow<FIFATeam>(team);
                            FIFATeam.ClearCache();
                        }

                        //swTeams.Stop();
                        //Debug.WriteLine("Teams took: " + swTeams.Elapsed + " to build");
                        //}

                        //if (CareerDB2.Current.players == null)
                        //{

                        //Stopwatch swTeams = new Stopwatch();
                        //swTeams.Start();

                        CareerDB2.Current.players = CareerFile.Databases[1].GetTable("players").ConvertToDataTable().AsEnumerable();
                        CareerDB2.Current.teamplayerlinks = CareerFile.Databases[1].GetTable("teamplayerlinks").ConvertToDataTable().AsEnumerable();
                        CareerDB2.Current.editedplayernames = CareerFile.Databases[1].GetTable("editedplayernames").ConvertToDataTable().AsEnumerable();
                        CareerDB2.Current.leagues = CareerFile.Databases[1].GetTable("leagues").ConvertToDataTable().AsEnumerable();
                        CareerDB2.Current.leagueteamlinks = CareerFile.Databases[1].GetTable("leagueteamlinks").ConvertToDataTable().AsEnumerable();
                        CareerDB2.Current.teamsEnumerable = CareerFile.Databases[1].GetTable("teams").ConvertToDataTable().AsEnumerable();


                        //swTeams.Stop();
                        //Debug.WriteLine("Team Player Links took: " + swTeams.Elapsed + " to build");
                        //}

                        //if (CareerDB2.Current.leagueteamlinks == null)
                        //{
                        CareerDB2.Current.leagueteamlinks = CareerFile.Databases[1].GetTable("leagueteamlinks").ConvertToDataTable().AsEnumerable();
                        //}

                    }
                    return CareerFile;
                }
            }
            return null;
        }

        private static CareerFile careerFile;

        public static CareerFile CurrentCareerFile
        {
            get { return careerFile; }
            set 
            { 
                if(careerFile != null)
                {
                    careerFile = null;
                }

                careerFile = value; 
            
            }
        }


        public async Task<List<FIFAPlayerStat>> GetPlayerStatsAsync()
        {
            return await Task.Run(() => { return GetPlayerStats(); });
        }

        public List<FIFAPlayerStat> GetPlayerStats()
        {
            if (CurrentCareerFile == null)
                throw new Exception("");


            var fifaUser = CareerDB1.FIFAUser;
            var userTeam = CareerDB1.UserTeam;
            var userTeamPlayers = CareerDB1.UserPlayers;

            var tid1 = fifaUser.clubteamid;


            List<FIFAPlayerStat> stats = new List<FIFAPlayerStat>();

            //using (NativeReader nr = new NativeReader(CurrentCareerFile.DbStream))
            //{
            //    CurrentCareerFile.DbStream.Position = 0;
            //    //nr.Position = 6000000;
            //    var rBytes = nr.ReadToEnd();
            //    foreach (var player in userTeamPlayers)
            //    {
            //        var searchByte = BitConverter.GetBytes(tid1).ToList();
            //        var playerIdByte = BitConverter.GetBytes(player.playerid).ToArray();
            //        searchByte.AddRange(playerIdByte);

            //        BoyerMoore boyerMoore2 = new BoyerMoore(searchByte.ToArray());
            //        var found2 = boyerMoore2.SearchAll(rBytes);
            //        //var found2 = ByteSearchList(rBytes, searchByte.ToArray());
            //        foreach (var pos in found2)
            //        {
            //            nr.Position = pos;
            //            var playerStat = FIFAPlayerStat.ConvertBytesToStats(nr.ReadBytes(87));
            //            if (playerStat != null && playerStat.PlayerId > 0 && playerStat.Apps > 0)
            //            {
            //                playerStat.OVR = player.overallrating;
            //                playerStat.OVRGrowth = player.GetPlayerGrowth();
            //                stats.Add(playerStat);
            //            }
            //        }
            //    }
            //}

            return stats;
        }

        int ByteSearch(byte[] src, byte[] pattern)
        {
            int maxFirstCharSlot = src.Length - pattern.Length + 1;
            int j;
            for (int i = 0; i < maxFirstCharSlot; i++)
            {
                if (src[i] != pattern[0]) continue;//comp only first byte

                // found a match on first byte, it tries to match rest of the pattern
                for (j = pattern.Length - 1; j >= 1 && src[i + j] == pattern[j]; j--) ;
                if (j == 0) return i;
            }
            return -1;
        }

        IEnumerable<int?> ByteSearchList(byte[] src, byte[] pattern)
        {
            int maxFirstCharSlot = src.Length - pattern.Length + 1;
            int j;
            for (int i = 0; i < maxFirstCharSlot; i++)
            {
                if (src[i] != pattern[0]) continue;//comp only first byte

                // found a match on first byte, it tries to match rest of the pattern
                for (j = pattern.Length - 1; j >= 1 && src[i + j] == pattern[j]; j--) ;
                if (j == 0)  yield return i;
            }
            yield return null;
        }

        public List<FIFAPlayerStat> UserTeamPlayerStats = new List<FIFAPlayerStat>(30000);


        public async Task<Finances> GetUserFinances()
        {
            userFinances = new Finances();
            //using (NativeReader nr = new NativeReader(CurrentCareerFile.DbStream))
            //{
            //    var rBytes = nr.ReadToEnd();
            //    var searchByte = ASCIIEncoding.ASCII.GetBytes("ubp01");
            //    BoyerMoore boyerMoore2 = new BoyerMoore(searchByte.ToArray());
            //    var found2 = boyerMoore2.Search(rBytes);
            //    nr.Position = found2;
            //    var nameOfUserFinances = nr.ReadNullTerminatedString();
            //    userFinances.ClubWorth = nr.ReadLong();
            //    userFinances.StartingBudget = nr.ReadInt();
            //    nr.Position += 16;
            //    userFinances.TransferBudget = nr.ReadInt();
            //}
            return userFinances;
        }

        public void UpdateUserFinancesInFile()
        {
            var userFinanceLocation = 0;
            //using (NativeReader nr = new NativeReader(CurrentCareerFile.DbStream))
            //{
            //    var rBytes = nr.ReadToEnd();
            //    var searchByte = ASCIIEncoding.ASCII.GetBytes("ubp01");
            //    BoyerMoore boyerMoore2 = new BoyerMoore(searchByte.ToArray());
            //    userFinanceLocation = boyerMoore2.Search(rBytes);
                
            //}

            //using (NativeWriter nw = new NativeWriter(CurrentCareerFile.DbStream))
            //{
            //    nw.Position = userFinanceLocation;
            //    nw.Position += 6;
            //    nw.Position += 8;
            //    nw.Write(userFinances.StartingBudget);
            //    nw.Position += 16;
            //    nw.Write(userFinances.TransferBudget);
            //}
        }

        public IEnumerable<FileInfo> CareerFileInfos
        {
            get
            {
                return new DirectoryInfo(GetFIFAMyDocumentsSaveDirectory())
                    .GetFiles()
                .Where(x => x.Name.StartsWith("Career"))
                .OrderByDescending(x => x.LastAccessTime);
            }
        }

        public IDictionary<string, string> CareerFileNames
        {
            get
            {
                return new DirectoryInfo(GetFIFAMyDocumentsSaveDirectory())
                    .GetFiles()
                .Where(x => x.Name.StartsWith("Career"))
                .OrderByDescending(x => x.LastAccessTime).ToDictionary(x => x.FullName, x => new NativeReader(File.ReadAllBytes(x.FullName)).Skip(18).ReadNullTerminatedString());
            }
        }

        public IEnumerable<CareerFile> CareerFiles 
        { 
            get
            {
                //return CareerFileInfos.Select(x => new CareerFile(x.FullName, CEMInternalDataDirectory + "fifa_ng_db-meta.XML"));
                return null;
            } 
        }

        private void InitialSetup()
        {
            var fi = CareerFileInfos.FirstOrDefault();

            CurrentCareerFile = SetupCareerFile(fi.FullName);

        }

        private void UpdateSetup(string fullPath)
        {
            CurrentCareerFile = SetupCareerFile(fullPath);
        }

        public void MonitorSaveDirectory()
        {
            FileSystemWatcher = new FileSystemWatcher(GetFIFAMyDocumentsSaveDirectory());
            FileSystemWatcher.Created += FileSystemWatcher_Created;
            FileSystemWatcher.Changed += FileSystemWatcher_Changed;
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            InitialSetup();
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            InitialSetup();
        }

        public byte[] HexStringToByte(string param1, string param2)
        {
            return new byte[] {
                Convert.ToByte("0x" + param1.Substring(6, 2))
                , Convert.ToByte("0x" + param1.Substring(4, 2))
                , Convert.ToByte("0x" + param1.Substring(2, 2))
                , Convert.ToByte("0x" + param1.Substring(0, 2))
                , Convert.ToByte("0x" + param2.Substring(6, 2))
                , Convert.ToByte("0x" + param2.Substring(4, 2))
                , Convert.ToByte("0x" + param2.Substring(2, 2))
                , Convert.ToByte("0x" + param2.Substring(0, 2))
            };
        }

        public string FlipHexString(string innerHex)
        {
            return innerHex.Substring(6, 2) + innerHex.Substring(4, 2) + innerHex.Substring(2, 2) + innerHex.Substring(0, 2);
        }

        public string HexStringLittleEndian(int number)
        {
            return FlipHexString(number.ToString("X8"));
        }

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

        public void Dispose()
        {
            FileSystemWatcher.Dispose();
        }
    }
}
