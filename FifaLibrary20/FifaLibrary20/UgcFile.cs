using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace FifaLibrary
{
	public class UgcFile
	{
		private int m_NFiles = -1;

		private UgcDirEntry[] m_UgcDir;

		private BinaryReader m_BinaryReader;

		public int NFiles => m_NFiles;

		public UgcDirEntry[] UgcDir => m_UgcDir;

		public UgcFile(string fileName)
		{
			Load(fileName);
		}

		public bool Load(string fileName)
		{
			FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			if (fileStream == null)
			{
				return false;
			}
			BinaryReader binaryReader = m_BinaryReader = new BinaryReader(fileStream);
			if (binaryReader == null)
			{
				return false;
			}
			return Load(binaryReader);
		}

		public bool Load(BinaryReader r)
		{
			r.BaseStream.Position = 0L;
			r.ReadBytes(56);
			m_NFiles = r.ReadInt32() + 1;
			m_UgcDir = new UgcDirEntry[m_NFiles];
			r.ReadInt32();
			for (int i = 0; i < m_NFiles; i++)
			{
				m_UgcDir[i] = new UgcDirEntry(r);
			}
			return true;
		}

		public bool Extract(int fileIndex, string outputFolder)
		{
			if (fileIndex < 0)
			{
				return false;
			}
			if (fileIndex >= m_NFiles)
			{
				return false;
			}
			m_BinaryReader.BaseStream.Position = m_UgcDir[fileIndex].Offset + 44;
			if (m_UgcDir[fileIndex].FileName == null || m_UgcDir[fileIndex].FileName == string.Empty)
			{
				return false;
			}
			string outputFileName = outputFolder + "\\" + m_UgcDir[fileIndex].ToString();
			if (m_UgcDir[fileIndex].IsPng())
			{
				return ExtractPng(m_BinaryReader, outputFileName);
			}
			return ExtractDb(m_BinaryReader, outputFileName);
		}

		public bool ExtractPng(BinaryReader r, string outputFileName)
		{
			byte[] array = new byte[8];
			int num = 0;
			array[0] = 73;
			array[1] = 69;
			array[2] = 78;
			array[3] = 68;
			array[4] = 174;
			array[5] = 66;
			array[6] = 96;
			array[7] = 130;
			FileStream fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write);
			if (fileStream == null)
			{
				return false;
			}
			BinaryWriter binaryWriter = new BinaryWriter(fileStream);
			if (binaryWriter == null)
			{
				return false;
			}
			do
			{
				byte b = r.ReadByte();
				num = ((b == array[num]) ? (num + 1) : 0);
				binaryWriter.Write(b);
			}
			while (num != array.Length);
			fileStream.Close();
			binaryWriter.Close();
			return true;
		}

		public bool ExtractDb(BinaryReader r, string outputFileName)
		{
			FileStream fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write);
			if (fileStream == null)
			{
				return false;
			}
			BinaryWriter binaryWriter = new BinaryWriter(fileStream);
			if (binaryWriter == null)
			{
				return false;
			}
			binaryWriter.Write(r.ReadBytes(8));
			int num = r.ReadInt32();
			binaryWriter.Write(num);
			num -= 12;
			for (int i = 0; i < num; i++)
			{
				byte value = r.ReadByte();
				binaryWriter.Write(value);
			}
			fileStream.Close();
			binaryWriter.Close();
			return true;
		}

		public void ImportKitGraphics(string xmlFileName, ToolStripStatusLabel statusBar)
		{
			DbFile dbFile = new DbFile(FifaEnvironment.TempFolder + "\\" + m_UgcDir[0].ToString(), xmlFileName);
			Table table = dbFile.GetTable("teams");
			dbFile.GetTable("cz_leagues");
			Table table2 = dbFile.GetTable("cz_teamkits");
			Table table3 = dbFile.GetTable("cz_teams");
			Team[] array = new Team[table.NValidRecords];
			for (int i = 0; i < table.NValidRecords; i++)
			{
				Record record = table.Records[i];
				record.GetAndCheckIntField(FI.teams_assetid);
				int andCheckIntField = record.GetAndCheckIntField(FI.teams_teamid);
				Team team = null;
				team = FifaEnvironment.Teams.FitTeam(record.StringField[FI.teams_teamname], 0);
				if (team == null)
				{
					continue;
				}
				if (statusBar != null)
				{
					statusBar.Text = "Importing Team: " + team.DatabaseName;
					statusBar.Owner.Refresh();
				}
				array[i] = team;
				Bitmap bitmap = null;
				Bitmap bitmap2 = null;
				for (int j = 0; j < table3.NValidRecords; j++)
				{
					Record record2 = table3.Records[j];
					if (record2.GetAndCheckIntField(FI.cz_teams_teamid) == andCheckIntField)
					{
						int andCheckIntField2 = record2.GetAndCheckIntField(FI.cz_teams_hascrestimage);
						int andCheckIntField3 = record2.GetAndCheckIntField(FI.cz_teams_hassponsorimage);
						string text = FifaEnvironment.TempFolder + "\\" + andCheckIntField2.ToString() + ".png";
						if (File.Exists(text))
						{
							bitmap = new Bitmap(text);
						}
						text = FifaEnvironment.TempFolder + "\\" + andCheckIntField3.ToString() + ".png";
						if (File.Exists(text))
						{
							bitmap2 = new Bitmap(text);
						}
						break;
					}
				}
				_ = array[i].assetid;
				for (int k = 0; k < table2.NValidRecords; k++)
				{
					Record record3 = table2.Records[k];
					int andCheckIntField4 = record3.GetAndCheckIntField(FI.cz_teamkits_kitid);
					int andCheckIntField5 = record3.GetAndCheckIntField(FI.cz_teamkits_teamid);
					int andCheckIntField6 = record3.GetAndCheckIntField(FI.cz_teamkits_kittypeid);
					string text2 = Kit.KitTextureFileName(andCheckIntField4, 0, 0);
					if (andCheckIntField != andCheckIntField5)
					{
						continue;
					}
					Kit kit = FifaEnvironment.Kits.GetKit(team.Id, andCheckIntField6);
					if (kit == null)
					{
						kit = new Kit(FifaEnvironment.Kits.GetNewId(), team.Id, andCheckIntField6);
						FifaEnvironment.Kits.Add(kit);
						kit.LinkTeam(FifaEnvironment.Teams);
					}
					kit.jerseyBackName = (record3.IntField[FI.cz_teamkits_jerseybacknameplacementcode] != 0);
					kit.jerseyNameFontCase = record3.IntField[FI.cz_teamkits_jerseybacknamefontcase];
					kit.jerseyNameFont = record3.IntField[FI.cz_teamkits_jerseynamefonttype];
					int red = record3.IntField[FI.cz_teamkits_jerseynamecolorr];
					int green = record3.IntField[FI.cz_teamkits_jerseynamecolorg];
					int blue = record3.IntField[FI.cz_teamkits_jerseynamecolorb];
					kit.JerseyNameColor = Color.FromArgb(255, red, green, blue);
					kit.jerseyNameLayout = record3.IntField[FI.cz_teamkits_jerseynamelayouttype];
					kit.jerseyNumberFont = record3.IntField[FI.cz_teamkits_numberfonttype];
					kit.jerseyNumberColor = record3.IntField[FI.cz_teamkits_numbercolor];
					kit.shortsNumberColor = record3.IntField[FI.cz_teamkits_shortsnumbercolor];
					kit.shortsNumberFont = record3.IntField[FI.cz_teamkits_shortsnumberfonttype];
					kit.shortsNumber = true;
					kit.jerseyCollar = 0;
					red = record3.IntField[FI.cz_teamkits_jerseycolorprimr];
					green = record3.IntField[FI.cz_teamkits_jerseycolorprimg];
					blue = record3.IntField[FI.cz_teamkits_jerseycolorprimb];
					Color color = Color.FromArgb(255, red, green, blue);
					red = record3.IntField[FI.cz_teamkits_jerseycolorsecr];
					green = record3.IntField[FI.cz_teamkits_jerseycolorsecg];
					blue = record3.IntField[FI.cz_teamkits_jerseycolorsecb];
					Color color2 = Color.FromArgb(255, red, green, blue);
					red = record3.IntField[FI.cz_teamkits_jerseycolortertr];
					green = record3.IntField[FI.cz_teamkits_jerseycolortertg];
					blue = record3.IntField[FI.cz_teamkits_jerseycolortertb];
					Color color3 = Color.FromArgb(255, red, green, blue);
					red = record3.IntField[FI.cz_teamkits_shortcolorprimr];
					green = record3.IntField[FI.cz_teamkits_shortcolorprimg];
					blue = record3.IntField[FI.cz_teamkits_shortcolorprimb];
					Color color4 = Color.FromArgb(255, red, green, blue);
					red = record3.IntField[FI.cz_teamkits_shortcolorsecr];
					green = record3.IntField[FI.cz_teamkits_shortcolorsecg];
					blue = record3.IntField[FI.cz_teamkits_shortcolorsecb];
					Color color5 = Color.FromArgb(255, red, green, blue);
					red = record3.IntField[FI.cz_teamkits_shortcolortertr];
					green = record3.IntField[FI.cz_teamkits_shortcolortertg];
					blue = record3.IntField[FI.cz_teamkits_shortcolortertb];
					Color color6 = Color.FromArgb(255, red, green, blue);
					red = record3.IntField[FI.cz_teamkits_sockscolorprimr];
					green = record3.IntField[FI.cz_teamkits_sockscolorprimg];
					blue = record3.IntField[FI.cz_teamkits_sockscolorprimb];
					Color color7 = Color.FromArgb(255, red, green, blue);
					red = record3.IntField[FI.cz_teamkits_sockscolorsecr];
					green = record3.IntField[FI.cz_teamkits_sockscolorsecg];
					blue = record3.IntField[FI.cz_teamkits_sockscolorsecb];
					Color color8 = Color.FromArgb(255, red, green, blue);
					red = record3.IntField[FI.cz_teamkits_sockscolortertr];
					green = record3.IntField[FI.cz_teamkits_sockscolortertg];
					blue = record3.IntField[FI.cz_teamkits_sockscolortertb];
					Color color9 = Color.FromArgb(255, red, green, blue);
					red = record3.IntField[FI.cz_teamkits_sponsorcolourr];
					green = record3.IntField[FI.cz_teamkits_sponsorcolourg];
					blue = record3.IntField[FI.cz_teamkits_sponsorcolourb];
					Color color10 = Color.FromArgb(255, red, green, blue);
					float num = record3.FloatField[FI.cz_teamkits_hotspotjerseyfrontsponsorl];
					float num2 = record3.FloatField[FI.cz_teamkits_hotspotjerseyfrontsponsorr];
					float num3 = record3.FloatField[FI.cz_teamkits_hotspotjerseyfrontsponsort];
					float num4 = record3.FloatField[FI.cz_teamkits_hotspotjerseyfrontsponsorb];
					Kit.KitTextureFileName(team.Id, andCheckIntField6, 0);
					Kit kit2 = FifaEnvironment.Kits.GetKit(andCheckIntField4, 0);
					Bitmap bitmap3 = null;
					if (kit2 != null)
					{
						kit.jerseyCollar = kit2.jerseyCollar;
					}
					if (kit2 != null)
					{
						bitmap3 = kit2.GetMiniKit(0);
						string rx3FileName = FifaEnvironment.TempFolder + "\\" + text2;
						FifaEnvironment.ExportFileFromZdata(text2, FifaEnvironment.TempFolder);
						kit.ImportKitTextures(rx3FileName);
						Bitmap[] kitTextures = kit.GetKitTextures();
						if (bitmap != null)
						{
							kitTextures[0] = (Bitmap)bitmap.Clone();
						}
						else
						{
							kitTextures[0] = null;
						}
						GraphicUtil.ColorizeRGB(kitTextures[1], color, color2, color3, 0, 1024);
						if (bitmap3 != null)
						{
							GraphicUtil.PrepareToColorize(bitmap3, 25, 231);
							GraphicUtil.ColorizeRGB(bitmap3, color, color2, color3, 25, 231);
						}
						if (bitmap2 != null && kitTextures[1] != null)
						{
							Bitmap upperBitmap = (color10.R == 225 && color10.G == 225 && color10.B == 225) ? bitmap2 : GraphicUtil.ColorizeWhite(bitmap2, color10);
							int num5 = (int)((double)num * 1024.0);
							int num6 = (int)((double)num3 * 1024.0);
							int width = (int)((double)num2 * 1024.0) - num5;
							int height = (int)((double)num4 * 1024.0) - num6;
							kitTextures[1] = GraphicUtil.Overlap(destRectangle: new Rectangle(num5, num6, width, height), lowerBitmap: kitTextures[1], upperBitmap: upperBitmap);
						}
						GraphicUtil.ColorizeRGB(kitTextures[3], color7, color8, color9, 0, 194);
						GraphicUtil.ColorizeRGB(kitTextures[3], color4, color5, color6, 194, 512);
						kit.SetKitTextures(kitTextures);
						if (bitmap3 != null)
						{
							kit.SetMiniKit(bitmap3);
						}
					}
				}
			}
			if (statusBar != null)
			{
				statusBar.Text = "Importing complete";
				statusBar.Owner.Refresh();
			}
		}

		public void Import(string xmlFileName, bool useGraphics, ToolStripStatusLabel statusBar)
		{
			DbFile dbFile = new DbFile(FifaEnvironment.TempFolder + "\\" + m_UgcDir[0].ToString(), xmlFileName);
			Table table = dbFile.GetTable("leagueteamlinks");
			Table table2 = dbFile.GetTable("leagues");
			Table table3 = dbFile.GetTable("stadiumassignments");
			Table table4 = dbFile.GetTable("teamstadiumlinks");
			Table table5 = dbFile.GetTable("teamplayerlinks");
			Table table6 = dbFile.GetTable("formations");
			Table table7 = dbFile.GetTable("teams");
			Table table8 = dbFile.GetTable("editedplayernames");
			Table table9 = dbFile.GetTable("players");
			dbFile.GetTable("cz_leagues");
			Table table10 = dbFile.GetTable("cz_teamkits");
			Table table11 = dbFile.GetTable("cz_teams");
			Table table12 = dbFile.GetTable("cz_players");
			Table table13 = dbFile.GetTable("cz_assets");
			League[] array = new League[table2.NValidRecords];
			Team[] array2 = new Team[table7.NValidRecords];
			Player[] array3 = new Player[table9.NValidRecords];
			if (statusBar != null)
			{
				statusBar.Text = "Importing ...";
				statusBar.Owner.Refresh();
			}
			int[] array4 = new int[256];
			for (int i = 0; i < 256; i++)
			{
				array4[i] = 0;
			}
			for (int j = 0; j < table9.NValidRecords; j++)
			{
				Record record = table9.Records[j];
				array4[record.IntField[FI.players_nationality]]++;
			}
			int num = 0;
			int countryid = 216;
			for (int k = 0; k < 256; k++)
			{
				if (array4[k] > num)
				{
					num = array4[k];
					countryid = k;
				}
			}
			Country country = FifaEnvironment.Countries.SearchCountry(countryid);
			int num2 = 400;
			for (int l = 0; l < table2.NValidRecords; l++)
			{
				League league = FifaEnvironment.Leagues.FitLeague(table2.Records[l].StringField[FI.leagues_leaguename], 0);
				int andCheckIntField = table2.Records[l].GetAndCheckIntField(FI.leagues_leagueid);
				if (league != null && country.Id != 216 && league.Country != country)
				{
					league = null;
				}
				bool flag;
				if (league == null)
				{
					num2 = FifaEnvironment.Leagues.GetNextId(num2);
					league = new League(table2.Records[l]);
					flag = false;
				}
				else
				{
					league.RemoveAllTeams();
					flag = true;
					array[andCheckIntField - 385] = league;
				}
				if (league.Id < 385 || league.Id > 389)
				{
					continue;
				}
				array[league.Id - 385] = league;
				league.Id = num2;
				if (league.leaguename.Length > 15)
				{
					league.ShortName = league.leaguename.Substring(0, 15);
				}
				else
				{
					league.ShortName = league.leaguename;
				}
				league.LongName = league.leaguename;
				league.Country = country;
				if (!flag)
				{
					FifaEnvironment.Leagues.InsertId(league);
				}
				if (!useGraphics)
				{
					continue;
				}
				for (int m = 0; m < table13.NValidRecords; m++)
				{
					Record record2 = table13.Records[m];
					if (record2.GetAndCheckIntField(FI.cz_assets_dbid) == 385 + l)
					{
						string text = string.Concat(str2: record2.GetAndCheckIntField(FI.cz_assets_crestid).ToString(), str0: FifaEnvironment.TempFolder, str1: "\\", str3: ".png");
						if (File.Exists(text))
						{
							Bitmap srcBitmap = new Bitmap(text);
							Bitmap bitmap = new Bitmap(256, 256, PixelFormat.Format32bppPArgb);
							Bitmap bitmap2 = new Bitmap(256, 256, PixelFormat.Format32bppPArgb);
							Bitmap bitmap3 = new Bitmap(256, 64, PixelFormat.Format32bppPArgb);
							Rectangle srcRect = new Rectangle(0, 0, 128, 128);
							Rectangle destRect = new Rectangle(32, 32, 192, 192);
							Rectangle destRect2 = new Rectangle(25, 0, 150, 150);
							Rectangle destRect3 = new Rectangle(145, 4, 56, 56);
							GraphicUtil.RemapRectangle(srcBitmap, srcRect, bitmap, destRect);
							GraphicUtil.RemapRectangle(srcBitmap, srcRect, bitmap2, destRect2);
							GraphicUtil.RemapRectangle(srcBitmap, srcRect, bitmap3, destRect3);
							array[l].SetAnimLogo(bitmap);
							array[l].SetAnimLogoDark(bitmap);
							array[l].SetSmallLogo(bitmap2);
							array[l].SetSmallLogoDark(bitmap2);
							array[l].SetTinyLogo(bitmap3);
							array[l].SetTinyLogoDark(bitmap3);
							break;
						}
					}
				}
			}
			int num3 = 130020;
			for (int n = 0; n < table7.NValidRecords; n++)
			{
				Record record3 = table7.Records[n];
				int andCheckIntField2 = record3.GetAndCheckIntField(FI.teams_assetid);
				int andCheckIntField3 = record3.GetAndCheckIntField(FI.teams_teamid);
				Team team = null;
				bool flag2 = false;
				if (andCheckIntField2 != 33554432)
				{
					team = (Team)FifaEnvironment.Teams.SearchId(andCheckIntField2);
					if (team != null)
					{
						team.Roster.ResetToEmpty();
						flag2 = true;
					}
				}
				if (team == null)
				{
					team = FifaEnvironment.Teams.FitTeam(record3.StringField[FI.teams_teamname], 0);
					if (team != null && country.Id != 216 && team.Country != country)
					{
						team = null;
					}
					team?.Roster.ResetToEmpty();
					flag2 = (team != null);
				}
				if (team == null)
				{
					num3 = FifaEnvironment.Teams.GetNextId(num3);
					team = new Team(table7.Records[n]);
					team.Id = num3;
					team.Country = country;
					FifaEnvironment.Teams.InsertId(team);
				}
				if (statusBar != null)
				{
					statusBar.Text = "Importing Team: " + team.DatabaseName;
					statusBar.Owner.Refresh();
				}
				array2[n] = team;
				team.assetid = andCheckIntField3;
				team.TeamNameFull = team.DatabaseName;
				if (team.DatabaseName.Length > 15)
				{
					team.TeamNameAbbr15 = team.DatabaseName.Substring(0, 15);
				}
				else
				{
					team.TeamNameAbbr15 = team.DatabaseName;
				}
				if (team.DatabaseName.Length > 10)
				{
					team.TeamNameAbbr10 = team.DatabaseName.Substring(0, 10);
				}
				else
				{
					team.TeamNameAbbr10 = team.DatabaseName;
				}
				if (flag2)
				{
					continue;
				}
				andCheckIntField2 = array2[n].assetid;
				Bitmap bitmap4 = null;
				Bitmap bitmap5 = null;
				if (useGraphics)
				{
					for (int num4 = 0; num4 < table11.NValidRecords; num4++)
					{
						Record record4 = table11.Records[num4];
						if (record4.GetAndCheckIntField(FI.cz_teams_teamid) == andCheckIntField2)
						{
							int andCheckIntField4 = record4.GetAndCheckIntField(FI.cz_teams_hascrestimage);
							int andCheckIntField5 = record4.GetAndCheckIntField(FI.cz_teams_hassponsorimage);
							string text2 = FifaEnvironment.TempFolder + "\\" + andCheckIntField4.ToString() + ".png";
							if (File.Exists(text2))
							{
								bitmap4 = new Bitmap(text2);
								Rectangle srcRect2 = new Rectangle(0, 0, 128, 128);
								Bitmap destBitmap = new Bitmap(256, 256, PixelFormat.Format32bppPArgb);
								Bitmap destBitmap2 = new Bitmap(128, 128, PixelFormat.Format32bppPArgb);
								Bitmap bitmap6 = new Bitmap(256, 256, PixelFormat.Format32bppPArgb);
								Bitmap bitmap7 = new Bitmap(64, 64, PixelFormat.Format32bppPArgb);
								Bitmap bitmap8 = new Bitmap(32, 32, PixelFormat.Format32bppPArgb);
								Bitmap bitmap9 = new Bitmap(16, 16, PixelFormat.Format32bppPArgb);
								Rectangle destRect4 = new Rectangle(0, 0, 150, 150);
								Rectangle destRect5 = new Rectangle(0, 0, 102, 102);
								Rectangle destRect6 = new Rectangle(0, 0, 256, 256);
								Rectangle destRect7 = new Rectangle(0, 0, 50, 50);
								Rectangle destRect8 = new Rectangle(0, 0, 32, 32);
								Rectangle destRect9 = new Rectangle(0, 0, 16, 16);
								GraphicUtil.RemapRectangle(bitmap4, srcRect2, destBitmap, destRect4);
								GraphicUtil.RemapRectangle(bitmap4, srcRect2, destBitmap2, destRect5);
								GraphicUtil.RemapRectangle(bitmap4, srcRect2, bitmap6, destRect6);
								GraphicUtil.RemapRectangle(bitmap4, srcRect2, bitmap7, destRect7);
								GraphicUtil.RemapRectangle(bitmap4, srcRect2, bitmap8, destRect8);
								GraphicUtil.RemapRectangle(bitmap4, srcRect2, bitmap9, destRect9);
								array2[n].SetCrest(bitmap6);
								array2[n].SetCrest16(bitmap9);
								array2[n].SetCrest32(bitmap8);
								array2[n].SetCrest50(bitmap7);
								array2[n].SetCrestDark(bitmap6);
								array2[n].SetCrest16Dark(bitmap9);
								array2[n].SetCrest32Dark(bitmap8);
								array2[n].SetCrest50Dark(bitmap7);
							}
							text2 = FifaEnvironment.TempFolder + "\\" + andCheckIntField5.ToString() + ".png";
							if (File.Exists(text2))
							{
								bitmap5 = new Bitmap(text2);
							}
							break;
						}
					}
				}
				for (int num5 = 0; num5 < table10.NValidRecords; num5++)
				{
					Record record5 = table10.Records[num5];
					int andCheckIntField6 = record5.GetAndCheckIntField(FI.cz_teamkits_kitid);
					int andCheckIntField7 = record5.GetAndCheckIntField(FI.cz_teamkits_teamid);
					int andCheckIntField8 = record5.GetAndCheckIntField(FI.cz_teamkits_kittypeid);
					string text3 = Kit.KitTextureFileName(andCheckIntField6, 0, 0);
					if (team.assetid != andCheckIntField7)
					{
						continue;
					}
					Kit kit = new Kit(FifaEnvironment.Kits.GetNewId(), team.Id, andCheckIntField8);
					FifaEnvironment.Kits.Add(kit);
					kit.LinkTeam(FifaEnvironment.Teams);
					kit.jerseyBackName = (record5.IntField[FI.cz_teamkits_jerseybacknameplacementcode] != 0);
					kit.jerseyNameFontCase = record5.IntField[FI.cz_teamkits_jerseybacknamefontcase];
					kit.jerseyNameFont = record5.IntField[FI.cz_teamkits_jerseynamefonttype];
					int red = record5.IntField[FI.cz_teamkits_jerseynamecolorr];
					int green = record5.IntField[FI.cz_teamkits_jerseynamecolorg];
					int blue = record5.IntField[FI.cz_teamkits_jerseynamecolorb];
					kit.JerseyNameColor = Color.FromArgb(255, red, green, blue);
					kit.jerseyNameLayout = record5.IntField[FI.cz_teamkits_jerseynamelayouttype];
					kit.jerseyNumberFont = record5.IntField[FI.cz_teamkits_numberfonttype];
					kit.jerseyNumberColor = record5.IntField[FI.cz_teamkits_numbercolor];
					kit.shortsNumberColor = record5.IntField[FI.cz_teamkits_shortsnumbercolor];
					kit.shortsNumberFont = record5.IntField[FI.cz_teamkits_shortsnumberfonttype];
					kit.shortsNumber = true;
					kit.jerseyCollar = 0;
					red = record5.IntField[FI.cz_teamkits_jerseycolorprimr];
					green = record5.IntField[FI.cz_teamkits_jerseycolorprimg];
					blue = record5.IntField[FI.cz_teamkits_jerseycolorprimb];
					Color color2 = kit.TeamColor1 = Color.FromArgb(255, red, green, blue);
					red = record5.IntField[FI.cz_teamkits_jerseycolorsecr];
					green = record5.IntField[FI.cz_teamkits_jerseycolorsecg];
					blue = record5.IntField[FI.cz_teamkits_jerseycolorsecb];
					Color color4 = kit.TeamColor2 = Color.FromArgb(255, red, green, blue);
					red = record5.IntField[FI.cz_teamkits_jerseycolortertr];
					green = record5.IntField[FI.cz_teamkits_jerseycolortertg];
					blue = record5.IntField[FI.cz_teamkits_jerseycolortertb];
					Color color6 = kit.TeamColor3 = Color.FromArgb(255, red, green, blue);
					red = record5.IntField[FI.cz_teamkits_shortcolorprimr];
					green = record5.IntField[FI.cz_teamkits_shortcolorprimg];
					blue = record5.IntField[FI.cz_teamkits_shortcolorprimb];
					Color color7 = Color.FromArgb(255, red, green, blue);
					red = record5.IntField[FI.cz_teamkits_shortcolorsecr];
					green = record5.IntField[FI.cz_teamkits_shortcolorsecg];
					blue = record5.IntField[FI.cz_teamkits_shortcolorsecb];
					Color color8 = Color.FromArgb(255, red, green, blue);
					red = record5.IntField[FI.cz_teamkits_shortcolortertr];
					green = record5.IntField[FI.cz_teamkits_shortcolortertg];
					blue = record5.IntField[FI.cz_teamkits_shortcolortertb];
					Color color9 = Color.FromArgb(255, red, green, blue);
					red = record5.IntField[FI.cz_teamkits_sockscolorprimr];
					green = record5.IntField[FI.cz_teamkits_sockscolorprimg];
					blue = record5.IntField[FI.cz_teamkits_sockscolorprimb];
					Color color10 = Color.FromArgb(255, red, green, blue);
					red = record5.IntField[FI.cz_teamkits_sockscolorsecr];
					green = record5.IntField[FI.cz_teamkits_sockscolorsecg];
					blue = record5.IntField[FI.cz_teamkits_sockscolorsecb];
					Color color11 = Color.FromArgb(255, red, green, blue);
					red = record5.IntField[FI.cz_teamkits_sockscolortertr];
					green = record5.IntField[FI.cz_teamkits_sockscolortertg];
					blue = record5.IntField[FI.cz_teamkits_sockscolortertb];
					Color color12 = Color.FromArgb(255, red, green, blue);
					red = record5.IntField[FI.cz_teamkits_sponsorcolourr];
					green = record5.IntField[FI.cz_teamkits_sponsorcolourg];
					blue = record5.IntField[FI.cz_teamkits_sponsorcolourb];
					Color color13 = Color.FromArgb(255, red, green, blue);
					float num6 = record5.FloatField[FI.cz_teamkits_hotspotjerseyfrontsponsorl];
					float num7 = record5.FloatField[FI.cz_teamkits_hotspotjerseyfrontsponsorr];
					float num8 = record5.FloatField[FI.cz_teamkits_hotspotjerseyfrontsponsort];
					float num9 = record5.FloatField[FI.cz_teamkits_hotspotjerseyfrontsponsorb];
					Kit.KitTextureFileName(team.Id, andCheckIntField8, 0);
					Kit kit2 = FifaEnvironment.Kits.GetKit(andCheckIntField6, 0);
					Bitmap bitmap10 = null;
					if (kit2 != null)
					{
						kit.jerseyCollar = kit2.jerseyCollar;
					}
					if (useGraphics && kit2 != null)
					{
						bitmap10 = kit2.GetMiniKit(0);
						string rx3FileName = FifaEnvironment.TempFolder + "\\" + text3;
						FifaEnvironment.ExportFileFromZdata(text3, FifaEnvironment.TempFolder);
						kit.ImportKitTextures(rx3FileName);
						Bitmap[] kitTextures = kit.GetKitTextures();
						if (bitmap4 != null)
						{
							kitTextures[0] = (Bitmap)bitmap4.Clone();
						}
						else
						{
							kitTextures[0] = null;
						}
						GraphicUtil.ColorizeRGB(kitTextures[1], color2, color4, color6, 0, 1024);
						if (bitmap10 != null)
						{
							GraphicUtil.PrepareToColorize(bitmap10, 25, 231);
							GraphicUtil.ColorizeRGB(bitmap10, color2, color4, color6, 25, 231);
						}
						if (bitmap5 != null && kitTextures[1] != null)
						{
							Bitmap upperBitmap = (color13.R == 225 && color13.G == 225 && color13.B == 225) ? bitmap5 : GraphicUtil.ColorizeWhite(bitmap5, color13);
							int num10 = (int)((double)num6 * 1024.0);
							int num11 = (int)((double)num8 * 1024.0);
							int width = (int)((double)num7 * 1024.0) - num10;
							int height = (int)((double)num9 * 1024.0) - num11;
							kitTextures[1] = GraphicUtil.Overlap(destRectangle: new Rectangle(num10, num11, width, height), lowerBitmap: kitTextures[1], upperBitmap: upperBitmap);
						}
						GraphicUtil.ColorizeRGB(kitTextures[3], color10, color11, color12, 0, 194);
						GraphicUtil.ColorizeRGB(kitTextures[3], color7, color8, color9, 194, 512);
						kit.SetKitTextures(kitTextures);
						if (bitmap10 != null)
						{
							kit.SetMiniKit(bitmap10);
						}
					}
				}
			}
			for (int num12 = 0; num12 < array2.Length; num12++)
			{
				for (int num13 = 0; num13 < array2.Length; num13++)
				{
					if (array2[num12].rivalteam == array2[num13].assetid)
					{
						array2[num12].RivalTeam = array2[num13];
						break;
					}
				}
				if (array2[num12].RivalTeam == null)
				{
					array2[num12].RivalTeam = (Team)FifaEnvironment.Teams.SearchId(array2[num12].rivalteam);
				}
				if (array2[num12].RivalTeam == null)
				{
					array2[num12].RivalTeam = array2[0];
				}
			}
			for (int num14 = 0; num14 < table3.NValidRecords; num14++)
			{
				Record record6 = table3.Records[num14];
				int andCheckIntField9 = record6.GetAndCheckIntField(FI.stadiumassignments_teamid);
				for (int num15 = 0; num15 < array2.Length; num15++)
				{
					if (array2[num15].assetid == andCheckIntField9)
					{
						array2[num15].stadiumcustomname = record6.GetAndCheckStringField(FI.stadiumassignments_stadiumcustomname);
						break;
					}
				}
			}
			for (int num16 = 0; num16 < table4.NValidRecords; num16++)
			{
				Record record7 = table4.Records[num16];
				int andCheckIntField10 = record7.GetAndCheckIntField(FI.teamstadiumlinks_teamid);
				for (int num17 = 0; num17 < array2.Length; num17++)
				{
					if (array2[num17].assetid == andCheckIntField10)
					{
						array2[num17].stadiumid = record7.GetAndCheckIntField(FI.teamstadiumlinks_stadiumid);
						array2[num17].LinkStadium(FifaEnvironment.Stadiums);
						break;
					}
				}
			}
			for (int num18 = 0; num18 < table11.NValidRecords; num18++)
			{
				Record record8 = table11.Records[num18];
				int andCheckIntField11 = record8.GetAndCheckIntField(FI.cz_teams_teamid);
				for (int num19 = 0; num19 < array2.Length; num19++)
				{
					if (array2[num19].assetid == andCheckIntField11)
					{
						array2[num19].TeamNameAbbr3 = record8.GetAndCheckStringField(FI.cz_teams_teamabbrev3);
						break;
					}
				}
			}
			for (int num20 = 0; num20 < table6.NValidRecords; num20++)
			{
				Record obj = table6.Records[num20];
				Formation formation = new Formation(obj);
				Formation formation2 = FifaEnvironment.GenericFormations.GetExactFormation(formation);
				if (formation2 == null)
				{
					int num21 = formation.Id = FifaEnvironment.Formations.GetNewId();
					formation.teamid = -1;
					FifaEnvironment.Formations.InsertId(formation);
					FifaEnvironment.GenericFormations.InsertId(formation);
					formation2 = formation;
				}
				int andCheckIntField12 = obj.GetAndCheckIntField(FI.formations_teamid);
				for (int num22 = 0; num22 < array2.Length; num22++)
				{
					if (array2[num22].assetid == andCheckIntField12)
					{
						array2[num22].Formation = formation2;
						array2[num22].formationid = formation2.Id;
						break;
					}
				}
			}
			statusBar.Text = "Importing Players ...";
			int num23 = 230000;
			for (int num24 = 0; num24 < table9.NValidRecords; num24++)
			{
				Record record9 = table9.Records[num24];
				int andCheckIntField13 = record9.GetAndCheckIntField(FI.players_playerid);
				int num25 = 0;
				for (int num26 = 0; num26 < table12.NValidRecords; num26++)
				{
					Record record10 = table12.Records[num26];
					if (record10.GetAndCheckIntField(FI.cz_players_playerid) == andCheckIntField13)
					{
						num25 = record10.GetAndCheckIntField(FI.cz_players_assetid);
						break;
					}
				}
				Player player = null;
				bool flag3 = false;
				if (num25 != 0)
				{
					player = (Player)FifaEnvironment.Players.SearchId(num25);
					if (player != null)
					{
						flag3 = true;
					}
				}
				if (player == null)
				{
					DateTime birthdate = FifaUtil.ConvertToDate(record9.GetAndCheckIntField(FI.players_birthdate));
					string firstname = null;
					string text4 = null;
					string text5 = null;
					string text6 = null;
					for (int num27 = 0; num27 < table8.NValidRecords; num27++)
					{
						Record record11 = table8.Records[num27];
						if (record11.IntField[FI.editedplayernames_playerid] == andCheckIntField13)
						{
							firstname = record11.GetAndCheckStringField(FI.editedplayernames_firstname);
							text4 = record11.GetAndCheckStringField(FI.editedplayernames_surname);
							text5 = record11.GetAndCheckStringField(FI.editedplayernames_commonname);
							if (text5.Contains(text4))
							{
								text5 = null;
							}
							text6 = record11.GetAndCheckStringField(FI.editedplayernames_playerjerseyname);
							if (text6 != text4 && text6.Contains(text4))
							{
								text6 = text4;
							}
							break;
						}
					}
					player = FifaEnvironment.Players.FitPlayer(firstname, text4, birthdate);
					if (player == null)
					{
						if (num25 != 0)
						{
							player = new Player(table9.Records[num24]);
							player.Id = num25;
						}
						else
						{
							num23 = FifaEnvironment.Players.GetNextId(num23);
							player = new Player(table9.Records[num24]);
							player.Id = num23;
						}
						player.firstname = firstname;
						player.lastname = text4;
						player.commonname = text5;
						player.playerjerseyname = text6;
						FifaEnvironment.Players.InsertId(player);
						player.LinkCountry(FifaEnvironment.Countries);
					}
				}
				if (flag3)
				{
					foreach (Team playingForTeam in player.m_PlayingForTeams)
					{
						if (playingForTeam.Id == 111592)
						{
							player.NotPlayFor(playingForTeam);
							break;
						}
					}
				}
				array3[num24] = player;
				player.m_assetid = andCheckIntField13;
			}
			for (int num28 = 0; num28 < table.NValidRecords; num28++)
			{
				Record obj2 = table.Records[num28];
				int andCheckIntField14 = obj2.GetAndCheckIntField(FI.leagueteamlinks_leagueid);
				int andCheckIntField15 = obj2.GetAndCheckIntField(FI.leagueteamlinks_teamid);
				League league2 = null;
				switch (andCheckIntField14)
				{
				case 384:
					league2 = League.GetDefaultLeague();
					break;
				case 385:
				case 386:
				case 387:
				case 388:
				case 389:
					league2 = array[andCheckIntField14 - 385];
					break;
				}
				if (league2 == null)
				{
					continue;
				}
				for (int num29 = 0; num29 < array2.Length; num29++)
				{
					if (array2[num29].assetid == andCheckIntField15)
					{
						league2.AddTeam(array2[num29]);
						break;
					}
				}
			}
			for (int num30 = 0; num30 < table5.NValidRecords; num30++)
			{
				Record record12 = table5.Records[num30];
				int andCheckIntField16 = record12.GetAndCheckIntField(FI.teamplayerlinks_playerid);
				int andCheckIntField17 = record12.GetAndCheckIntField(FI.teamplayerlinks_teamid);
				for (int num31 = 0; num31 < array3.Length; num31++)
				{
					if (array3[num31].m_assetid != andCheckIntField16)
					{
						continue;
					}
					for (int num32 = 0; num32 < array2.Length; num32++)
					{
						if (array2[num32].assetid == andCheckIntField17)
						{
							array3[num31].PlayFor(array2[num32]);
							TeamPlayer value = new TeamPlayer(record12, array3[num31], array2[num32]);
							array2[num32].Roster.Add(value);
							if (array2[num32].captainid == array3[num31].m_assetid)
							{
								array2[num32].PlayerCaptain = array3[num31];
								array2[num32].captainid = array3[num31].Id;
							}
							if (array2[num32].freekicktakerid == array3[num31].m_assetid)
							{
								array2[num32].PlayerFreeKick = array3[num31];
								array2[num32].freekicktakerid = array3[num31].Id;
							}
							if (array2[num32].leftcornerkicktakerid == array3[num31].m_assetid)
							{
								array2[num32].PlayerLeftCorner = array3[num31];
								array2[num32].leftcornerkicktakerid = array3[num31].Id;
							}
							if (array2[num32].longkicktakerid == array3[num31].m_assetid)
							{
								array2[num32].PlayerLongKick = array3[num31];
								array2[num32].longkicktakerid = array3[num31].Id;
							}
							if (array2[num32].penaltytakerid == array3[num31].m_assetid)
							{
								array2[num32].PlayerPenalty = array3[num31];
								array2[num32].penaltytakerid = array3[num31].Id;
							}
							if (array2[num32].rightcornerkicktakerid == array3[num31].m_assetid)
							{
								array2[num32].PlayerRightCorner = array3[num31];
								array2[num32].rightcornerkicktakerid = array3[num31].Id;
							}
							break;
						}
					}
					break;
				}
			}
			for (int num33 = 0; num33 < array3.Length; num33++)
			{
				array3[num33].m_assetid = array3[num33].Id;
			}
			for (int num34 = 0; num34 < array2.Length; num34++)
			{
				array2[num34].assetid = array2[num34].Id;
			}
			if (statusBar != null)
			{
				statusBar.Text = "Importing complete";
				statusBar.Owner.Refresh();
			}
		}

		public void ImportPlayers(string xmlFileName, bool useGraphics, ToolStripStatusLabel statusBar)
		{
			DbFile dbFile = new DbFile(FifaEnvironment.TempFolder + "\\" + m_UgcDir[0].ToString(), xmlFileName);
			Table table = dbFile.GetTable("editedplayernames");
			Table table2 = dbFile.GetTable("players");
			Table table3 = dbFile.GetTable("cz_players");
			dbFile.GetTable("cz_assets");
			Player[] array = new Player[table2.NValidRecords];
			if (statusBar != null)
			{
				statusBar.Text = "Importing ...";
				statusBar.Owner.Refresh();
			}
			int[] array2 = new int[256];
			for (int i = 0; i < 256; i++)
			{
				array2[i] = 0;
			}
			for (int j = 0; j < table2.NValidRecords; j++)
			{
				Record record = table2.Records[j];
				array2[record.IntField[FI.players_nationality]]++;
			}
			int num = 0;
			int countryid = 216;
			for (int k = 0; k < 256; k++)
			{
				if (array2[k] > num)
				{
					num = array2[k];
					countryid = k;
				}
			}
			FifaEnvironment.Countries.SearchCountry(countryid);
			statusBar.Text = "Importing Players ...";
			int num2 = 230000;
			for (int l = 0; l < table2.NValidRecords; l++)
			{
				Record record2 = table2.Records[l];
				int andCheckIntField = record2.GetAndCheckIntField(FI.players_playerid);
				int num3 = 0;
				for (int m = 0; m < table3.NValidRecords; m++)
				{
					Record record3 = table3.Records[m];
					if (record3.GetAndCheckIntField(FI.cz_players_playerid) == andCheckIntField)
					{
						num3 = record3.GetAndCheckIntField(FI.cz_players_assetid);
						break;
					}
				}
				Player player = null;
				if (num3 != 0)
				{
					player = (Player)FifaEnvironment.Players.SearchId(num3);
				}
				if (player == null)
				{
					DateTime birthdate = FifaUtil.ConvertToDate(record2.GetAndCheckIntField(FI.players_birthdate));
					string firstname = null;
					string text = null;
					string text2 = null;
					string text3 = null;
					for (int n = 0; n < table.NValidRecords; n++)
					{
						Record record4 = table.Records[n];
						if (record4.IntField[FI.editedplayernames_playerid] == andCheckIntField)
						{
							firstname = record4.GetAndCheckStringField(FI.editedplayernames_firstname);
							text = record4.GetAndCheckStringField(FI.editedplayernames_surname);
							text2 = record4.GetAndCheckStringField(FI.editedplayernames_commonname);
							if (text2.Contains(text))
							{
								text2 = null;
							}
							text3 = record4.GetAndCheckStringField(FI.editedplayernames_playerjerseyname);
							if (text3 != text && text3.Contains(text))
							{
								text3 = text;
							}
							break;
						}
					}
					player = FifaEnvironment.Players.FitPlayer(firstname, text, birthdate);
					if (player == null)
					{
						if (num3 != 0)
						{
							player = new Player(table2.Records[l]);
							player.Id = num3;
						}
						else
						{
							num2 = FifaEnvironment.Players.GetNextId(num2);
							player = new Player(table2.Records[l]);
							player.Id = num2;
						}
						player.firstname = firstname;
						player.lastname = text;
						player.commonname = text2;
						player.playerjerseyname = text3;
						FifaEnvironment.Players.InsertId(player);
						player.LinkCountry(FifaEnvironment.Countries);
					}
				}
				array[l] = player;
				player.m_assetid = andCheckIntField;
			}
			for (int num4 = 0; num4 < array.Length; num4++)
			{
				array[num4].m_assetid = array[num4].Id;
			}
			if (statusBar != null)
			{
				statusBar.Text = "Importing complete";
				statusBar.Owner.Refresh();
			}
		}

		public void UpdateRosters(string xmlFileName, bool useKitGraphics, ToolStripStatusLabel statusBar)
		{
			DbFile dbFile = new DbFile(FifaEnvironment.TempFolder + "\\" + m_UgcDir[0].ToString(), xmlFileName);
			dbFile.GetTable("leagueteamlinks");
			Table table = dbFile.GetTable("leagues");
			dbFile.GetTable("stadiumassignments");
			dbFile.GetTable("teamstadiumlinks");
			Table table2 = dbFile.GetTable("teamplayerlinks");
			Table table3 = dbFile.GetTable("formations");
			Table table4 = dbFile.GetTable("teams");
			Table table5 = dbFile.GetTable("editedplayernames");
			Table table6 = dbFile.GetTable("players");
			dbFile.GetTable("cz_leagues");
			dbFile.GetTable("cz_teamkits");
			dbFile.GetTable("cz_teams");
			Table table7 = dbFile.GetTable("cz_players");
			dbFile.GetTable("cz_assets");
			_ = new League[table.NValidRecords];
			Team[] array = new Team[table4.NValidRecords];
			Player[] array2 = new Player[table6.NValidRecords];
			if (statusBar != null)
			{
				statusBar.Text = "Updating ...";
				statusBar.Owner.Refresh();
			}
			int[] array3 = new int[256];
			for (int i = 0; i < 256; i++)
			{
				array3[i] = 0;
			}
			for (int j = 0; j < table6.NValidRecords; j++)
			{
				Record record = table6.Records[j];
				array3[record.IntField[FI.players_nationality]]++;
			}
			int num = 0;
			int countryid = 216;
			for (int k = 0; k < 256; k++)
			{
				if (array3[k] > num)
				{
					num = array3[k];
					countryid = k;
				}
			}
			Country country = FifaEnvironment.Countries.SearchCountry(countryid);
			int num2 = 0;
			for (int l = 0; l < table4.NValidRecords; l++)
			{
				Record record2 = table4.Records[l];
				int andCheckIntField = record2.GetAndCheckIntField(FI.teams_assetid);
				int andCheckIntField2 = record2.GetAndCheckIntField(FI.teams_teamid);
				Team team = null;
				if (andCheckIntField != 33554432)
				{
					team = (Team)FifaEnvironment.Teams.SearchId(andCheckIntField);
					team?.Roster.ResetToEmpty();
				}
				if (team == null)
				{
					team = FifaEnvironment.Teams.FitTeam(record2.StringField[FI.teams_teamname], 0);
					if (team != null && country.Id != 216 && team.Country != country)
					{
						team = null;
					}
					team?.Roster.ResetToEmpty();
				}
				if (team != null)
				{
					array[num2++] = team;
					team.assetid = andCheckIntField2;
				}
			}
			for (int m = 0; m < table3.NValidRecords; m++)
			{
				Record record3 = table3.Records[m];
				int andCheckIntField3 = record3.GetAndCheckIntField(FI.formations_teamid);
				for (int n = 0; n < num2; n++)
				{
					if (array[n].assetid == andCheckIntField3)
					{
						Formation formation = new Formation(record3);
						Formation formation2 = FifaEnvironment.GenericFormations.GetExactFormation(formation);
						if (formation2 == null)
						{
							int num3 = formation.Id = FifaEnvironment.Formations.GetNewId();
							formation.teamid = -1;
							FifaEnvironment.Formations.InsertId(formation);
							FifaEnvironment.GenericFormations.InsertId(formation);
							formation2 = formation;
						}
						array[n].Formation = formation2;
						array[n].formationid = formation2.Id;
						break;
					}
				}
			}
			statusBar.Text = "Updating Players ...";
			int num4 = 230000;
			int num5 = 0;
			for (int num6 = 0; num6 < table6.NValidRecords; num6++)
			{
				Record record4 = table6.Records[num6];
				int andCheckIntField4 = record4.GetAndCheckIntField(FI.players_playerid);
				bool flag = false;
				for (int num7 = 0; num7 < table2.NValidRecords; num7++)
				{
					Record record5 = table2.Records[num7];
					if (andCheckIntField4 != record5.GetAndCheckIntField(FI.teamplayerlinks_playerid))
					{
						continue;
					}
					int andCheckIntField5 = record5.GetAndCheckIntField(FI.teamplayerlinks_teamid);
					for (int num8 = 0; num8 < num2; num8++)
					{
						if (array[num8].assetid == andCheckIntField5)
						{
							flag = true;
							break;
						}
					}
					break;
				}
				if (!flag)
				{
					continue;
				}
				DateTime birthdate = FifaUtil.ConvertToDate(record4.GetAndCheckIntField(FI.players_birthdate));
				string firstname = null;
				string text = null;
				string text2 = null;
				string text3 = null;
				int num9 = 0;
				for (int num10 = 0; num10 < table7.NValidRecords; num10++)
				{
					Record record6 = table7.Records[num10];
					if (record6.GetAndCheckIntField(FI.cz_players_playerid) == andCheckIntField4)
					{
						num9 = record6.GetAndCheckIntField(FI.cz_players_assetid);
						break;
					}
				}
				Player player = null;
				if (num9 != 0)
				{
					player = (Player)FifaEnvironment.Players.SearchId(num9);
				}
				if (player == null)
				{
					for (int num11 = 0; num11 < table5.NValidRecords; num11++)
					{
						Record record7 = table5.Records[num11];
						if (record7.IntField[FI.editedplayernames_playerid] == andCheckIntField4)
						{
							firstname = record7.GetAndCheckStringField(FI.editedplayernames_firstname);
							text = record7.GetAndCheckStringField(FI.editedplayernames_surname);
							text2 = record7.GetAndCheckStringField(FI.editedplayernames_commonname);
							if (text2.Contains(text))
							{
								text2 = null;
							}
							text3 = record7.GetAndCheckStringField(FI.editedplayernames_playerjerseyname);
							if (text3 != text && text3.Contains(text))
							{
								text3 = text;
							}
							break;
						}
					}
					player = FifaEnvironment.Players.FitPlayer(firstname, text, birthdate);
					if (player == null)
					{
						num4 = FifaEnvironment.Players.GetNextId(num4);
						player = new Player(table6.Records[num6]);
						player.Id = num4;
						player.firstname = firstname;
						player.lastname = text;
						player.commonname = text2;
						player.playerjerseyname = text3;
						FifaEnvironment.Players.InsertId(player);
						player.LinkCountry(FifaEnvironment.Countries);
					}
				}
				array2[num5++] = player;
				player.m_assetid = andCheckIntField4;
			}
			for (int num12 = 0; num12 < table2.NValidRecords; num12++)
			{
				Record record8 = table2.Records[num12];
				int andCheckIntField6 = record8.GetAndCheckIntField(FI.teamplayerlinks_playerid);
				int andCheckIntField7 = record8.GetAndCheckIntField(FI.teamplayerlinks_teamid);
				for (int num13 = 0; num13 < num5; num13++)
				{
					if (array2[num13].m_assetid != andCheckIntField6)
					{
						continue;
					}
					for (int num14 = 0; num14 < num2; num14++)
					{
						if (array[num14].assetid == andCheckIntField7)
						{
							array2[num13].PlayFor(array[num14]);
							TeamPlayer value = new TeamPlayer(record8, array2[num13], array[num14]);
							array[num14].Roster.Add(value);
							if (array[num14].captainid == array2[num13].m_assetid)
							{
								array[num14].PlayerCaptain = array2[num13];
								array[num14].captainid = array2[num13].Id;
							}
							if (array[num14].freekicktakerid == array2[num13].m_assetid)
							{
								array[num14].PlayerFreeKick = array2[num13];
								array[num14].freekicktakerid = array2[num13].Id;
							}
							if (array[num14].leftfreekicktakerid == array2[num13].m_assetid)
							{
								array[num14].PlayerLeftFreeKick = array2[num13];
								array[num14].leftfreekicktakerid = array2[num13].Id;
							}
							if (array[num14].rightfreekicktakerid == array2[num13].m_assetid)
							{
								array[num14].PlayerRightFreeKick = array2[num13];
								array[num14].rightfreekicktakerid = array2[num13].Id;
							}
							if (array[num14].leftcornerkicktakerid == array2[num13].m_assetid)
							{
								array[num14].PlayerLeftCorner = array2[num13];
								array[num14].leftcornerkicktakerid = array2[num13].Id;
							}
							if (array[num14].longkicktakerid == array2[num13].m_assetid)
							{
								array[num14].PlayerLongKick = array2[num13];
								array[num14].longkicktakerid = array2[num13].Id;
							}
							if (array[num14].penaltytakerid == array2[num13].m_assetid)
							{
								array[num14].PlayerPenalty = array2[num13];
								array[num14].penaltytakerid = array2[num13].Id;
							}
							if (array[num14].rightcornerkicktakerid == array2[num13].m_assetid)
							{
								array[num14].PlayerRightCorner = array2[num13];
								array[num14].rightcornerkicktakerid = array2[num13].Id;
							}
							break;
						}
					}
					break;
				}
			}
			for (int num15 = 0; num15 < array2.Length; num15++)
			{
				array2[num15].m_assetid = array2[num15].Id;
			}
			for (int num16 = 0; num16 < array.Length; num16++)
			{
				array[num16].assetid = array[num16].Id;
			}
			if (statusBar != null)
			{
				statusBar.Text = "Update complete";
				statusBar.Owner.Refresh();
			}
		}
	}
}
