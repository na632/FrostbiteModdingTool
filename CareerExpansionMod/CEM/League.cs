using System;
using System.Collections.Generic;
using System.Text;

namespace v2k4FIFAModdingCL.Career.CME
{
    public class League
    {
        int LeagueId { get; set; }

        string LeagueName { get; set; }
        int LeagueMaxAge { get; set; }
        int LeagueMaxPlayersAboveMaxAge { get; set; }
    }
}
