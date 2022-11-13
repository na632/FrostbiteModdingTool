using CareerExpansionMod.CEM;
using CareerExpansionMod.CEM.FIFA;
using FifaLibrary;
using FMT.FileTools;
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

namespace FMT.FifaLibrary.CEM
{
    public class CEMCore2 : IDisposable
    {
        private string Game { get; set; }

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
                case "FIFA22":
                    directory = Microsoft.VisualBasic.FileIO.SpecialDirectories.MyDocuments + "\\FIFA 22\\settings\\";
                    break;
                case "FIFA23":
                    directory = Microsoft.VisualBasic.FileIO.SpecialDirectories.MyDocuments + "\\FIFA 23\\settings\\";
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



        /// <summary>
        /// Starts up a singular instance of CEMCore
        /// </summary>
        /// <param name="inGame"></param>
        /// <param name="monitorSaveDirectoryAndInitial">Whether to monitor the save directory and run initial setup</param>
        /// <exception cref="Exception"></exception>
        public CEMCore2(string inGame, bool monitorSaveDirectoryAndInitial = true)
        {
            if (CEMCoreInstance != null)
                throw new Exception("Cannot create another instance of CEMCore");

            CEMCoreInstance = this;
            Game = inGame;
            if (monitorSaveDirectoryAndInitial)
            {
                MonitorSaveDirectory();
                InitialSetup();
            }
        }

        public CEMCore2(string inGame
            , Stream careerStream
            , Stream DBMetaStream)
        {
            if (CEMCoreInstance != null)
                throw new Exception("Cannot create another instance of CEMCore");

            if (careerStream == null)
                throw new ArgumentNullException("Career Stream must be provided!");

            if (DBMetaStream == null)
                throw new ArgumentNullException("DB Meta Stream must be provided!");

            CEMCoreInstance = this;
            Game = inGame;

            DbStream = new MemoryStream();
            careerStream.CopyTo(DbStream);
            careerStream.Position = 0;
            DBMetaStream.Position = 0;
            XmlDbMetaStream = DBMetaStream;
            CurrentCareerFile = SetupCareerFile(careerStream, string.Empty);
        }


        public static async Task<CareerFile> SetupCareerFileAsync(string inCareerFilePath)
        {
            return await Task.Run(() => { return SetupCareerFile(inCareerFilePath); });

        }

        static FileStream fsDB;
        static Stream DbStream;
        static Stream XmlDbMetaStream;

        public static CareerFile SetupCareerFile(Stream stream, string filePathOrName)
        {
            
            if (CurrentCareerFile == null)
                CurrentCareerFile = new CareerFile(stream, XmlDbMetaStream, filePathOrName);

            var CareerFile = CurrentCareerFile;

            // Setup Internal Career Entities
            CareerDB1.Current = new CareerDB1();
            CareerDB1.Current.ParentDataSet = CareerFile.Databases[0].ConvertToDataSet();
            CareerDB2.Current = new CareerDB2();
            CareerDB2.Current.ParentDataSet = CareerFile.Databases[1].ConvertToDataSet();

            if (CareerFile.Databases.Length > 0)
            {
                var usersDt = CareerFile.Databases[0].Table[3].ConvertToDataTable();
                // Read User. Set ClubTeamId to 1 less. Don't know why I have to do this!
                CareerDB1.FIFAUser = CreateItemFromRow<FIFAUsers>(usersDt.Rows[0]);

                var dbTeams = CareerFile.Databases[1].GetTable("teams");
                CareerDB2.Current.teams = dbTeams.ConvertToDataTable();
                // TODO: FIXME!
                //var allTeams = CareerDB2.Current.teams;
                //var teams = allTeams.Select("teamid" (from myRow in allTeams.AsEnumerable()
                //                where myRow["teamid"].ToString() == CareerDB1.FIFAUser.clubteamid.ToString()
                //                select myRow);
                //var team = teams.FirstOrDefault();
                //if (team != null)
                //{
                //    CareerDB1.UserTeam = CreateItemFromRow<FIFATeam>(team);
                //    FIFATeam.ClearCache();
                //}

                CareerDB2.Current.players = CareerFile.Databases[1].GetTable("players").ConvertToDataTable().AsEnumerable();
                CareerDB2.Current.teamplayerlinks = CareerFile.Databases[1].GetTable("teamplayerlinks").ConvertToDataTable().AsEnumerable();
                CareerDB2.Current.editedplayernames = CareerFile.Databases[1].GetTable("editedplayernames").ConvertToDataTable().AsEnumerable();
                CareerDB2.Current.leagues = CareerFile.Databases[1].GetTable("leagues").ConvertToDataTable().AsEnumerable();
                CareerDB2.Current.leagueteamlinks = CareerFile.Databases[1].GetTable("leagueteamlinks").ConvertToDataTable().AsEnumerable();
                CareerDB2.Current.teamsEnumerable = CareerFile.Databases[1].GetTable("teams").ConvertToDataTable().AsEnumerable();

                CareerDB2.Current.leagueteamlinks = CareerFile.Databases[1].GetTable("leagueteamlinks").ConvertToDataTable().AsEnumerable();
            }
            return CareerFile;
        }
        public static CareerFile SetupCareerFile(string inCareerFilePath)
        {
            if(fsDB != null)
            {
                fsDB.Close();
                fsDB.Dispose(); 
            }

            if (XmlDbMetaStream != null)
                XmlDbMetaStream.Position = 0;
            else
            {
                XmlDbMetaStream = new FileStream(CEMInternalDataDirectory + "fifa_ng_db-meta.XML", FileMode.Open);
                Debug.WriteLine("CEM CORE: Using fallback db meta xml");
            }

            fsDB = new FileStream(inCareerFilePath, FileMode.Open);
            return SetupCareerFile(fsDB, inCareerFilePath);
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


        public MemoryStream GetCopyOfCareerFile() 
        {
            MemoryStream msCurrentfile = new MemoryStream();
            using (var fsCurrentFile = new FileStream(CareerFile.Current.OriginalFileName, FileMode.Open))
            {
                fsCurrentFile.CopyTo(msCurrentfile);
                msCurrentfile.Position = 0;
            }
            return msCurrentfile;
        }


    public async Task<List<FIFAPlayerStat>> GetPlayerStatsAsync(int? teamId = null)
        {
            return await Task.Run(() => { return GetPlayerStats(teamId); });
        }

        public List<FIFAPlayerStat> GetPlayerStats(int? teamId = null)
        {
            if (CurrentCareerFile == null)
                throw new Exception("");

            var fifaUser = CareerDB1.FIFAUser;
            var tid1 = fifaUser.clubteamid;

            List<FIFAPlayer> players = CareerDB1.UserPlayers;
            if (teamId.HasValue && teamId != fifaUser.clubteamid)
            {
                var teams = (from myRow in CareerDB2.Current.teams.AsEnumerable()
                             where myRow.Field<int>("teamid") == teamId.Value
                             select myRow);
                var team = teams.FirstOrDefault();
                if (team == null)
                {
                    throw new NullReferenceException($"Unable to find team for id:{teamId}");
                }

                var fifaTeam = CreateItemFromRow<FIFATeam>(team);
                players = fifaTeam.GetPlayers();
                tid1 = teamId.Value;
            }

            List<FIFAPlayerStat> stats = new List<FIFAPlayerStat>();
            var msCurrentfile = new MemoryStream();
            DbStream.Position = 0;
            DbStream.CopyTo(msCurrentfile);

            //using (var fsCurrentFile = new FileStream(CareerFile.Current.OriginalFileName, FileMode.Open))
            //{
            //    fsCurrentFile.CopyTo(msCurrentfile);
            //    msCurrentfile.Position = 0;
            //}
            using (NativeReader nr = new NativeReader(msCurrentfile))
            {
                foreach (var player in players)
                {
                    var searchByte = BitConverter.GetBytes(tid1).ToList();
                    var playerIdByte = BitConverter.GetBytes(player.playerid).ToArray();
                    searchByte.AddRange(playerIdByte);

                    var found2 = nr.ScanAOB2(searchByte.ToArray());
                    foreach (var pos in found2)
                    {
                        nr.Position = pos;
                        var playerStat = FIFAPlayerStat.ConvertBytesToStats(nr.ReadBytes(87));
                        if (playerStat != null && playerStat.PlayerId > 0 && playerStat.Apps > 0)
                        {
                            playerStat.OVR = player.overallrating;
                            playerStat.OVRGrowth = player.GetPlayerGrowth();
                            stats.Add(playerStat);
                        }
                    }
                }
            }

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


        public int? GetFinancesOffset(int? teamId = null)
        {
            if (teamId == null) {
                var fifaUser = CareerDB1.FIFAUser;
                teamId = fifaUser.clubteamid;
            }
            using (var nr = new NativeReader(GetCopyOfCareerFile()))
            {
                List<byte> searchByte = new List<byte>();
                searchByte.Add(0x00); // 00 at the start? 
                searchByte.AddRange(BitConverter.GetBytes(0)); // 00 00 00 00
                searchByte.AddRange(BitConverter.GetBytes(teamId.Value)); // team id converted to bytes
                searchByte.Add(0xFF); // FF at the end? 
                var position = nr.ScanAOB2(searchByte.ToArray());
                return position.FirstOrDefault();
            }
        }

        public async Task<Finances> GetFinances(int? teamId = null)
        {
            var userFinanceOffset = GetFinancesOffset(teamId);
            if (!userFinanceOffset.HasValue)
                return null;

            return await Task.Run(() => {
                userFinances = new Finances();
                using (NativeReader nr = new NativeReader(GetCopyOfCareerFile()))
                {
                    nr.Position = userFinanceOffset.Value;
                    var unk1 = nr.ReadByte(); // unk 0
                    var unk2 = nr.ReadInt(); // unk 0
                    var tid = nr.ReadInt(); // team id
                    var ff = nr.ReadByte(); // -1
                    nr.Position -= 1;
                    nr.Position -= 4;
                    nr.Position -= 32;
                    userFinances.TransferBudget = nr.ReadInt();
                    //var nameOfUserFinances = nr.ReadNullTerminatedString();
                    //userFinances.ClubWorth = nr.ReadLong();
                    //userFinances.StartingBudget = nr.ReadInt();
                    //nr.Position += 16;
                    //userFinances.TransferBudget = nr.ReadInt();
                }
                return userFinances;
            });
        }

        public void UpdateUserFinancesInFile()
        {
            //var userFinanceLocation = 0;
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

        /// <summary>
        /// Gathers the most recent Career File
        /// </summary>
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
            if(FileSystemWatcher != null) 
                FileSystemWatcher.Dispose();  

            if(DbStream != null)
                DbStream.Dispose();

            if(XmlDbMetaStream != null) 
                XmlDbMetaStream.Dispose();
        }
    }
}
