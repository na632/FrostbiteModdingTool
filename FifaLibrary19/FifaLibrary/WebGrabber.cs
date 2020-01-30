using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace FifaLibrary
{
	public class WebGrabber
	{
		public enum EWebSiteDomain
		{
			None,
			Transfermrkt,
			Soccerway
		}

		private DataTable m_WebTable = new DataTable("PlayerWebData");

		private bool m_CanExtractWebTeam;

		private bool m_CanExtractWebPlayer;

		private EWebSiteDomain m_WebSiteDomain;

		private Bitmap m_PlayerPicture;

		public DataTable WebTable => m_WebTable;

		public bool CanExtractWebTeam => m_CanExtractWebTeam;

		public bool CanExtractWebPlayer => m_CanExtractWebPlayer;

		public EWebSiteDomain WebSiteDomain => m_WebSiteDomain;

		public Bitmap Picture => m_PlayerPicture;

		public WebGrabber()
		{
			m_WebTable.Columns.Add("type");
			m_WebTable.Columns.Add("id");
			m_WebTable.Columns.Add("name");
			m_WebTable.Columns.Add("firstname");
			m_WebTable.Columns.Add("lastname");
			m_WebTable.Columns.Add("commonname");
			m_WebTable.Columns.Add("country");
			m_WebTable.Columns.Add("birthdate");
			m_WebTable.Columns.Add("age");
			m_WebTable.Columns.Add("role");
			m_WebTable.Columns.Add("height");
			m_WebTable.Columns.Add("weight");
			m_WebTable.Columns.Add("foot");
			m_WebTable.Columns.Add("team");
			m_WebTable.Columns.Add("number");
			m_WebTable.Columns.Add("since");
			m_WebTable.Columns.Add("contract");
			m_WebTable.Columns.Add("previousteam");
			m_WebTable.Columns.Add("loanedfrom");
			m_WebTable.Columns.Add("loanenddate");
			m_WebTable.Columns.Add("marketvalue");
			m_WebTable.Columns.Add("stadium");
			m_WebTable.Columns.Add("seats");
			m_WebTable.Columns.Add("managername");
			m_WebTable.Columns.Add("managersurname");
		}

		public bool Sync(string webDocumentTitle)
		{
			bool result = false;
			bool flag = webDocumentTitle.Contains("(Detailed view)") && webDocumentTitle.Contains("Detailed squad");
			bool flag2 = webDocumentTitle.Contains("Results, fixtures, squad, statistics, photos, videos and news - Soccerway");
			if ((flag | flag2) != m_CanExtractWebTeam)
			{
				m_CanExtractWebTeam = (flag | flag2);
				if (flag)
				{
					m_WebSiteDomain = EWebSiteDomain.Transfermrkt;
				}
				if (flag2)
				{
					m_WebSiteDomain = EWebSiteDomain.Soccerway;
				}
				result = true;
			}
			bool flag3 = webDocumentTitle.Contains("Profile with news, career statistics and history - Soccerway");
			bool flag4 = webDocumentTitle.Contains("Player Profile");
			if ((flag3 | flag4) != m_CanExtractWebPlayer)
			{
				m_CanExtractWebPlayer = (flag3 | flag4);
				if (flag3)
				{
					m_WebSiteDomain = EWebSiteDomain.Soccerway;
				}
				if (flag4)
				{
					m_WebSiteDomain = EWebSiteDomain.Transfermrkt;
				}
				result = true;
			}
			return result;
		}

		public string ExtractTeamNameFromWebTitle(string webDocumentTitle)
		{
			int num = webDocumentTitle.LastIndexOf('-');
			string result = string.Empty;
			if (num >= 3)
			{
				result = webDocumentTitle.Substring(0, num - 1);
			}
			return result;
		}

		public string ExtractPlayerNameFromWebTitle(string webDocumentTitle)
		{
			int num = webDocumentTitle.LastIndexOf('-');
			string result = string.Empty;
			if (num >= 3)
			{
				result = webDocumentTitle.Substring(0, num - 1);
			}
			return result;
		}

		public bool ExtractInfoFromWeb(HtmlDocument webPage)
		{
			if (m_CanExtractWebPlayer)
			{
				return ExtractPlayerInfoFromWeb(webPage);
			}
			if (m_CanExtractWebTeam)
			{
				return ExtractRosterInfoFromWeb(webPage);
			}
			return false;
		}

		public bool ExtractRosterInfoFromWeb(HtmlDocument webPage)
		{
			EWebSiteDomain webSiteDomain = m_WebSiteDomain;
			bool result = false;
			switch (webSiteDomain)
			{
			case EWebSiteDomain.Soccerway:
				result = ExtractRosterInfoFromSoccerway(webPage);
				break;
			case EWebSiteDomain.Transfermrkt:
				result = ExtractRosterInfoFromTransfermrkt(webPage);
				break;
			}
			return result;
		}

		private bool ExtractRosterInfoFromSoccerway(HtmlDocument webPage)
		{
			return false;
		}

		private void SplitPlayerName(string nameSurname, DataRow webDataRow)
		{
			int num = nameSurname.IndexOf(' ');
			string text = null;
			string text2 = null;
			if (num < 0 || webDataRow["country"].ToString().Contains("Korea"))
			{
				webDataRow["commonname"] = nameSurname;
				return;
			}
			int num2 = nameSurname.LastIndexOf(' ');
			if (num2 == num)
			{
				text = (string)(webDataRow["firstname"] = nameSurname.Substring(0, num));
				text2 = (string)(webDataRow["lastname"] = nameSurname.Substring(num + 1));
			}
			else if (nameSurname.Substring(num, num2 - num + 1).ToLower() == " da " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " das " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " la " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " le " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " de " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " del " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " di " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " ten " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " van der " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " van de " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " van " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " st. " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " el " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " al " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " de la " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " mac " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " mc " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " von " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " ben ")
			{
				text = (string)(webDataRow["firstname"] = nameSurname.Substring(0, num));
				text2 = (string)(webDataRow["lastname"] = nameSurname.Substring(num + 1));
			}
			else
			{
				text = (string)(webDataRow["firstname"] = nameSurname.Substring(0, num2));
				text2 = (string)(webDataRow["lastname"] = nameSurname.Substring(num2 + 1));
			}
		}

		public bool ExtractRosterInfoFromTransfermrkt(HtmlDocument webPage)
		{
			HtmlElementCollection elementsByTagName = webPage.GetElementsByTagName("span");
			m_WebTable.Rows.Clear();
			DataRow dataRow = m_WebTable.NewRow();
			string text = ExtractTeamNameFromWebTitle(webPage.Title);
			if (m_PlayerPicture != null)
			{
				m_PlayerPicture.Dispose();
				m_PlayerPicture = null;
			}
			dataRow["name"] = text;
			dataRow["type"] = "Team";
			Team team = FifaEnvironment.Teams.MatchByname(text);
			if (team != null)
			{
				dataRow["id"] = team.Id.ToString();
			}
			else
			{
				int newId = FifaEnvironment.Teams.GetNewId();
				if (newId != -1)
				{
					dataRow["id"] = newId.ToString();
				}
			}
			int num = FifaEnvironment.Players.GetNewId();
			for (int i = 0; i < elementsByTagName.Count; i++)
			{
				HtmlElement htmlElement = elementsByTagName[i];
				if (htmlElement.OuterText == null || !htmlElement.OuterText.Contains("Stadium"))
				{
					continue;
				}
				if (elementsByTagName[i + 1].Children.Count > 0)
				{
					if (elementsByTagName[i + 1].Children.Count > 0)
					{
						dataRow["stadium"] = elementsByTagName[i + 1].Children[0].OuterText;
					}
					if (elementsByTagName[i + 1].Children.Count > 1)
					{
						dataRow["seats"] = elementsByTagName[i + 1].Children[1].OuterText;
					}
				}
				break;
			}
			m_WebTable.Rows.Add(dataRow);
			Bitmap bitmap = SearchImageContaining(webPage, "wappen/head");
			int width = bitmap.Width * 256 / bitmap.Height;
			m_PlayerPicture = GraphicUtil.ResizeBitmap(bitmap, width, 256, InterpolationMode.HighQualityBicubic);
			m_PlayerPicture = GraphicUtil.CanvasSizeBitmap(m_PlayerPicture, 256, 256);
			elementsByTagName = webPage.GetElementsByTagName("table");
			foreach (HtmlElement item in elementsByTagName)
			{
				if (item.OuterText.StartsWith("\r\n#\r\n\r\nPlayer(s)\r\n\r\nborn/age\r\n\r\nNat.\r\n\r\nHeight\r\n\r\nFoot\r\n\r\nIn the team since\r\n\r\nbefore\r\n\r\nContract until\r\n\r\nMarket value"))
				{
					if (item.Children.Count < 2)
					{
						return false;
					}
					HtmlElement htmlElement3 = item.Children[1];
					int count = htmlElement3.Children.Count;
					_ = new string[count, 12];
					for (int j = 0; j < count; j++)
					{
						HtmlElement htmlElement4 = htmlElement3.Children[j];
						string outerText = htmlElement4.Children[0].OuterText;
						string empty = string.Empty;
						HtmlElementCollection elementsByTagName2 = htmlElement4.Children[1].GetElementsByTagName("A");
						int count2 = elementsByTagName2.Count;
						empty = elementsByTagName2[count2 - 2].OuterText;
						string value = string.Empty;
						string value2 = string.Empty;
						int num2;
						string innerHtml;
						if (elementsByTagName2[0].InnerHtml.Contains("Loaned from: "))
						{
							innerHtml = elementsByTagName2[0].InnerHtml;
							num2 = innerHtml.IndexOf("Loaned from: ");
							innerHtml = innerHtml.Substring(num2 + 13);
							num2 = innerHtml.IndexOf(';');
							innerHtml = innerHtml.Substring(0, num2);
							value = innerHtml;
							innerHtml = elementsByTagName2[0].InnerHtml;
							num2 = innerHtml.IndexOf("return on: ");
							innerHtml = innerHtml.Substring(num2 + 11);
							num2 = innerHtml.IndexOf('"');
							innerHtml = innerHtml.Substring(0, num2);
							value2 = innerHtml;
						}
						else
						{
							elementsByTagName2[0].InnerHtml.Contains("Joined from: ");
						}
						elementsByTagName2 = htmlElement4.Children[1].GetElementsByTagName("TD");
						int count3 = elementsByTagName2.Count;
						string outerText2 = elementsByTagName2[count3 - 1].OuterText;
						string empty2 = string.Empty;
						innerHtml = htmlElement4.Children[2].OuterText;
						num2 = innerHtml.IndexOf('(');
						empty2 = innerHtml.Substring(0, num2 - 1);
						string value3 = innerHtml.Substring(num2 + 1, 2);
						innerHtml = htmlElement4.Children[3].InnerHtml;
						string value4 = string.Empty;
						int num3 = innerHtml.IndexOf('"');
						if (num3 >= 0)
						{
							innerHtml = innerHtml.Substring(num3 + 1);
							int num4 = innerHtml.IndexOf('"');
							if (num4 >= 1)
							{
								value4 = innerHtml.Substring(0, num4);
							}
						}
						string outerText3 = htmlElement4.Children[4].OuterText;
						outerText3 = outerText3.Replace(".", string.Empty);
						outerText3 = outerText3.Replace(",", string.Empty);
						outerText3 = outerText3.Replace(" ", string.Empty);
						outerText3 = outerText3.Replace("m", string.Empty);
						string outerText4 = htmlElement4.Children[5].OuterText;
						string outerText5 = htmlElement4.Children[6].OuterText;
						string value5 = string.Empty;
						innerHtml = htmlElement4.Children[7].InnerHtml;
						if (innerHtml != null)
						{
							num3 = innerHtml.IndexOf("alt=");
							if (num3 >= 0)
							{
								innerHtml = innerHtml.Substring(num3 + 5);
								int num5 = innerHtml.IndexOf('"');
								if (num5 >= 1)
								{
									value5 = innerHtml.Substring(0, num5);
								}
							}
						}
						string outerText6 = htmlElement4.Children[8].OuterText;
						string outerText7 = htmlElement4.Children[9].OuterText;
						dataRow = m_WebTable.NewRow();
						dataRow["name"] = empty;
						dataRow["type"] = "Player";
						dataRow["birthdate"] = empty2;
						dataRow["age"] = value3;
						DateTime birthdate = FifaUtil.ConvertToDate(empty2);
						dataRow["country"] = value4;
						dataRow["role"] = outerText2;
						dataRow["height"] = outerText3;
						dataRow["foot"] = outerText4;
						dataRow["number"] = outerText;
						dataRow["team"] = text;
						dataRow["since"] = outerText5;
						dataRow["contract"] = outerText6;
						dataRow["previousteam"] = value5;
						dataRow["loanedfrom"] = value;
						dataRow["loanenddate"] = value2;
						dataRow["marketvalue"] = outerText7;
						SplitPlayerName(empty, dataRow);
						string commonName = dataRow["commonname"].ToString();
						string firstName = dataRow["firstname"].ToString();
						string lastName = dataRow["lastname"].ToString();
						Player player = FifaEnvironment.Players.MatchPlayerByNameBirthday(ref firstName, ref lastName, ref commonName, birthdate);
						dataRow["commonname"] = commonName;
						dataRow["firstname"] = firstName;
						dataRow["lastname"] = lastName;
						if (player != null)
						{
							dataRow["id"] = player.Id.ToString();
						}
						else
						{
							dataRow["id"] = num.ToString();
							num = FifaEnvironment.Players.GetNextId(num + 1);
						}
						m_WebTable.Rows.Add(dataRow);
					}
					return true;
				}
			}
			return false;
		}

		public bool ExtractPlayerInfoFromTransfermrkt(HtmlDocument webPage)
		{
			HtmlElementCollection elementsByTagName = webPage.GetElementsByTagName("table");
			string text = null;
			string text2 = null;
			string text3 = null;
			DateTime birthdate = default(DateTime);
			bool flag = false;
			string text4 = ExtractPlayerNameFromWebTitle(webPage.Title);
			m_WebTable.Rows.Clear();
			DataRow dataRow = m_WebTable.NewRow();
			dataRow["name"] = text4;
			dataRow["type"] = "Player";
			for (int i = 0; i < elementsByTagName.Count; i++)
			{
				HtmlElement htmlElement = elementsByTagName[i];
				if (!htmlElement.Children[0].OuterText.Contains("Date of birth:"))
				{
					continue;
				}
				flag = true;
				HtmlElement htmlElement2 = htmlElement.Children[0];
				for (int j = 0; j < htmlElement2.Children.Count; j++)
				{
					string outerText = htmlElement2.Children[j].Children[0].OuterText;
					string outerText2 = htmlElement2.Children[j].Children[1].OuterText;
					outerText2 = outerText2.Replace("\t", "");
					outerText2 = outerText2.Trim();
					switch (outerText)
					{
					case "Age:":
						dataRow["age"] = outerText2;
						break;
					case "Weight ":
						dataRow["weight"] = outerText2;
						break;
					case "Contract until:":
						dataRow["contract"] = outerText2;
						break;
					case "Nationality:":
					{
						int num = outerText2.IndexOf('\r');
						if (num >= 0)
						{
							outerText2 = outerText2.Substring(0, num);
						}
						dataRow["country"] = outerText2;
						break;
					}
					case "Last name ":
						dataRow["lastname"] = outerText2;
						text2 = outerText2;
						break;
					case "Height:":
						outerText2 = outerText2.Replace(".", string.Empty);
						outerText2 = outerText2.Replace(",", string.Empty);
						outerText2 = outerText2.Replace(" ", string.Empty);
						outerText2 = (string)(dataRow["height"] = outerText2.Replace("m", string.Empty));
						break;
					case "Foot:":
						dataRow["foot"] = outerText2;
						break;
					case "In the team since:":
						dataRow["since"] = outerText2;
						break;
					case "First name ":
						dataRow["firstname"] = outerText2;
						text = outerText2;
						break;
					case "Date of birth:":
						dataRow["birthdate"] = outerText2;
						birthdate = FifaUtil.ConvertToDate(outerText2);
						break;
					case "Position:":
						dataRow["role"] = outerText2;
						break;
					}
				}
			}
			if (flag)
			{
				SplitPlayerName(text4, dataRow);
				text3 = dataRow["commonname"].ToString();
				text = dataRow["firstname"].ToString();
				text2 = dataRow["lastname"].ToString();
				Player player = FifaEnvironment.Players.MatchPlayerByNameBirthday(ref text, ref text2, ref text3, birthdate);
				dataRow["commonname"] = text3;
				dataRow["firstname"] = text;
				dataRow["lastname"] = text2;
				if (player != null)
				{
					dataRow["id"] = player.Id.ToString();
				}
				else
				{
					dataRow["id"] = FifaEnvironment.Players.GetNewId().ToString();
				}
				m_WebTable.Rows.Add(dataRow);
				Bitmap bitmap = SearchImageContaining(webPage, "portrait", "spielerfotos");
				if (bitmap != null)
				{
					int width = bitmap.Width * 128 / bitmap.Height;
					m_PlayerPicture = GraphicUtil.ResizeBitmap(bitmap, width, 128, InterpolationMode.HighQualityBicubic);
					m_PlayerPicture = GraphicUtil.CanvasSizeBitmap(m_PlayerPicture, 128, 128);
				}
				else
				{
					m_PlayerPicture = null;
				}
				return true;
			}
			return false;
		}

		private Bitmap SearchImageContaining(HtmlDocument webPage, string caption1)
		{
			return SearchImageContaining(webPage, caption1, "default");
		}

		private Bitmap SearchImageContaining(HtmlDocument webPage, string caption1, string caption2)
		{
			for (int i = 0; i < webPage.Images.Count; i++)
			{
				string attribute = webPage.Images[i].GetAttribute("src");
				if (attribute != null && (attribute.Contains(caption1) || attribute.Contains(caption2)) && !attribute.Contains("default"))
				{
					Uri url = new Uri(attribute);
					return (Bitmap)DownloadImage(url);
				}
			}
			return null;
		}

		public Image DownloadImage(Uri url)
		{
			return Image.FromStream(new MemoryStream(new WebClient().DownloadData(url)));
		}

		public bool ExtractPlayerInfoFromSoccerway(HtmlDocument webPage)
		{
			HtmlElementCollection elementsByTagName = webPage.GetElementsByTagName("dl");
			if (elementsByTagName.Count == 1)
			{
				HtmlElement htmlElement = elementsByTagName[0];
				m_WebTable.Rows.Clear();
				DataRow dataRow = m_WebTable.NewRow();
				string firstName = null;
				string lastName = null;
				string text = null;
				DateTime birthdate = default(DateTime);
				for (int i = 0; i < htmlElement.Children.Count; i += 2)
				{
					string outerText = htmlElement.Children[i].OuterText;
					string outerText2 = htmlElement.Children[i + 1].OuterText;
					switch (outerText)
					{
					case "Age ":
						outerText2.Trim();
						dataRow["age"] = outerText2;
						break;
					case "Weight ":
						outerText2.Replace("kg", "").Trim();
						dataRow["weight"] = outerText2;
						break;
					case "Height ":
					{
						string text4 = outerText2.Replace("cm", "");
						text4 = (string)(dataRow["height"] = text4.Trim());
						break;
					}
					case "Nationality ":
						dataRow["country"] = outerText2.Trim();
						break;
					case "Last name ":
						lastName = (string)(dataRow["lastname"] = outerText2.Trim());
						break;
					case "First name ":
						firstName = (string)(dataRow["firstname"] = outerText2.Trim());
						break;
					case "Date of birth ":
						dataRow["birthdate"] = outerText2.Trim();
						birthdate = FifaUtil.ConvertToDate(outerText2);
						break;
					case "Position ":
						dataRow["role"] = outerText2.Trim();
						break;
					case "Foot ":
						dataRow["foot"] = outerText2.Trim();
						break;
					}
				}
				dataRow["name"] = firstName + " " + lastName;
				dataRow["type"] = "Player";
				text = dataRow["commonname"].ToString();
				Player player = FifaEnvironment.Players.MatchPlayerByNameBirthday(ref firstName, ref lastName, ref text, birthdate);
				dataRow["commonname"] = text;
				dataRow["firstname"] = firstName;
				dataRow["lastname"] = lastName;
				if (player != null)
				{
					dataRow["id"] = player.Id.ToString();
				}
				else
				{
					dataRow["id"] = FifaEnvironment.Players.GetNewId().ToString();
				}
				m_WebTable.Rows.Add(dataRow);
				m_PlayerPicture = SearchImageContaining(webPage, "150x150");
				m_PlayerPicture = GraphicUtil.MakeAutoTransparent(m_PlayerPicture);
				m_PlayerPicture = GraphicUtil.ResizeBitmap(m_PlayerPicture, 128, 128, InterpolationMode.HighQualityBicubic);
				return true;
			}
			return false;
		}

		public bool ExtractPlayerInfoFromWeb(HtmlDocument webPage)
		{
			EWebSiteDomain webSiteDomain = m_WebSiteDomain;
			if (webSiteDomain != EWebSiteDomain.Transfermrkt)
			{
				return webSiteDomain == EWebSiteDomain.Soccerway && ExtractPlayerInfoFromSoccerway(webPage);
			}
			return ExtractPlayerInfoFromTransfermrkt(webPage);
		}
	}
}
