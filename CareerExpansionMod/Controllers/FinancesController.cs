using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareerExpansionMod.CME;
using CareerExpansionMod.CME.FIFA;
using CareerExpansionMod.CME.Finances;
using Microsoft.AspNetCore.Mvc;

namespace CareerExpansionMod.Controllers
{
    public class FinancesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Prices()
        {
            return View();
        }

        public IActionResult Sponsors()
        {
            // TODO: Clean Up
            // THIS IS VERY HACKY BUT I DONT CARE

            var FullListOfSponsors = Sponsor.LoadAll();
            var CurrentTeamSponsors = CareerDB1.Current != null && CareerDB1.FIFAUser != null
                                            ? SponsorsToTeam.LoadSponsorsForTeam(CareerDB1.FIFAUser.clubteamid)
                                            : SponsorsToTeam.LoadSponsorsForTeam(1960);
            ViewBag.CurrentTeamSponsors = CurrentTeamSponsors;

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Alcohol))
                ViewBag.AlcoholSponsor = Sponsor.Load(CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Alcohol).SponsorName);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Drinks))
                ViewBag.DrinksSponsor = Sponsor.Load(CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Drinks).SponsorName);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Food))
                ViewBag.FoodSponsor = Sponsor.Load(CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Food).SponsorName);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.General))
                ViewBag.GeneralSponsor = Sponsor.Load(CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.General).SponsorName);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Gym))
                ViewBag.GymSponsor = Sponsor.Load(CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Gym).SponsorName);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Hospitality))
                ViewBag.HospitalitySponsor = Sponsor.Load(CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Hospitality).SponsorName);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Kit))
                ViewBag.KitSponsor = Sponsor.Load(CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Kit).SponsorName);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Legal))
                ViewBag.LegalSponsor = Sponsor.Load(CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Legal).SponsorName);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Main))
                ViewBag.MainSponsor = Sponsor.Load(CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Main).SponsorName);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Nutritional))
                ViewBag.NutritionalSponsor = Sponsor.Load(CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Nutritional).SponsorName);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Training))
                ViewBag.TrainingSponsor = Sponsor.Load(CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Training).SponsorName);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Travel))
                ViewBag.TravelSponsor = Sponsor.Load(CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Travel).SponsorName);

            return View();
        }

        [HttpGet]
        public JsonResult GetStartingBudget()
        {
            return Json(CMECore.CMECoreInstance.Finances.StartingBudget);
        }

        [HttpGet]
        public JsonResult GetTransferBudget()
        {
            return Json(CMECore.CMECoreInstance.Finances.TransferBudget);
        }

        [HttpGet]
        //[Route("Finance/SetTransferBudget/{amount}")]
        public JsonResult SetTransferBudget(int amount)
        {
            CMECore.CMECoreInstance.Finances.TransferBudget = amount;
            return Json(CMECore.CMECoreInstance.Finances.TransferBudget);

            //return Json("ERR: 001");
        }

        [HttpGet]
        public JsonResult RequestFunds()
        {
            var success = CMECore.CMECoreInstance.Finances.RequestAdditionalFunds(out string return_message);
            dynamic rJson = new { Success = true, Message = string.Empty };
            //rJson.Success = true;
            //rJson.Message = return_message;
            return Json(rJson);

            //return Json("ERR: 001");
        }

        [HttpGet]
        public JsonResult GetSponsorOptions(int type)
        {
            if (type <= -1)
                return Json(Sponsor.LoadAll());
            else
                return Json(Sponsor.LoadAll().Where(x => (int)x.SponsorType == type));
        }
    }
}