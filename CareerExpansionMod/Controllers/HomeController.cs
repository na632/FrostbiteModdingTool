using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CareerExpansionMod.Models;
using CareerExpansionMod.CEM;

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
                if(coreHack.GameDate.HasValue)
                {
                    return new JsonResult($"Career Mode is running");
                }
                else
                    return new JsonResult($"{Startup.FIFAProcess.ProcessName} is running");
            }
            return new JsonResult("FIFA hasn't been started: ERR:001");
        }

        [HttpGet]
        [Route("~/GetCareerModeSaveName")]
        public JsonResult GetCareerModeSaveName()
        {
            if (Startup.FIFAProcess != null)
            {
                var coreHack = new v2k4FIFAModdingCL.MemHack.Core.CoreHack();
                if (!string.IsNullOrEmpty(coreHack.SaveName))
                {
                    return new JsonResult($"{coreHack.SaveName}");
                }
                return new JsonResult("Career Mode Name Invalid Value: ERR:001");
            }
            return new JsonResult("Career Mode Name Invalid Value: ERR:002");
        }

        [HttpGet]
        [Route("~/GetCareerModeSaveFileName")]
        public JsonResult GetCareerModeSaveFileName()
        {
            if (Startup.FIFAProcess != null)
            {
                var coreHack = new v2k4FIFAModdingCL.MemHack.Core.CoreHack();
                if (!string.IsNullOrEmpty(coreHack.SaveFileName))
                {
                    return new JsonResult($"{coreHack.SaveFileName}");
                }
                return new JsonResult("Career Mode file name Invalid Value: ERR:001");
            }
            return new JsonResult("Career Mode file name Invalid Value: ERR:002");
        }

        [HttpGet]
        [Route("~/GetCareerModeGameDate")]
        public JsonResult GetCareerModeGameDate()
        {
            if (Startup.FIFAProcess != null)
            {
                var coreHack = new v2k4FIFAModdingCL.MemHack.Core.CoreHack();
                if (coreHack.GameDate.HasValue)
                {
                    return new JsonResult($"{coreHack.GameDate.Value.ToShortDateString()}");
                }
                return new JsonResult("Career Mode Game Date Invalid Value: ERR:001");
            }
            return new JsonResult("Career Mode Game Date Invalid Value: ERR:002");
        }


        [HttpGet]
        [Route("~/IsBGLoading")]
        public JsonResult IsBGLoading()
        {
            dynamic rJson = new { BGLoading = v2k4FIFAModdingCL.MemHack.Core.CoreHack.IsBGLoading, BGLoadingWhat = string.Empty };

            if (Startup.FIFAProcess != null)
            {
                
            }
            return new JsonResult(rJson);
        }

        [HttpGet]
        [Route("~/IsCareerFileLoadComplete")]
        public JsonResult IsCareerFileLoadComplete()
        {
            return new JsonResult(CEMCore.SetupCareerFileComplete);
        }

        [HttpGet]
        [Route("~/GetMailNews")]
        public JsonResult GetMailNews()
        {
            List<CEMNews> news = new List<CEMNews>();
            news.Add(new CEMNews() { Headline = "Phase 1 is out now!", Content = "Hi all, Welcome to the early Phase 1 test of CEM" });
            return new JsonResult(news);
        }
    }
}
