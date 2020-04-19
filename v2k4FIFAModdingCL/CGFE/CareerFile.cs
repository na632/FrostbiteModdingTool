// Decompiled with JetBrains decompiler
// Type: FifaLibrary.CareerFile
// Assembly: FifaLibrary19, Version=14.3.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E4EB9DC-F35A-4480-B02D-6E230B595138
// Assembly location: C:\Program Files (x86)\RDBM 20\RDBM20\FifaLibrary19.dll

using System.Data;
using System.IO;

namespace v2k4FIFAModdingCL.CGFE
{
    public class CareerFile
    {
        private static uint[] s_CrcTable = new uint[256]
        {
      0U,
      79764919U,
      159529838U,
      222504665U,
      319059676U,
      398814059U,
      445009330U,
      507990021U,
      638119352U,
      583659535U,
      797628118U,
      726387553U,
      890018660U,
      835552979U,
      1015980042U,
      944750013U,
      1276238704U,
      1221641927U,
      1167319070U,
      1095957929U,
      1595256236U,
      1540665371U,
      1452775106U,
      1381403509U,
      1780037320U,
      1859660671U,
      1671105958U,
      1733955601U,
      2031960084U,
      2111593891U,
      1889500026U,
      1952343757U,
      2552477408U,
      2632100695U,
      2443283854U,
      2506133561U,
      2334638140U,
      2414271883U,
      2191915858U,
      2254759653U,
      3190512472U,
      3135915759U,
      3081330742U,
      3009969537U,
      2905550212U,
      2850959411U,
      2762807018U,
      2691435357U,
      3560074640U,
      3505614887U,
      3719321342U,
      3648080713U,
      3342211916U,
      3287746299U,
      3467911202U,
      3396681109U,
      4063920168U,
      4143685023U,
      4223187782U,
      4286162673U,
      3779000052U,
      3858754371U,
      3904687514U,
      3967668269U,
      881225847U,
      809987520U,
      1023691545U,
      969234094U,
      662832811U,
      591600412U,
      771767749U,
      717299826U,
      311336399U,
      374308984U,
      453813921U,
      533576470U,
      25881363U,
      88864420U,
      134795389U,
      214552010U,
      2023205639U,
      2086057648U,
      1897238633U,
      1976864222U,
      1804852699U,
      1867694188U,
      1645340341U,
      1724971778U,
      1587496639U,
      1516133128U,
      1461550545U,
      1406951526U,
      1302016099U,
      1230646740U,
      1142491917U,
      1087903418U,
      2896545431U,
      2825181984U,
      2770861561U,
      2716262478U,
      3215044683U,
      3143675388U,
      3055782693U,
      3001194130U,
      2326604591U,
      2389456536U,
      2200899649U,
      2280525302U,
      2578013683U,
      2640855108U,
      2418763421U,
      2498394922U,
      3769900519U,
      3832873040U,
      3912640137U,
      3992402750U,
      4088425275U,
      4151408268U,
      4197601365U,
      4277358050U,
      3334271071U,
      3263032808U,
      3476998961U,
      3422541446U,
      3585640067U,
      3514407732U,
      3694837229U,
      3640369242U,
      1762451694U,
      1842216281U,
      1619975040U,
      1682949687U,
      2047383090U,
      2127137669U,
      1938468188U,
      2001449195U,
      1325665622U,
      1271206113U,
      1183200824U,
      1111960463U,
      1543535498U,
      1489069629U,
      1434599652U,
      1363369299U,
      622672798U,
      568075817U,
      748617968U,
      677256519U,
      907627842U,
      853037301U,
      1067152940U,
      995781531U,
      51762726U,
      131386257U,
      177728840U,
      240578815U,
      269590778U,
      349224269U,
      429104020U,
      491947555U,
      4046411278U,
      4126034873U,
      4172115296U,
      4234965207U,
      3794477266U,
      3874110821U,
      3953728444U,
      4016571915U,
      3609705398U,
      3555108353U,
      3735388376U,
      3664026991U,
      3290680682U,
      3236090077U,
      3449943556U,
      3378572211U,
      3174993278U,
      3120533705U,
      3032266256U,
      2961025959U,
      2923101090U,
      2868635157U,
      2813903052U,
      2742672763U,
      2604032198U,
      2683796849U,
      2461293480U,
      2524268063U,
      2284983834U,
      2364738477U,
      2175806836U,
      2238787779U,
      1569362073U,
      1498123566U,
      1409854455U,
      1355396672U,
      1317987909U,
      1246755826U,
      1192025387U,
      1137557660U,
      2072149281U,
      2135122070U,
      1912620623U,
      1992383480U,
      1753615357U,
      1816598090U,
      1627664531U,
      1707420964U,
      295390185U,
      358241886U,
      404320391U,
      483945776U,
      43990325U,
      106832002U,
      186451547U,
      266083308U,
      932423249U,
      861060070U,
      1041341759U,
      986742920U,
      613929101U,
      542559546U,
      756411363U,
      701822548U,
      3316196985U,
      3244833742U,
      3425377559U,
      3370778784U,
      3601682597U,
      3530312978U,
      3744426955U,
      3689838204U,
      3819031489U,
      3881883254U,
      3928223919U,
      4007849240U,
      4037393693U,
      4100235434U,
      4180117107U,
      4259748804U,
      2310601993U,
      2373574846U,
      2151335527U,
      2231098320U,
      2596047829U,
      2659030626U,
      2470359227U,
      2550115596U,
      2947551409U,
      2876312838U,
      2788305887U,
      2733848168U,
      3165939309U,
      3094707162U,
      3040238851U,
      2985771188U
        };
        private DbFile[] m_Database = new DbFile[4];
        private string m_InGameName;
        protected string m_FileName;
        protected string m_XmlFileName;
        protected DataSet m_DescriptorDataSet;
        protected FifaPlatform m_Platform;
        private char[] m_HeaderSignature;
        private byte[] m_CryptArea;
        private uint m_CrcEaHeader;
        private long m_CrcEaHeaderPosition;
        private int m_NDatabases;

        public string InGameName
        {
            get
            {
                return this.m_InGameName;
            }
            set
            {
                this.m_InGameName = value;
            }
        }

        public string FileName
        {
            get
            {
                return this.m_FileName;
            }
        }

        public string XmlFileName
        {
            get
            {
                return this.m_XmlFileName;
            }
        }

        public DataSet DescriptorDataSet
        {
            get
            {
                return this.m_DescriptorDataSet;
            }
            set
            {
                this.m_DescriptorDataSet = value;
            }
        }

        public FifaPlatform Platform
        {
            get
            {
                return this.m_Platform;
            }
            set
            {
                this.m_Platform = value;
            }
        }

        public DbFile[] Databases
        {
            get
            {
                return this.m_Database;
            }
        }

        public int NDatabases
        {
            get
            {
                return this.m_NDatabases;
            }
        }

        public CareerFile(string careerFileName, string xmlFileName)
        {
            this.m_FileName = careerFileName;
            this.m_XmlFileName = xmlFileName;
            this.m_DescriptorDataSet = (DataSet)null;
            this.Load();
        }

        public bool Load()
        {
            return this.LoadXml() && this.LoadEA(this.m_FileName);
        }

        public bool LoadXml(string xmlFileName)
        {
            if (!File.Exists(xmlFileName))
                return false;
            this.m_XmlFileName = xmlFileName;
            this.m_DescriptorDataSet = new DataSet("XML_Descriptor");
            int num = (int)this.m_DescriptorDataSet.ReadXml(this.m_XmlFileName);
            return true;
        }

        public bool LoadXml()
        {
            return this.m_XmlFileName != null && this.LoadXml(this.m_XmlFileName);
        }

        public bool LoadEA(string fileName)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            DbReader r = new DbReader((Stream)fileStream, FifaPlatform.PC);
            r.BaseStream.Position = 18L;
            this.m_InGameName = FifaUtil.ReadNullTerminatedString((BinaryReader)r);
            while (r.BaseStream.Position < r.BaseStream.Length)
            {
                this.m_Database[this.m_NDatabases] = new DbFile();
                this.m_Database[this.m_NDatabases].DescriptorDataSet = this.m_DescriptorDataSet;
                if (this.m_Database[this.m_NDatabases].LoadDb(r, true))
                {
                    ++this.m_NDatabases;
                    if (this.m_NDatabases == 3)
                        break;
                }
                else
                    break;
            }
            r.Close();
            fileStream.Close();
            return true;
        }

        public bool SaveEa(string fileName, string templateName)
        {
            string fileName1 = Path.GetFileName(fileName);
            return (fileName1.StartsWith("Squad") || fileName1.StartsWith("Career")) && this.SaveEa(fileName);
        }

        public bool SaveEa(string fileName)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
            DbWriter w = new DbWriter((Stream)fileStream, this.m_Platform);
            DbReader r = new DbReader((Stream)fileStream, this.m_Platform);
            r.BaseStream.Position = 18L;
            int length = 0;
            while (r.ReadByte() != (byte)0)
                ++length;
            while (r.ReadByte() == (byte)0)
                ++length;
            w.BaseStream.Position = 18L;
            if (this.m_InGameName.Length > length)
                this.m_InGameName = this.m_InGameName.Substring(0, length);
            FifaUtil.WriteNullPaddedString((BinaryWriter)w, this.m_InGameName, length);
            w.BaseStream.Position = 118L;
            for (int index = 0; index < 8; ++index)
                w.Write((byte)0);
            for (int index = 0; index < this.m_NDatabases; ++index)
            {
                w.BaseStream.Position = this.m_Database[index].SignaturePosition;
                this.m_Database[index].SaveDb(w);
            }
            for (int index = 0; index < this.m_NDatabases; ++index)
                this.m_Database[index].ComputeAllCrc(r, w);
            r.Close();
            w.Close();
            fileStream.Close();
            return true;
        }

        public bool ExportDB(int dbIndex, string fileName)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
            DbWriter w = new DbWriter((Stream)fileStream, this.m_Platform);
            DbReader r = new DbReader((Stream)fileStream, this.m_Platform);
            if (dbIndex >= this.m_NDatabases)
                return false;
            long signaturePosition = this.m_Database[dbIndex].SignaturePosition;
            this.m_Database[dbIndex].SignaturePosition = 0L;
            this.m_Database[dbIndex].SaveDb(w);
            this.m_Database[dbIndex].ComputeAllCrc(r, w);
            this.m_Database[dbIndex].SignaturePosition = signaturePosition;
            r.Close();
            w.Close();
            fileStream.Close();
            return true;
        }

        private uint ComputeCrc(BinaryReader r, int offset, int size)
        {
            uint num1 = uint.MaxValue;
            r.BaseStream.Position = (long)offset;
            for (int index = 0; index < size; ++index)
            {
                byte num2 = r.ReadByte();
                num1 = CareerFile.s_CrcTable[((int)(num1 >> 24) ^ (int)num2) & (int)byte.MaxValue] ^ num1 << 8;
            }
            return num1 ^ uint.MaxValue;
        }

        private uint ComputeChecksum24(BinaryReader r)
        {
            r.BaseStream.Position = 0L;
            uint num1 = ((((uint)r.ReadByte() << 8 | (uint)r.ReadByte()) << 8 | (uint)r.ReadByte()) << 8 | (uint)r.ReadByte()) ^ uint.MaxValue;
            for (int index = 20; index > 0; --index)
            {
                byte num2 = r.ReadByte();
                uint num3 = CareerFile.s_CrcTable[(int)(num1 >> 24)];
                num1 = (num1 << 8 | (uint)num2) ^ num3;
            }
            return num1 ^ uint.MaxValue;
        }

        public void ConvertFromDataSet(DataSet[] dataSet)
        {
            if (dataSet.Length != this.m_NDatabases)
                return;
            for (int index = 0; index < this.m_NDatabases; ++index)
                this.m_Database[index].ConvertFromDataSet(dataSet[index]);
        }

        public DataSet[] ConvertToDataSet()
        {
            DataSet[] dataSetArray = new DataSet[this.m_NDatabases];
            for (int index = 0; index < this.m_NDatabases; ++index)
                dataSetArray[index] = this.m_Database[index].ConvertToDataSet();
            return dataSetArray;
        }
    }
}
