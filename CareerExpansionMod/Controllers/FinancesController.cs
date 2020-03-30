using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using v2k4FIFAModdingCL.MemHack;
using v2k4FIFAModdingCL.MemHack.Core;
using v2k4FIFAModdingCL.MemHack.Career;

namespace CareerExpansionMod.Controllers
{
    public class FinancesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetStartingBudget()
        {
            using (var f = new Finances()) 
            {
                return Json(f.StartingBudget);

            }
        }

        [HttpGet]
        public JsonResult GetTransferBudget()
        {
            using (var f = new Finances())
            {
                return Json(f.TransferBudget);
            }
        }

        [HttpGet]
        //[Route("Finance/SetTransferBudget/{amount}")]
        public JsonResult SetTransferBudget(int amount)
        {
            using (var f = new Finances())
            {
                f.TransferBudget = amount;
                return Json(f.TransferBudget);
            }

            //return Json("ERR: 001");
        }

        [HttpGet]
        public JsonResult RequestFunds()
        {
            using (var f = new Finances())
            {
                var success = f.RequestAdditionalFunds(out string return_message);
                dynamic rJson = new { Success = true, Message = string.Empty };
                //rJson.Success = true;
                //rJson.Message = return_message;
                return Json(rJson);
            }

            //return Json("ERR: 001");
        }
    }
}