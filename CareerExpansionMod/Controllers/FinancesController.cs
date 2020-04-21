using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareerExpansionMod.CEM;
using CareerExpansionMod.CEM.FIFA;
using CareerExpansionMod.CEM.Finances;
using Microsoft.AspNetCore.Mvc;

namespace CareerExpansionMod.Controllers
{
    public class FinancesController : Controller
    {
        IEnumerable<Sponsor> FullListOfSponsors = Sponsor.LoadAll();
        List<SponsorsToTeam> CurrentTeamSponsors = CareerDB1.Current != null && CareerDB1.FIFAUser != null
                                        ? SponsorsToTeam.LoadSponsorsForTeam(CareerDB1.FIFAUser.clubteamid)
                                        : SponsorsToTeam.LoadSponsorsForTeam(1960);

        private void LoadSponsorsIntoViewBag()
        {
            ViewBag.CurrentTeamSponsors = CurrentTeamSponsors;

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Alcohol))
                ViewBag.AlcoholSponsor = CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Alcohol);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Drinks))
                ViewBag.DrinksSponsor = CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Drinks);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Food))
                ViewBag.FoodSponsor = CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Food);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.General))
                ViewBag.GeneralSponsor = CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.General);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Gym))
                ViewBag.GymSponsor = CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Gym);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Hospitality))
                ViewBag.HospitalitySponsor = CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Hospitality);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Kit))
                ViewBag.KitSponsor = CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Kit);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Legal))
                ViewBag.LegalSponsor = CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Legal);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Main))
                ViewBag.MainSponsor = CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Main);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Nutritional))
                ViewBag.NutritionalSponsor = CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Nutritional);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Training))
                ViewBag.TrainingSponsor = CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Training);

            if (CurrentTeamSponsors.Exists(x => x.SponsorType == eSponsorType.Travel))
                ViewBag.TravelSponsor = CurrentTeamSponsors.FirstOrDefault(x => x.SponsorType == eSponsorType.Travel);

        }

        public IActionResult Index()
        {
            LoadSponsorsIntoViewBag();
            return View();
        }

        public IActionResult Prices()
        {
            LoadSponsorsIntoViewBag();
            return View();
        }

        public IActionResult Sponsors()
        {
            LoadSponsorsIntoViewBag();
            
            return View();
        }

        [HttpGet]
        public JsonResult GetStartingBudget()
        {
            return Json(CEMCore.CEMCoreInstance.Finances.StartingBudget);
        }

        [HttpGet]
        public JsonResult GetTransferBudget()
        {
            return Json(CEMCore.CEMCoreInstance.Finances.TransferBudget);
        }

        [HttpGet]
        //[Route("Finance/SetTransferBudget/{amount}")]
        public JsonResult SetTransferBudget(int amount)
        {
            CEMCore.CEMCoreInstance.Finances.TransferBudget = amount;
            return Json(CEMCore.CEMCoreInstance.Finances.TransferBudget);

            //return Json("ERR: 001");
        }

        [HttpGet]
        public JsonResult RequestFunds()
        {
            var success = CEMCore.CEMCoreInstance.Finances.RequestAdditionalFunds(out string return_message);
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

        [HttpPost]
        public JsonResult AcceptSponsor(string type, string sponsorName)
        {
            return Json("");
        }
    }
}