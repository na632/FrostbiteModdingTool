using CareerExpansionMod.CEM;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CareerExpansionMod.CEM.FIFA;
using System.Security.Cryptography;

namespace v2k4FIFAModding.Career.CME.FIFA
{
    public class FIFATeam
    {
        public int assetid { get; set; }
        public int teamcolor1g { get; set; }
        public int teamcolor1r { get; set; }
        public int clubworth { get; set; }
        public int teamcolor2b { get; set; }
        public int teamcolor2r { get; set; }
        public int foundationyear { get; set; }
        public int teamcolor3r { get; set; }
        public int teamcolor1b { get; set; }
        public int opponentweakthreshold { get; set; }
        public int latitude { get; set; }
        public int teamcolor3g { get; set; }
        public int opponentstrongthreshold { get; set; }
        public int ballid { get; set; }
        public int teamcolor2g { get; set; }
        public string teamname { get; set; }
        public int teamcolor3b { get; set; }
        public int powid { get; set; }
        public int defensivestyle { get; set; }
        public int rightfreekicktakerid { get; set; }
        public int flamethrowercannon { get; set; }
        public int domesticprestige { get; set; }
        public int genericint2 { get; set; }
        public int defensivedepth { get; set; }
        public int hasvikingclap { get; set; }
        public int jerseytype { get; set; }
        public int popularity { get; set; }
        public int hastifo { get; set; }
        public int teamstadiumcapacity { get; set; }
        public int iscompetitionscarfenabled { get; set; }
        public int cityid { get; set; }
        public int defensivewidth { get; set; }
        public int rivalteam { get; set; }
        public int isbannerenabled { get; set; }
        public int midfieldrating { get; set; }
        public int matchdayoverallrating { get; set; }
        public int matchdaymidfieldrating { get; set; }
        public int attackrating { get; set; }
        public int playersinboxcorner { get; set; }
        public int longitude { get; set; }
        public int matchdaydefenserating { get; set; }
        public int hasstandingcrowd { get; set; }
        public int favoriteteamsheetid { get; set; }
        public int defenserating { get; set; }
        public int iscompetitionpoleflagenabled { get; set; }
        public int skinnyflags { get; set; }
        public int uefa_consecutive_wins { get; set; }
        public int longkicktakerid { get; set; }
        public int bodytypeid { get; set; }
        public int trait1vweak { get; set; }
        public int iscompetitioncrowdcardsenabled { get; set; }
        public int rightcornerkicktakerid { get; set; }
        public int suitvariationid { get; set; }
        public int uefa_cl_wins { get; set; }
        public int domesticcups { get; set; }
        public int ethnicity { get; set; }
        public int leftcornerkicktakerid { get; set; }
        public int youthdevelopment { get; set; }
        public int teamid { get; set; }
        public int uefa_el_wins { get; set; }
        public int trait1vequal { get; set; }
        public int suittypeid { get; set; }
        public int numtransfersin { get; set; }
        public int playersinboxfk { get; set; }
        public int stanchionflamethrower { get; set; }
        public int captainid { get; set; }
        public int offensivestyle { get; set; }
        public int personalityid { get; set; }
        public int playersinboxcross { get; set; }
        public int prev_el_champ { get; set; }
        public int leftfreekicktakerid { get; set; }
        public int leaguetitles { get; set; }
        public int genericbanner { get; set; }
        public int transferbudget { get; set; }
        public int overallrating { get; set; }
        public int offensivewidth { get; set; }
        public int profitability { get; set; }
        public int utcoffset { get; set; }
        public int penaltytakerid { get; set; }
        public int freekicktakerid { get; set; }
        public int crowdskintonecode { get; set; }
        public int internationalprestige { get; set; }
        public int trainingstadium { get; set; }
        public int form { get; set; }
        public int genericint1 { get; set; }
        public int trait1vstrong { get; set; }
        public int matchdayattackrating { get; set; }

        public List<FIFAPlayer> GetPlayers()
        {
            List<FIFAPlayer> players = new List<FIFAPlayer>();

            var tplinks = CareerDB2.Current.teamplayerlinks.Where(x => x["teamid"].ToString() == CareerDB1.FIFAUser.clubteamid.ToString());
            var ps = (from tpl in tplinks
                      join p in CareerDB2.Current.players on tpl["playerid"].ToString() equals p["playerid"].ToString()
                      select p) ;
            
            foreach(var i in ps)
            {

                var pl = CareerDB2.Current.players.FirstOrDefault(x => x["playerid"].ToString() == i["playerid"].ToString());
                if(pl != null)
                {
                    players.Add(
                    CEMUtilities.CreateItemFromRow<FIFAPlayer>(pl)
                    );
                }


            }
           //var plays = CareerDB2.Current.players.Where(x => tplinks.Any(y => y["playerid"].ToString() == x["playerid"].ToString()));

            //for (var i = 0; i < ps.Count(); i++)
            //{
            //    players.Add(
            //    CEMUtilities.CreateItemFromRow<FIFAPlayer>(ps.ElementAt(i))
            //    );
            //}

            return players;
        }

        public static double InfluenceCalculation(FIFAPlayer p)
        {
           

            var leadership = (6 - p.emotion) + p.personality;

            var dateJoined = CEMUtilities.FIFACoreDateTime.AddDays(p.playerjointeamdate);

            leadership += Convert.ToInt32(Math.Ceiling(p.TimeAtClubInYears > 2 ? p.TimeAtClubInYears * 0.75 : p.TimeAtClubInYears));

            leadership += Convert.ToInt32(Math.Ceiling(p.AgeInYears * 0.1));

            return leadership;
        }

        public EmotionalTypes EmotionalTypeOfTeam
        {
            get
            {
                var avgEmotion = GetPlayers().Average(x => x.emotion);
                var t = (EmotionalTypes)Math.Round(avgEmotion);

                return t;
            }

        }

        public PersonalityTypes PersonalityTypeOfTeam 
        {
            get
            {
                var avg = GetPlayers().Average(x => x.personality);
                var t = (PersonalityTypes)Math.Round(avg);

                return t;
            }
        }

        public List<FIFAPlayer> GetTeamLeaders()
        {
            List<FIFAPlayer> players = GetPlayers().OrderByDescending(x => InfluenceCalculation(x)).Take(3).ToList();

            return players;
        }

        public List<FIFAPlayer> GetTeamInfluences()
        {
            List<FIFAPlayer> players = GetPlayers().OrderByDescending(x => InfluenceCalculation(x)).Skip(3).Take(4).ToList();

            return players;
        }

        public List<FIFAPlayer> GetTeamTroubleMakers()
        {
             List<FIFAPlayer> players = GetPlayers().OrderBy(x => InfluenceCalculation(x)).Take(3).ToList();


            return players;
        }
    }
}
