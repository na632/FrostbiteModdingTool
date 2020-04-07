using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareerExpansionMod.CME;
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
    }
}