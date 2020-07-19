using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

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
            dynamic returnData = new { };
            return Json(returnData);
        }

        [Route("~/Transfers/ManualSearchByName/{id}")]
        public JsonResult ManualSearchByName(string name)
        {
            dynamic returnData = new { };
            return Json(returnData);
        }
    }
}