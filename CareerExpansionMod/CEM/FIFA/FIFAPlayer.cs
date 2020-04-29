﻿using CareerExpansionMod.CEM;
using CareerExpansionMod.CEM.FIFA;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace v2k4FIFAModding.Career.CME.FIFA
{
    public enum PersonalityTypes
    {
Selfish = 1,
     Individual = 2,
         Normal = 3,
              [Display(Name = "Team Player")]
              [Description("Team Player")]
              TeamPlayer = 4,
        [Display(Name = "Ultimate Professional")]
              [Description("Ultimate Professional")]
                   UltimateProfessional = 5
    }

    public enum EmotionalTypes
    {
        [Description("Very Calm")]
        [Display(Name = "Very Calm")]
        VeryCalm = 1,
        Calm = 2,
        Normal = 3,
        [Description("Hot Temper")]
        [Display(Name = "Hot Temper")]
        HotTemper = 4,
        Volcano = 5
    }
    public class FIFAPlayer
    {
        public int firstnameid { get; set; }
        public int lastnameid { get; set; }
        public int playerjerseynameid { get; set; }
        public int commonnameid { get; set; }
        public int skintypecode { get; set; }
        public int trait2 { get; set; }
        public int bodytypecode { get; set; }
        public int haircolorcode { get; set; }
        public int facialhairtypecode { get; set; }
        public int curve { get; set; }
        public int jerseystylecode { get; set; }
        public int agility { get; set; }
        public int tattooback { get; set; }
        public int accessorycode4 { get; set; }
        public int gksavetype { get; set; }
        public int positioning { get; set; }
        public int tattooleftarm { get; set; }
        public int hairtypecode { get; set; }
        public int standingtackle { get; set; }
        public int preferredposition3 { get; set; }
        public int longpassing { get; set; }
        public int penalties { get; set; }
        public int animfreekickstartposcode { get; set; }
        public int animpenaltieskickstylecode { get; set; }
        public int isretiring { get; set; }
        public int longshots { get; set; }
        public int gkdiving { get; set; }
        public int interceptions { get; set; }
        public int shoecolorcode2 { get; set; }
        public int crossing { get; set; }
        public int potential { get; set; }
        public int gkreflexes { get; set; }
        public int finishingcode1 { get; set; }
        public int reactions { get; set; }
        public int composure { get; set; }
        public int vision { get; set; }
        public int contractvaliduntil { get; set; }
        public int animpenaltiesapproachcode { get; set; }
        public int finishing { get; set; }
        public int dribbling { get; set; }
        public int slidingtackle { get; set; }
        public int accessorycode3 { get; set; }
        public int accessorycolourcode1 { get; set; }
        public int headtypecode { get; set; }
        public int sprintspeed { get; set; }
        public int height { get; set; }
        public int hasseasonaljersey { get; set; }
        public int tattoohead { get; set; }
        public int preferredposition2 { get; set; }
        public int strength { get; set; }
        public int shoetypecode { get; set; }
        public int birthdate { get; set; }
        public int preferredposition1 { get; set; }
        public int tattooleftleg { get; set; }
        public int ballcontrol { get; set; }
        public int shotpower { get; set; }
        public int trait1 { get; set; }
        public int socklengthcode { get; set; }
        public int weight { get; set; }
        public int hashighqualityhead { get; set; }
        public int gkglovetypecode { get; set; }
        public int tattoorightarm { get; set; }
        public int balance { get; set; }
        public int gender { get; set; }
        public int headassetid { get; set; }
        public int gkkicking { get; set; }
        public int internationalrep { get; set; }
        public int animpenaltiesmotionstylecode { get; set; }
        public int shortpassing { get; set; }
        public int freekickaccuracy { get; set; }
        public int skillmoves { get; set; }
        public int faceposerpreset { get; set; }
        public int usercaneditname { get; set; }
        public int avatarpomid { get; set; }
        public int attackingworkrate { get; set; }
        public int finishingcode2 { get; set; }
        public int aggression { get; set; }
        public int acceleration { get; set; }
        public int headingaccuracy { get; set; }
        public int iscustomized { get; set; }
        public int eyebrowcode { get; set; }
        public int runningcode2 { get; set; }
        public int modifier { get; set; }
        public int gkhandling { get; set; }
        public int eyecolorcode { get; set; }
        public int jerseysleevelengthcode { get; set; }
        public int accessorycolourcode3 { get; set; }
        public int accessorycode1 { get; set; }
        public int playerjointeamdate { get; set; }
        public int headclasscode { get; set; }
        public int defensiveworkrate { get; set; }
        public int tattoofront { get; set; }
        public int nationality { get; set; }
        public int preferredfoot { get; set; }
        public int sideburnscode { get; set; }
        public int weakfootabilitytypecode { get; set; }
        public int jumping { get; set; }
        /// <summary>
        /// Personality of the player - How professional they are
        /// 1 - Selfish
        /// 2 - Individual
        /// 3 - Balanced
        /// 4 - Team Player / Exceptional Group Player
        /// 5 - Ultimate Professional / Team Leader
        /// </summary>
        public int personality { get; set; }

        public PersonalityTypes PersonalityType
        {
            get
            {
                return (PersonalityTypes)personality;
            }
        }


        public int gkkickstyle { get; set; }
        public int stamina { get; set; }
        public int playerid { get; set; }
        public int marking { get; set; }
        public int accessorycolourcode4 { get; set; }
        public int gkpositioning { get; set; }
        public int headvariation { get; set; }
        public int skillmoveslikelihood { get; set; }
        public int skintonecode { get; set; }
        public int shortstyle { get; set; }
        public int overallrating { get; set; }
        public int smallsidedshoetypecode { get; set; }

        /// <summary>
        /// How emotional a player is 
        /// 1 - 
        /// 2 - 
        /// 3 - Balanced
        /// 4 - Somewhat emotional
        /// 5 - Highly Emotional (Volcano)
        /// </summary>
        public int emotion { get; set; }

        public EmotionalTypes EmotionalType
        {
            get
            {
                return (EmotionalTypes)emotion;
            }
        }
        public int runstylecode { get; set; }
        public int jerseyfit { get; set; }
        public int accessorycode2 { get; set; }
        public int shoedesigncode { get; set; }
        public int shoecolorcode1 { get; set; }
        public int hairstylecode { get; set; }
        public int animpenaltiesstartposcode { get; set; }
        public int runningcode1 { get; set; }
        public int preferredposition4 { get; set; }
        public int volleys { get; set; }
        public int accessorycolourcode2 { get; set; }
        public int tattoorightleg { get; set; }
        public int facialhaircolorcode { get; set; }


        public string Name
        {
            get
            {
                return FIFAPlayerName.GetNameFromFIFAPlayer(this);
            }
        }

        /// Custom Additions
        /// 

        public DateTime DateJoined
        {
            get
            {
                return CEMUtilities.FIFACoreDateTime.AddDays(playerjointeamdate);
            }
        }

        public DateTime DateOfBirth
        {
            get
            {
                return CEMUtilities.FIFACoreDateTime.AddDays(birthdate);
            }
        }

        public int TimeAtClubInYears
        {
            get
            {
                var gameDate = CEMCore.CEMCoreInstance.CoreHack.GameDate;
                if (gameDate.HasValue)
                {
                    int TimeAtClubInYears = Convert.ToInt32(Math.Ceiling((gameDate - DateJoined).Value.TotalDays / 365));
                    return TimeAtClubInYears > 0 ? TimeAtClubInYears : 1;
                }
                return 1;
            }
        }

        public int AgeInYears
        {
            get
            {
                var gameDate = CEMCore.CEMCoreInstance.CoreHack.GameDate;
                if (gameDate.HasValue)
                {
                    int TimeAtClubInYears = Convert.ToInt32(Math.Ceiling((gameDate - this.DateOfBirth).Value.TotalDays / 365));
                    return TimeAtClubInYears > 0 ? TimeAtClubInYears : 1;
                }
                return 18;
            }
        }


    }

}