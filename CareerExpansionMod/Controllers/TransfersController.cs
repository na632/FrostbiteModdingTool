using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using v2k4FIFAModding.Career.CME.FIFA;

namespace CareerExpansionMod.Controllers
{
    public class TransfersController : Controller
    {
        public IActionResult Index()
        {
            return View("TransfersDashboard");
        }

        public IActionResult TransfersDashboard()
        {
            return View();
        }

        public IActionResult ManualTransfer()
        {
            return View();
        }

        [Route("~/Transfers/ManualSearchById/{id}")]
        public JsonResult ManualSearchById(string id)
        {
            return Json(FIFAPlayer.GetPlayersById(int.Parse(id)));
        }

        [Route("~/Transfers/ManualSearchByName/{id}")]
        public JsonResult ManualSearchByName(string name)
        {
            return Json(FIFAPlayer.GetPlayersByName(name));
        }


        [Route("~/Transfers/GetLeagues/")]
        public JsonResult GetLeagues()
        {
            if (CareerDB2.Current != null)
            {
                var l = CareerDB2.Current.leagues.Select(x=> new { leagueid = x["leagueid"], leaguename = x["leaguename"] }).ToList();
                return Json(l);
            }
            return Json(null);
        }

        [Route("~/Transfers/GetTeamsForLeague/{id}")]
        public JsonResult GetTeamsForLeague(string id)
        {
            return Json("");
        }
    }
}