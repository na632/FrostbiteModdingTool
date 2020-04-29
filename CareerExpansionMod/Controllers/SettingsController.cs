using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareerExpansionMod.CEM;
using Microsoft.AspNetCore.Mvc;

namespace CareerExpansionMod.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            CEMCoreSettings settings = new CEMCoreSettings();
            settings.Load();
            return View("Index", settings);
        }

        [HttpPost]
        public RedirectToActionResult Edit(CEMCoreSettings settings)
        {
            settings.Save();
            return new RedirectToActionResult("Index", "Settings", null);
        }
    }
}