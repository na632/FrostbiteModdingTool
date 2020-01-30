using System.Drawing;
using System.IO;

namespace FifaLibrary
{
	public class Stadium : IdObject
	{
		public enum ETimeOfDay
		{
			Cloudy = 0,
			Sunny = 1,
			Night = 3,
			Sunset = 4
		}

		public enum EWeather
		{
			Dry = 0,
			CanRain = 1,
			CanSnow = 3
		}

		private string m_name;

		private int m_countrycode;

		private Country m_Country;

		private int m_policetypecode;

		private int m_sectionfacedbydefault;

		private int m_hometeamid;

		private Team m_HomeTeam;

		private int m_capacity;

		private int m_yearbuilt;

		private int m_seatcolor;

		private int m_stadiumtype;

		private int m_stadiumgoalnetstyle;

		private int m_stadiumgloalnetdepth;

		private int m_stadiumgoalnetlength;

		private int m_stadiumgoalnetwidth;

		private int m_stadiummowpattern_code;

		private int m_stadiumpitchlength;

		private int m_stadiumpitchwidth;

		private int m_adboardendlinedistance;

		private int m_adboardsidelinedistance;

		private int m_adboardtype;

		private int[] m_timeofday = new int[4];

		private int[] m_todweather = new int[4];

		private int m_dlc;

		private int m_stadiumgoalnettype;

		private int m_stadiumgoalnetpattern;

		private int m_stadiumgoalnettension;

		private int m_cameraheight;

		private int m_camerazoom;

		private string m_LocalName;

		private int m_managerhometopleftx;

		private int m_managerhometoplefty;

		private int m_managerhomebotrightx;

		private int m_managerhomebotrighty;

		private int m_managerawaytoplefty;

		private int m_managerawaytopleftx;

		private int m_managerawaybotrightx;

		private int m_managerawaybotrighty;

		private int m_subshometopleftx;

		private int m_subshometoplefty;

		private int m_subshomebotrightx;

		private int m_subshomebotrighty;

		private int m_subsawaytopleftx;

		private int m_subsawaytoplefty;

		private int m_subsawaybotrightx;

		private int m_subsawaybotrighty;

		private int m_upgradetier;

		private int m_upgradestyle;

		private int m_iseditable;

		private int m_isdynamic;

		private int m_genericrank;

		private int m_islicensed;

		private int m_stadhometechzoneminx;

		private int m_stadhometechzonemaxx;

		private int m_stadhometechzoneminz;

		private int m_stadhometechzonemaxz;

		private int m_stadawaytechzoneminx;

		private int m_stadawaytechzonemaxx;

		private int m_stadawaytechzoneminz;

		private int m_stadawaytechzonemaxz;

		public string name
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value;
			}
		}

		public int countrycode
		{
			get
			{
				return m_countrycode;
			}
			set
			{
				m_countrycode = value;
			}
		}

		public Country Country
		{
			get
			{
				return m_Country;
			}
			set
			{
				m_Country = value;
				if (m_Country != null)
				{
					m_countrycode = m_Country.Id;
				}
				else
				{
					m_countrycode = 0;
				}
			}
		}

		public int policetypecode
		{
			get
			{
				return m_policetypecode;
			}
			set
			{
				m_policetypecode = value;
			}
		}

		public int sectionfacedbydefault
		{
			get
			{
				return m_sectionfacedbydefault;
			}
			set
			{
				m_sectionfacedbydefault = value;
			}
		}

		public int hometeamid
		{
			get
			{
				return m_hometeamid;
			}
			set
			{
				m_hometeamid = value;
			}
		}

		public Team HomeTeam
		{
			get
			{
				return m_HomeTeam;
			}
			set
			{
				m_HomeTeam = value;
				if (m_HomeTeam != null)
				{
					m_hometeamid = m_HomeTeam.Id;
				}
				else
				{
					m_hometeamid = 0;
				}
			}
		}

		public int capacity
		{
			get
			{
				return m_capacity;
			}
			set
			{
				m_capacity = value;
			}
		}

		public int yearbuilt
		{
			get
			{
				return m_yearbuilt;
			}
			set
			{
				m_yearbuilt = value;
			}
		}

		public int seatcolor
		{
			get
			{
				return m_seatcolor;
			}
			set
			{
				m_seatcolor = value;
			}
		}

		public int stadiumtype
		{
			get
			{
				return m_stadiumtype;
			}
			set
			{
				m_stadiumtype = value;
			}
		}

		public int NetColor
		{
			get
			{
				return m_stadiumgoalnetstyle;
			}
			set
			{
				m_stadiumgoalnetstyle = value;
			}
		}

		public bool IsDeepNet
		{
			get
			{
				return m_stadiumgloalnetdepth == 320;
			}
			set
			{
				m_stadiumgloalnetdepth = (value ? 320 : 229);
			}
		}

		public int MowingPatternId
		{
			get
			{
				return m_stadiummowpattern_code;
			}
			set
			{
				m_stadiummowpattern_code = value;
			}
		}

		public int adboardendlinedistance
		{
			get
			{
				return m_adboardendlinedistance;
			}
			set
			{
				m_adboardendlinedistance = value;
			}
		}

		public int adboardsidelinedistance
		{
			get
			{
				return m_adboardsidelinedistance;
			}
			set
			{
				m_adboardsidelinedistance = value;
			}
		}

		public int adboardtype
		{
			get
			{
				return m_adboardtype;
			}
			set
			{
				m_adboardtype = value;
			}
		}

		public int stadiumgoalnettype
		{
			get
			{
				return m_stadiumgoalnettype;
			}
			set
			{
				m_stadiumgoalnettype = value;
			}
		}

		public int stadiumgoalnetpattern
		{
			get
			{
				return m_stadiumgoalnetpattern;
			}
			set
			{
				m_stadiumgoalnetpattern = value;
			}
		}

		public int stadiumgoalnettension
		{
			get
			{
				return m_stadiumgoalnettension;
			}
			set
			{
				m_stadiumgoalnettension = value;
			}
		}

		public int cameraheight
		{
			get
			{
				return m_cameraheight;
			}
			set
			{
				m_cameraheight = value;
			}
		}

		public int camerazoom
		{
			get
			{
				return m_camerazoom;
			}
			set
			{
				m_camerazoom = value;
			}
		}

		public string LocalName
		{
			get
			{
				return m_LocalName;
			}
			set
			{
				m_LocalName = value;
			}
		}

		public bool islicensed
		{
			get
			{
				return m_islicensed != 0;
			}
			set
			{
				m_islicensed = (value ? 1 : 0);
			}
		}

		public int stadhometechzoneminx
		{
			get
			{
				return m_stadhometechzoneminx;
			}
			set
			{
				m_stadhometechzoneminx = value;
			}
		}

		public int stadhometechzonemaxx
		{
			get
			{
				return m_stadhometechzonemaxx;
			}
			set
			{
				m_stadhometechzonemaxx = value;
			}
		}

		public int stadhometechzoneminz
		{
			get
			{
				return m_stadhometechzoneminz;
			}
			set
			{
				m_stadhometechzoneminz = value;
			}
		}

		public int stadhometechzonemaxz
		{
			get
			{
				return m_stadhometechzonemaxz;
			}
			set
			{
				m_stadhometechzonemaxz = value;
			}
		}

		public int stadawaytechzoneminx
		{
			get
			{
				return m_stadawaytechzoneminx;
			}
			set
			{
				m_stadawaytechzoneminx = value;
			}
		}

		public int stadawaytechzonemaxx
		{
			get
			{
				return m_stadawaytechzonemaxx;
			}
			set
			{
				m_stadawaytechzonemaxx = value;
			}
		}

		public int stadawaytechzoneminz
		{
			get
			{
				return m_stadawaytechzoneminz;
			}
			set
			{
				m_stadawaytechzoneminz = value;
			}
		}

		public int stadawaytechzonemaxz
		{
			get
			{
				return m_stadawaytechzonemaxz;
			}
			set
			{
				m_stadawaytechzonemaxz = value;
			}
		}

		public bool HasCloudyDay()
		{
			return false;
		}

		public void SetCloudyDay(bool enable)
		{
			SetTimeOfDay(enable, HasSunnyDay(), HasNight(), HasSunset());
		}

		public void SetSunnyDay(bool enable)
		{
			SetTimeOfDay(HasCloudyDay(), enable, HasNight(), HasSunset());
		}

		public void SetNight(bool enable)
		{
			SetTimeOfDay(HasCloudyDay(), HasSunnyDay(), enable, HasSunset());
		}

		public void SetSunset(bool enable)
		{
			SetTimeOfDay(HasCloudyDay(), HasSunnyDay(), HasNight(), enable);
		}

		private void SetTimeOfDay(bool hasCloudyDay, bool hasSunnyDay, bool hasNight, bool hasSunset)
		{
			int num = GetWeather();
			if (num == 2)
			{
				num = 3;
			}
			int num2 = 0;
			if (hasCloudyDay)
			{
				m_timeofday[num2] = 0;
				m_todweather[num2] = num;
				num2++;
			}
			if (hasSunnyDay)
			{
				m_timeofday[num2] = 1;
				m_todweather[num2] = 0;
				num2++;
			}
			if (hasNight)
			{
				m_timeofday[num2] = 3;
				m_todweather[num2] = num;
				num2++;
			}
			if (hasSunset)
			{
				m_timeofday[num2] = 4;
				m_todweather[num2] = 0;
				num2++;
			}
			for (int i = num2; i < 4; i++)
			{
				m_timeofday[i] = 5;
				m_todweather[i] = 0;
			}
		}

		public bool HasSunnyDay()
		{
			for (int i = 0; i < 4; i++)
			{
				if (m_timeofday[i] == 1)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasNight()
		{
			for (int i = 0; i < 4; i++)
			{
				if (m_timeofday[i] == 3)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasSunset()
		{
			return false;
		}

		public int GetWeather()
		{
			int num = 0;
			for (int i = 0; i < 4; i++)
			{
				if (m_todweather[i] != 5 && m_todweather[i] > num)
				{
					num = m_todweather[i];
				}
			}
			if (num == 3)
			{
				num = 2;
			}
			return num;
		}

		public void SetWeather(int weather)
		{
			if (weather == 2)
			{
				weather = 3;
			}
			for (int i = 0; i < 4; i++)
			{
				m_todweather[i] = ((m_timeofday[i] == 1 || m_timeofday[i] == 3) ? weather : 0);
			}
		}

		public Stadium(int stadiumid)
			: base(stadiumid)
		{
			m_name = "Stadium " + stadiumid.ToString();
			m_LocalName = m_name;
			m_seatcolor = 1;
			m_yearbuilt = 1950;
			m_countrycode = 0;
			m_policetypecode = 1;
			m_sectionfacedbydefault = 0;
			m_hometeamid = 0;
			m_capacity = 10000;
			m_stadiumtype = 0;
			m_stadiumgoalnetstyle = 0;
			m_stadiumgoalnetpattern = 0;
			m_stadiumgloalnetdepth = 229;
			m_stadiumgoalnetlength = 250;
			m_stadiumgoalnetwidth = 744;
			m_stadiummowpattern_code = 0;
			m_stadiumpitchlength = 10500;
			m_stadiumpitchwidth = 6800;
			m_adboardendlinedistance = 500;
			m_adboardsidelinedistance = 500;
			m_adboardtype = 0;
			for (int i = 0; i < 4; i++)
			{
				m_timeofday[i] = 0;
				m_todweather[i] = 0;
			}
			m_dlc = 0;
			m_stadiumgoalnettype = 0;
			m_stadiumgoalnettension = 0;
			m_cameraheight = 15;
			m_camerazoom = 9;
			m_genericrank = -1;
		}

		public Stadium(Record r)
			: base(r.IntField[FI.stadiums_stadiumid])
		{
			Load(r);
		}

		public void LinkCountry(CountryList countryList)
		{
			if (countryList == null)
			{
				return;
			}
			if (m_countrycode == 0)
			{
				m_Country = null;
				return;
			}
			m_Country = (Country)countryList.SearchId(m_countrycode);
			if (m_Country == null)
			{
				m_countrycode = 0;
			}
			else
			{
				m_countrycode = m_Country.Id;
			}
		}

		public void Load(Record r)
		{
			m_name = r.StringField[FI.stadiums_name];
			m_countrycode = r.GetAndCheckIntField(FI.stadiums_countrycode);
			m_hometeamid = r.GetAndCheckIntField(FI.stadiums_hometeamid);
			m_HomeTeam = null;
			m_capacity = r.GetAndCheckIntField(FI.stadiums_capacity);
			m_policetypecode = r.GetAndCheckIntField(FI.stadiums_policetypecode);
			m_seatcolor = r.GetAndCheckIntField(FI.stadiums_seatcolor);
			m_sectionfacedbydefault = r.GetAndCheckIntField(FI.stadiums_sectionfacedbydefault);
			m_stadiumgoalnetstyle = r.GetAndCheckIntField(FI.stadiums_stadiumgoalnetstyle);
			m_stadiumgloalnetdepth = r.GetAndCheckIntField(FI.stadiums_stadiumgloalnetdepth);
			m_stadiumgoalnetlength = r.GetAndCheckIntField(FI.stadiums_stadiumgoalnetlength);
			m_stadiumgoalnetwidth = r.GetAndCheckIntField(FI.stadiums_stadiumgoalnetwidth);
			m_stadiummowpattern_code = r.GetAndCheckIntField(FI.stadiums_stadiummowpattern_code);
			m_stadiumpitchlength = r.GetAndCheckIntField(FI.stadiums_stadiumpitchlength);
			m_stadiumpitchwidth = r.GetAndCheckIntField(FI.stadiums_stadiumpitchwidth);
			m_adboardendlinedistance = r.GetAndCheckIntField(FI.stadiums_adboardendlinedistance);
			m_adboardsidelinedistance = r.GetAndCheckIntField(FI.stadiums_adboardsidelinedistance);
			m_timeofday[0] = r.GetAndCheckIntField(FI.stadiums_timeofday1);
			m_timeofday[1] = r.GetAndCheckIntField(FI.stadiums_timeofday2);
			m_timeofday[2] = r.GetAndCheckIntField(FI.stadiums_timeofday3);
			m_timeofday[3] = r.GetAndCheckIntField(FI.stadiums_timeofday4);
			m_todweather[0] = r.GetAndCheckIntField(FI.stadiums_tod1weather);
			m_todweather[1] = r.GetAndCheckIntField(FI.stadiums_tod2weather);
			m_todweather[2] = r.GetAndCheckIntField(FI.stadiums_tod3weather);
			m_todweather[3] = r.GetAndCheckIntField(FI.stadiums_tod4weather);
			m_dlc = r.GetAndCheckIntField(FI.stadiums_dlc);
			m_stadiumgoalnettype = r.GetAndCheckIntField(FI.stadiums_stadiumgoalnettype);
			if (FI.stadiums_stadiumgoalnetpattern >= 0)
			{
				m_stadiumgoalnetpattern = r.GetAndCheckIntField(FI.stadiums_stadiumgoalnetpattern);
			}
			m_stadiumgoalnettension = r.GetAndCheckIntField(FI.stadiums_stadiumgoalnettension);
			m_cameraheight = r.GetAndCheckIntField(FI.stadiums_cameraheight);
			m_camerazoom = r.GetAndCheckIntField(FI.stadiums_camerazoom);
			m_stadiumtype = r.GetAndCheckIntField(FI.stadiums_stadiumtype);
			if (m_stadiumtype > 1)
			{
				m_stadiumtype = 1;
			}
			m_yearbuilt = r.GetAndCheckIntField(FI.stadiums_yearbuilt);
			m_islicensed = r.GetAndCheckIntField(FI.stadiums_islicensed);
			m_stadhometechzonemaxz = r.GetAndCheckIntField(FI.stadiums_stadhometechzonemaxz);
			m_stadawaytechzonemaxx = r.GetAndCheckIntField(FI.stadiums_stadawaytechzonemaxx);
			m_stadhometechzonemaxx = r.GetAndCheckIntField(FI.stadiums_stadhometechzonemaxx);
			m_stadawaytechzoneminz = r.GetAndCheckIntField(FI.stadiums_stadawaytechzoneminz);
			m_stadhometechzoneminx = r.GetAndCheckIntField(FI.stadiums_stadhometechzoneminx);
			m_stadhometechzoneminz = r.GetAndCheckIntField(FI.stadiums_stadhometechzoneminz);
			m_stadawaytechzonemaxz = r.GetAndCheckIntField(FI.stadiums_stadawaytechzonemaxz);
			m_stadawaytechzoneminx = r.GetAndCheckIntField(FI.stadiums_stadawaytechzoneminx);
			if (FifaEnvironment.Language != null)
			{
				m_LocalName = FifaEnvironment.Language.GetStadiumName(base.Id);
			}
			else
			{
				m_LocalName = string.Empty;
			}
			m_upgradetier = r.GetAndCheckIntField(FI.stadiums_upgradetier);
			m_upgradestyle = r.GetAndCheckIntField(FI.stadiums_upgradestyle);
			m_iseditable = r.GetAndCheckIntField(FI.stadiums_iseditable);
			m_isdynamic = r.GetAndCheckIntField(FI.stadiums_isdynamic);
			m_genericrank = r.GetAndCheckIntField(FI.stadiums_genericrank);
		}

		public override string ToString()
		{
			if (m_name != null && m_name != string.Empty)
			{
				return m_name;
			}
			return "Stadium n. " + base.Id.ToString();
		}

		public string DatabaseString()
		{
			return m_name;
		}

		public void LinkTeam(TeamList teamList)
		{
			if (teamList != null)
			{
				m_HomeTeam = (Team)teamList.SearchId(m_hometeamid);
			}
		}

		public override IdObject Clone(int stadiumid)
		{
			Stadium obj = (Stadium)base.Clone(stadiumid);
			obj.m_name = "Stadium " + stadiumid.ToString();
			obj.m_LocalName = obj.m_name;
			CloneModel(stadiumid);
			ClonePreview(stadiumid, 1);
			ClonePreview(stadiumid, 3);
			if (HasSunnyDay())
			{
				CloneTextures(stadiumid, 1);
				CloneCrowd(stadiumid, 1);
				CloneGlares(stadiumid, 1);
			}
			if (HasNight())
			{
				CloneTextures(stadiumid, 3);
				CloneCrowd(stadiumid, 3);
				CloneGlares(stadiumid, 3);
			}
			CloneRadiosity(stadiumid);
			return obj;
		}

		private void CloneTextures(int newId, int timeofday)
		{
			FifaEnvironment.CloneIntoZdata(TexturesFileName(timeofday), TexturesFileName(newId, timeofday));
		}

		private void ClonePreview(int newId, int timeofday)
		{
			FifaEnvironment.CloneIntoZdata(PreviewBigFileName(base.Id, timeofday), PreviewBigFileName(newId, timeofday));
			FifaEnvironment.CloneIntoZdata(PreviewLargeBigFileName(base.Id, timeofday), PreviewLargeBigFileName(newId, timeofday));
		}

		private void CloneModel(int newId)
		{
			FifaEnvironment.CloneIntoZdata(ModelFileName(base.Id), ModelFileName(newId));
		}

		public Bitmap GetNet()
		{
			return Net.GetNet(m_stadiumgoalnetstyle);
		}

		public bool SetNet(Bitmap bitmap)
		{
			return Net.SetNet(m_stadiumgoalnetstyle, bitmap);
		}

		public bool SetNet(string rx3FileName)
		{
			return Net.SetNet(m_stadiumgoalnetstyle, rx3FileName);
		}

		public bool DeleteNet()
		{
			return Net.DeleteNet(m_stadiumgoalnetstyle);
		}

		public Bitmap GetPolice()
		{
			if (m_policetypecode == 0)
			{
				return null;
			}
			return Police.GetPolice(m_policetypecode, 0);
		}

		public bool SetPolice(Bitmap bitmap)
		{
			if (m_policetypecode == 0)
			{
				return false;
			}
			if (Police.SetPolice(m_policetypecode, 0, bitmap))
			{
				return Police.SetPolice(m_policetypecode, 1, bitmap);
			}
			return false;
		}

		public bool DeletePolice()
		{
			if (m_policetypecode == 0)
			{
				return false;
			}
			if (Police.DeletePolice(m_policetypecode, 0))
			{
				return Police.DeletePolice(m_policetypecode, 1);
			}
			return false;
		}

		public Bitmap GetMowingPattern()
		{
			return MowingPattern.GetMowingPattern(m_stadiummowpattern_code);
		}

		public bool SetMowingPattern(Bitmap bitmap)
		{
			return MowingPattern.SetMowingPattern(m_stadiummowpattern_code, bitmap);
		}

		public bool SetMowingPattern(string rx3FileName)
		{
			return MowingPattern.SetMowingPattern(m_stadiummowpattern_code, rx3FileName);
		}

		public bool DeleteMowingPattern()
		{
			return MowingPattern.DeleteMowingPattern(m_stadiummowpattern_code);
		}

		public void SaveStadium(Record r)
		{
			r.IntField[FI.stadiums_stadiumid] = base.Id;
			r.StringField[FI.stadiums_name] = m_name;
			r.IntField[FI.stadiums_countrycode] = m_countrycode;
			r.IntField[FI.stadiums_hometeamid] = m_hometeamid;
			r.IntField[FI.stadiums_capacity] = m_capacity;
			r.IntField[FI.stadiums_policetypecode] = m_policetypecode;
			r.IntField[FI.stadiums_seatcolor] = m_seatcolor;
			r.IntField[FI.stadiums_sectionfacedbydefault] = m_sectionfacedbydefault;
			r.IntField[FI.stadiums_stadiumgoalnetstyle] = m_stadiumgoalnetstyle;
			r.IntField[FI.stadiums_stadiumgloalnetdepth] = m_stadiumgloalnetdepth;
			r.IntField[FI.stadiums_stadiumgoalnetlength] = m_stadiumgoalnetlength;
			r.IntField[FI.stadiums_stadiumgoalnetwidth] = m_stadiumgoalnetwidth;
			r.IntField[FI.stadiums_stadiummowpattern_code] = m_stadiummowpattern_code;
			r.IntField[FI.stadiums_stadiumpitchlength] = m_stadiumpitchlength;
			r.IntField[FI.stadiums_stadiumpitchwidth] = m_stadiumpitchwidth;
			r.IntField[FI.stadiums_adboardendlinedistance] = m_adboardendlinedistance;
			r.IntField[FI.stadiums_adboardsidelinedistance] = m_adboardsidelinedistance;
			r.IntField[FI.stadiums_timeofday1] = m_timeofday[0];
			r.IntField[FI.stadiums_timeofday2] = m_timeofday[1];
			r.IntField[FI.stadiums_timeofday3] = m_timeofday[2];
			r.IntField[FI.stadiums_timeofday4] = m_timeofday[3];
			r.IntField[FI.stadiums_tod1weather] = m_todweather[0];
			r.IntField[FI.stadiums_tod2weather] = m_todweather[1];
			r.IntField[FI.stadiums_tod3weather] = m_todweather[2];
			r.IntField[FI.stadiums_tod4weather] = m_todweather[3];
			r.IntField[FI.stadiums_dlc] = m_dlc;
			r.IntField[FI.stadiums_stadiumtype] = m_stadiumtype;
			r.IntField[FI.stadiums_yearbuilt] = m_yearbuilt;
			r.IntField[FI.stadiums_stadiumgoalnettype] = m_stadiumgoalnettype;
			if (FI.stadiums_stadiumgoalnetpattern >= 0)
			{
				r.IntField[FI.stadiums_stadiumgoalnetpattern] = m_stadiumgoalnetpattern;
			}
			r.IntField[FI.stadiums_stadiumgoalnettension] = m_stadiumgoalnettension;
			r.IntField[FI.stadiums_cameraheight] = m_cameraheight;
			r.IntField[FI.stadiums_camerazoom] = m_camerazoom;
			r.IntField[FI.stadiums_upgradetier] = m_upgradetier;
			r.IntField[FI.stadiums_upgradestyle] = m_upgradestyle;
			r.IntField[FI.stadiums_iseditable] = m_iseditable;
			r.IntField[FI.stadiums_isdynamic] = m_isdynamic;
			r.IntField[FI.stadiums_genericrank] = m_genericrank;
			r.IntField[FI.stadiums_islicensed] = m_islicensed;
			r.IntField[FI.stadiums_stadhometechzonemaxz] = m_stadhometechzonemaxz;
			r.IntField[FI.stadiums_stadawaytechzonemaxx] = m_stadawaytechzonemaxx;
			r.IntField[FI.stadiums_stadhometechzonemaxx] = m_stadhometechzonemaxx;
			r.IntField[FI.stadiums_stadawaytechzoneminz] = m_stadawaytechzoneminz;
			r.IntField[FI.stadiums_stadhometechzoneminx] = m_stadhometechzoneminx;
			r.IntField[FI.stadiums_stadhometechzoneminz] = m_stadhometechzoneminz;
			r.IntField[FI.stadiums_stadawaytechzonemaxz] = m_stadawaytechzonemaxz;
			r.IntField[FI.stadiums_stadawaytechzoneminx] = m_stadawaytechzoneminx;
			if (FifaEnvironment.Language != null)
			{
				FifaEnvironment.Language.SetStadiumName(base.Id, m_LocalName);
			}
		}

		public void SaveLangTable()
		{
			if (FifaEnvironment.Language != null)
			{
				FifaEnvironment.Language.SetStadiumName(base.Id, m_LocalName);
			}
		}

		public static string PreviewBigFileName(int stadiumid, int timeofday)
		{
			return "data/ui/artassets/stadium/stadium_" + stadiumid.ToString() + "_" + timeofday.ToString() + ".big";
		}

		public string PreviewBigFileName(int timeofday)
		{
			return "data/ui/artassets/stadium/stadium_" + base.Id.ToString() + "_" + timeofday.ToString() + ".big";
		}

		public string PreviewTemplateFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/ui/artassets/stadium/2014_stadium_#_%.big";
			}
			return "data/ui/artassets/stadium/stadium_#_%.big";
		}

		public string PreviewDdsFileName()
		{
			return "2";
		}

		public Bitmap GetPreview(int timeofday)
		{
			return FifaEnvironment.GetArtasset(PreviewBigFileName(timeofday));
		}

		public bool SetPreview(int timeofday, Bitmap bitmap)
		{
			return FifaEnvironment.SetArtasset(ids: new int[2]
			{
				base.Id,
				timeofday
			}, templateBigName: PreviewTemplateFileName(), ddsName: PreviewDdsFileName(), bitmap: bitmap);
		}

		public bool DeletePreview(int timeofday)
		{
			return FifaEnvironment.DeleteFromZdata(PreviewBigFileName(timeofday));
		}

		private void CloneTimeOfDayPreview(int newTimeOfDay, int timeofday)
		{
			FifaEnvironment.CloneIntoZdata(PreviewBigFileName(timeofday), PreviewBigFileName(newTimeOfDay));
		}

		public static string PreviewLargeBigFileName(int stadiumid, int timeofday)
		{
			return "data/ui/artassets/stadiumsbig/st_" + stadiumid.ToString() + "_" + timeofday.ToString() + ".big";
		}

		public string PreviewLargeBigFileName(int timeofday)
		{
			return "data/ui/artassets/stadiumsbig/st_" + base.Id.ToString() + "_" + timeofday.ToString() + ".big";
		}

		public string PreviewLargeTemplateFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/ui/artassets/stadiumsbig/2014_st_#_%.big";
			}
			return "data/ui/artassets/stadiumsbig/st_#_%.big";
		}

		public string PreviewLargeDdsFileName()
		{
			return "2";
		}

		public Bitmap GetPreviewLarge(int timeofday)
		{
			return FifaEnvironment.GetArtasset(PreviewLargeBigFileName(timeofday));
		}

		public bool SetPreviewLarge(int timeofday, Bitmap bitmap)
		{
			return FifaEnvironment.SetArtasset(ids: new int[2]
			{
				base.Id,
				timeofday
			}, templateBigName: PreviewLargeTemplateFileName(), ddsName: PreviewLargeDdsFileName(), bitmap: bitmap);
		}

		public bool DeletePreviewLarge(int timeofday)
		{
			return FifaEnvironment.DeleteFromZdata(PreviewLargeBigFileName(timeofday));
		}

		private void CloneTimeOfDayPreviewLarge(int newTimeOfDay, int timeofday)
		{
			FifaEnvironment.CloneIntoZdata(PreviewLargeBigFileName(timeofday), PreviewLargeBigFileName(newTimeOfDay));
		}

		public static string TexturesFileName(int stadiumid, int timeofday)
		{
			return "data/sceneassets/stadium/stadium_" + stadiumid.ToString() + "_" + timeofday.ToString() + "_textures.rx3";
		}

		public string TexturesFileName(int timeofday)
		{
			return "data/sceneassets/stadium/stadium_" + base.Id.ToString() + "_" + timeofday.ToString() + "_textures.rx3";
		}

		public string TexturesTemplateFileName()
		{
			return "data/sceneassets/stadium/stadium_#_%_textures.rx3";
		}

		public Bitmap[] GetTextures(int timeofday)
		{
			return FifaEnvironment.GetBmpsFromRx3(TexturesFileName(timeofday));
		}

		public bool SetTextures(int timeofday, Bitmap[] bitmaps)
		{
			return FifaEnvironment.ImportBmpsIntoStadium(TexturesFileName(timeofday), bitmaps);
		}

		public bool SetTextures(int timeofday, string rx3FileName)
		{
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, TexturesFileName(timeofday), delete: false, ECompressionMode.Chunkzip);
		}

		public bool DeleteContainer(int timeofday)
		{
			return FifaEnvironment.DeleteFromZdata(TexturesFileName(timeofday));
		}

		public static string ModelFileName(int stadiumid)
		{
			return "data/sceneassets/stadium/stadium_" + stadiumid.ToString() + ".rx3";
		}

		public string ModelFileName()
		{
			return "data/sceneassets/stadium/stadium_" + base.Id.ToString() + ".rx3";
		}

		public string ModelTemplateFileName()
		{
			return "data/sceneassets/stadium/stadium_#.rx3";
		}

		public Rx3File GetModel()
		{
			return FifaEnvironment.GetRx3FromZdata(ModelFileName());
		}

		public bool SetModel(string rx3FileName)
		{
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, ModelFileName(), delete: false, ECompressionMode.Chunkzip);
		}

		public bool DeleteModel(int timeofday)
		{
			return FifaEnvironment.DeleteFromZdata(ModelFileName());
		}

		public static string CrowdFileName(int stadiumid, int timeofday)
		{
			return "data/sceneassets/crowdplacement/crowd_" + stadiumid.ToString() + "_" + timeofday.ToString() + ".dat";
		}

		public string CrowdFileName(int timeofday)
		{
			return "data/sceneassets/crowdplacement/crowd_" + base.Id.ToString() + "_" + timeofday.ToString() + ".dat";
		}

		public bool CloneCrowd(int newId, int timeofday)
		{
			return FifaEnvironment.CloneIntoZdata(CrowdFileName(timeofday), CrowdFileName(newId, timeofday));
		}

		public bool SetCrowd(int timeofday, string datFileName)
		{
			return FifaEnvironment.ImportFileIntoZdataAs(datFileName, CrowdFileName(timeofday), delete: false, ECompressionMode.Chunkzip);
		}

		public void CloneTimeOfDayCrowd(int newTimeOfDay, int timeofday)
		{
			FifaEnvironment.CloneIntoZdata(CrowdFileName(timeofday), CrowdFileName(newTimeOfDay));
		}

		public static string RadiosityFileName(int stadiumid)
		{
			return "data/sceneassets/radiosity/stadium_" + stadiumid.ToString() + ".rad";
		}

		public string RadiosityFileName()
		{
			return "data/sceneassets/radiosity/stadium_" + base.Id.ToString() + ".rad";
		}

		public bool CloneRadiosity(int newId)
		{
			return FifaEnvironment.CloneIntoZdata(RadiosityFileName(), RadiosityFileName(newId));
		}

		public bool SetRadiosity(string radFileName)
		{
			return FifaEnvironment.ImportFileIntoZdataAs(radFileName, RadiosityFileName(), delete: false, ECompressionMode.Chunkzip);
		}

		public static string[] GlaresLightFileNames(int stadiumid, int timeofday)
		{
			return (FifaEnvironment.Year == 14) ? new string[8]
			{
				"data/sceneassets/fx/glares_" + stadiumid.ToString() + "_0.lnx",
				"data/sceneassets/fx/glares_" + stadiumid.ToString() + "_0.rx3",
				"data/sceneassets/fx/glares_" + stadiumid.ToString() + "_1.lnx",
				"data/sceneassets/fx/glares_" + stadiumid.ToString() + "_1.rx3",
				"data/sceneassets/fx/glares_" + stadiumid.ToString() + "_3.lnx",
				"data/sceneassets/fx/glares_" + stadiumid.ToString() + "_3.rx3",
				"data/sceneassets/fx/glares_" + stadiumid.ToString() + "_4.lnx",
				"data/sceneassets/fx/glares_" + stadiumid.ToString() + "_4.rx3"
			} : new string[8]
			{
				"data/sceneassets/fx/glares_" + stadiumid.ToString() + "_" + timeofday.ToString() + "_0.lnx",
				"data/sceneassets/fx/glares_" + stadiumid.ToString() + "_" + timeofday.ToString() + "_0.rx3",
				"data/sceneassets/fx/glares_" + stadiumid.ToString() + "_" + timeofday.ToString() + "_1.lnx",
				"data/sceneassets/fx/glares_" + stadiumid.ToString() + "_" + timeofday.ToString() + "_1.rx3",
				"data/sceneassets/fx/glares_" + stadiumid.ToString() + "_" + timeofday.ToString() + "_2.lnx",
				"data/sceneassets/fx/glares_" + stadiumid.ToString() + "_" + timeofday.ToString() + "_2.rx3",
				"data/sceneassets/fx/glares_" + stadiumid.ToString() + "_" + timeofday.ToString() + "_3.lnx",
				"data/sceneassets/fx/glares_" + stadiumid.ToString() + "_" + timeofday.ToString() + "_3.rx3"
			};
		}

		public static string[] GlaresLightFileNames(int stadiumId)
		{
			return new string[8]
			{
				"data/sceneassets/fx/glares_" + stadiumId.ToString() + "_0.lnx",
				"data/sceneassets/fx/glares_" + stadiumId.ToString() + "_0.rx3",
				"data/sceneassets/fx/glares_" + stadiumId.ToString() + "_1.lnx",
				"data/sceneassets/fx/glares_" + stadiumId.ToString() + "_1.rx3",
				"data/sceneassets/fx/glares_" + stadiumId.ToString() + "_3.lnx",
				"data/sceneassets/fx/glares_" + stadiumId.ToString() + "_3.rx3",
				"data/sceneassets/fx/glares_" + stadiumId.ToString() + "_4.lnx",
				"data/sceneassets/fx/glares_" + stadiumId.ToString() + "_4.rx3"
			};
		}

		public string[] GlaresLightFileNames()
		{
			return new string[8]
			{
				"data/sceneassets/fx/glares_" + base.Id.ToString() + "_0.lnx",
				"data/sceneassets/fx/glares_" + base.Id.ToString() + "_0.rx3",
				"data/sceneassets/fx/glares_" + base.Id.ToString() + "_1.lnx",
				"data/sceneassets/fx/glares_" + base.Id.ToString() + "_1.rx3",
				"data/sceneassets/fx/glares_" + base.Id.ToString() + "_3.lnx",
				"data/sceneassets/fx/glares_" + base.Id.ToString() + "_3.rx3",
				"data/sceneassets/fx/glares_" + base.Id.ToString() + "_4.lnx",
				"data/sceneassets/fx/glares_" + base.Id.ToString() + "_4.rx3"
			};
		}

		public bool CloneGlares(int newId)
		{
			string[] array = GlaresLightFileNames(base.Id);
			string[] array2 = GlaresLightFileNames(newId);
			string path = FifaEnvironment.GameDir + Path.GetDirectoryName(array2[0]);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (Path.GetExtension(array[i]).ToLower() == ".lnx")
				{
					string path2 = FifaEnvironment.TempFolder + "\\" + array[i];
					string text = "glares_" + base.Id.ToString() + "_";
					string newValue = "glares_" + newId.ToString() + "_";
					if (!FifaEnvironment.ExportFileFromZdata(array[i], FifaEnvironment.TempFolder))
					{
						continue;
					}
					StreamWriter streamWriter = new StreamWriter(FifaEnvironment.GameDir + array2[i]);
					StreamReader streamReader = new StreamReader(path2);
					string text2 = null;
					bool flag = false;
					while ((text2 = streamReader.ReadLine()) != null)
					{
						if (!flag && text2.Contains(text))
						{
							text2 = text2.Replace(text, newValue);
							flag = true;
						}
						streamWriter.WriteLine(text2);
					}
					streamReader.Close();
					streamWriter.Close();
				}
				else
				{
					FifaEnvironment.CloneIntoZdata(array[i], array2[i]);
				}
			}
			return true;
		}

		public bool CloneGlares(int newId, int timeofday)
		{
			string[] array = GlaresLightFileNames(base.Id, timeofday);
			string[] array2 = GlaresLightFileNames(newId, timeofday);
			string path = FifaEnvironment.GameDir + Path.GetDirectoryName(array2[0]);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (Path.GetExtension(array[i]).ToLower() == ".lnx")
				{
					string path2 = FifaEnvironment.TempFolder + "\\" + array[i];
					string text = "glares_" + base.Id.ToString() + "_";
					string newValue = "glares_" + newId.ToString() + "_";
					if (!FifaEnvironment.ExportFileFromZdata(array[i], FifaEnvironment.TempFolder))
					{
						continue;
					}
					StreamWriter streamWriter = new StreamWriter(FifaEnvironment.GameDir + array2[i]);
					StreamReader streamReader = new StreamReader(path2);
					string text2 = null;
					bool flag = false;
					while ((text2 = streamReader.ReadLine()) != null)
					{
						if (!flag && text2.Contains(text))
						{
							text2 = text2.Replace(text, newValue);
							flag = true;
						}
						streamWriter.WriteLine(text2);
					}
					streamReader.Close();
					streamWriter.Close();
				}
				else
				{
					FifaEnvironment.CloneIntoZdata(array[i], array2[i]);
				}
			}
			return true;
		}

		public bool SetGlaresLight(string[] sourceFileNames, int timeofday)
		{
			string[] array = GlaresLightFileNames(base.Id, timeofday);
			bool flag = true;
			if (sourceFileNames.Length != array.Length)
			{
				return false;
			}
			for (int i = 0; i < sourceFileNames.Length; i++)
			{
				flag = (File.Exists(sourceFileNames[i]) && flag && FifaEnvironment.ImportFileIntoZdataAs(sourceFileNames[i], array[i], delete: false, ECompressionMode.Chunkzip));
			}
			return flag;
		}

		public bool SetGlaresLight(string[] sourceFileNames)
		{
			string[] array = GlaresLightFileNames(base.Id);
			bool flag = true;
			if (sourceFileNames.Length != array.Length)
			{
				return false;
			}
			for (int i = 0; i < sourceFileNames.Length; i++)
			{
				flag = (File.Exists(sourceFileNames[i]) && flag && FifaEnvironment.ImportFileIntoZdataAs(sourceFileNames[i], array[i], delete: false, ECompressionMode.Chunkzip));
			}
			return flag;
		}
	}
}
