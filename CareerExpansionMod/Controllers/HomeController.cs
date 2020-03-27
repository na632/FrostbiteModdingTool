using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CareerExpansionMod.Models;

namespace CareerExpansionMod.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [Route("~/IsCareerModeActive")]
        public JsonResult IsCareerModeActive()
        {
            if(Startup.FIFAProcess != null)
            {
                var coreHack = new v2k4FIFAModdingCL.MemHack.Core.CoreHack();
                if(!coreHack.GameDate.HasValue)
                {
                    return new JsonResult($"Career Mode is running");
                }
                else
                    return new JsonResult($"{Startup.FIFAProcess.ProcessName} is running");
            }
            return new JsonResult("FIFA hasn't been started");
        }
    }
}
