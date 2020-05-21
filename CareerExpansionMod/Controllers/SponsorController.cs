using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CareerExpansionMod.CEM;
using CareerExpansionMod.CEM.Data;
using CareerExpansionMod.CEM.Finances;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CareerExpansionMod.Controllers
{
    public class SponsorController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            var sponsors = Sponsor.LoadAll();
            return View("ListSponsor", sponsors);
        }

        // GET: Admin/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            return View("CreateSponsor", new Sponsor());
        }

        // POST: Admin/Create
        [HttpPost]
        public ActionResult Create(Sponsor sponsor)
        {
            try
            {
                var form = Request.Form;
                for (var i = 0; i < 10; i++)
                {
                    var item = form["ddlSponsorLevel" + i.ToString()];
                    if (item.Count > 0)
                    {
                        sponsor.SponsorLevels[i] = Convert.ToInt32(item[0]);
                    }
                }

                sponsor.Save(true);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [Route("~/Sponsor/Edit/{name}")]
        public ActionResult Edit(string name)
        {
            var sponsor = Sponsor.Load(name);
            if (sponsor != null)
            {
                return View("EditSponsor", sponsor);
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [Route("~/Sponsor/Edit/{name}")]
        //public ActionResult Edit(string name, IFormCollection collection)
        public ActionResult Edit(string name, Sponsor sponsor)
        {
            try
            {
                var form = Request.Form;
                for (var i = 0; i < 10; i++)
                {
                    var item = form["ddlSponsorLevel" + i.ToString()];
                    if(item.Count > 0)
                    {
                        sponsor.SponsorLevels[i] = Convert.ToInt32(item[0]);
                    }
                }

                sponsor.Save(true);
                return RedirectToActionPermanent("Edit", "Sponsor", new { name = name });
                //return View();
            }
            catch
            {
                return View();
            }
        }

        // GET: Admin/Delete/5
        public ActionResult Delete(string name)
        {
            var sp = Sponsor.Load(name);
            sp.Delete();
            return RedirectToActionPermanent("Index", "Sponsor");
        }

        // POST: Admin/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult ImportFromCSV()
        {




            return View("ImportFromCSV");
        }

        [HttpPost]
        public ActionResult ImportFromCSV(CSVImportSponsor importSponsor)
        {
            var newGuidOfFile = Guid.NewGuid();
            var fileLocation = CEMCore.CEMMyDocumentsDirectory + newGuidOfFile.ToString() + ".csv";
            SponsorsToTeam.Load();

            ViewBag.Error = false;
            ViewBag.ErrorText = "";

            var row = 2;

            try
            {
                if (importSponsor.File != null)
                {
                    // Create File
                    using (var fs = new FileStream(fileLocation, System.IO.FileMode.OpenOrCreate))
                    {
                        importSponsor.File.CopyToAsync(fs).Wait();
                        fs.Flush();
                    }

                    using (var reader = new StreamReader(fileLocation))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        var records = csv.GetRecords<dynamic>();
                        if(records != null)
                        {
                            foreach (var record in records)
                            {
                                var dict = (IDictionary<string, object>)record;
                                if(dict!=null)
                                {
                                    var teamid = Convert.ToInt32(dict["Team ID"].ToString());


                                    // Kit Creator
                                    var kitSponsor = new Sponsor(dict["Kit Manufacturer"].ToString())
                                    {
                                        SponsorImageUrl = dict["Kit Manufacturer Logo"].ToString(),
                                        SponsorPayoutPerYearMax =
                                         !string.IsNullOrEmpty(dict["Kit Money"].ToString())
                                            ? Convert.ToInt32(dict["Kit Money"].ToString()) : 1000000,
                                         SponsorType = eSponsorType.Kit, 
                                         SponsorLevels = new [] { 1,1,1,1,1,1,1,1,1,1 }
                                    };
                                    kitSponsor.Save();
                                    if (!DbDataCache.SponsorsToTeams.Any(x => x.TeamId == teamid 
                                                && x.SponsorType == eSponsorType.Kit
                                        ))
                                    {
                                        var kitSponsorToTeam = new SponsorsToTeam()
                                        {
                                            Confidence = 5,
                                            ContractLengthInYears = 2,
                                            SponsorName = kitSponsor.SponsorName,
                                            PayoutPerYear = SponsorsToTeam.GetCaclulatedPayoutAmountPerYear(teamid, kitSponsor.SponsorName),
                                            SponsorType = eSponsorType.Kit,
                                            TeamId = teamid
                                        };
                                        DbDataCache.SponsorsToTeams.Add(kitSponsorToTeam);
                                    }

                                    // Main Creator
                                    var mainSponsor = new Sponsor(dict["Main Shirt Sponsor"].ToString())
                                    {
                                        SponsorImageUrl = dict["Main Shirt Sponsor Logo"].ToString(),
                                        SponsorPayoutPerYearMax =
                                         !string.IsNullOrEmpty(dict["Main Shirt Sponsor Money"].ToString())
                                            ? Convert.ToInt32(dict["Main Shirt Sponsor Money"].ToString()) : 1000000,
                                        SponsorType = eSponsorType.Main,
                                        SponsorLevels = new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
                                    };
                                    mainSponsor.Save();
                                    if (!DbDataCache.SponsorsToTeams.Any(x => x.TeamId == teamid
                                                && x.SponsorType == eSponsorType.Main
                                        ))
                                    {
                                        var mainSponsorToTeam = new SponsorsToTeam()
                                        {
                                            Confidence = 5,
                                            ContractLengthInYears = 2,
                                            SponsorName = mainSponsor.SponsorName,
                                            PayoutPerYear = SponsorsToTeam.GetCaclulatedPayoutAmountPerYear(teamid, mainSponsor.SponsorName),
                                            SponsorType = eSponsorType.Main,
                                            TeamId = teamid
                                        };
                                        DbDataCache.SponsorsToTeams.Add(mainSponsorToTeam);
                                    }

                                    if (!string.IsNullOrEmpty(dict["Sleeve Sponsor"].ToString()))
                                    {
                                        // Kit Sleeve Creator
                                        var sleeveSponsor = new Sponsor(dict["Sleeve Sponsor"].ToString())
                                        {
                                            SponsorImageUrl = dict["Sleeve Sponsor Logo"].ToString(),
                                            SponsorPayoutPerYearMax =
                                             !string.IsNullOrEmpty(dict["Sleeve Sponsor Money"].ToString())
                                                ? Convert.ToInt32(dict["Sleeve Sponsor Money"].ToString()) : 1000000,
                                            SponsorType = eSponsorType.Kit_Sleeve,
                                            SponsorLevels = new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
                                        };
                                        sleeveSponsor.Save();
                                        if (!DbDataCache.SponsorsToTeams.Any(x => x.TeamId == teamid
                                                    && x.SponsorType == eSponsorType.Kit_Sleeve

                                            ))
                                        {
                                            var sleeveSponsorToTeam = new SponsorsToTeam()
                                            {
                                                Confidence = 5,
                                                ContractLengthInYears = 2,
                                                SponsorName = sleeveSponsor.SponsorName,
                                                PayoutPerYear = SponsorsToTeam.GetCaclulatedPayoutAmountPerYear(teamid, sleeveSponsor.SponsorName),
                                                SponsorType = eSponsorType.Kit_Sleeve,
                                                TeamId = teamid
                                            };
                                            DbDataCache.SponsorsToTeams.Add(sleeveSponsorToTeam);
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(dict["Stadium Sponsor"].ToString()))
                                    {
                                        // Stadium / Training Ground Sponsor
                                        var stadiumSponsor = new Sponsor(dict["Stadium Sponsor"].ToString())
                                        {
                                            SponsorImageUrl = dict["Stadium Sponsor Logo"].ToString(),
                                            SponsorPayoutPerYearMax =
                                         !string.IsNullOrEmpty(dict["Stadium Sponsor Money"].ToString())
                                            ? Convert.ToInt32(dict["Stadium Sponsor Money"].ToString()) : 1000000,
                                            SponsorType = eSponsorType.Training,
                                            SponsorLevels = new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
                                        };
                                        stadiumSponsor.Save();
                                        if (!DbDataCache.SponsorsToTeams.Any(x => x.TeamId == teamid
                                                    && x.SponsorType == eSponsorType.Kit_Sleeve
                                            ))
                                        {
                                            var stadiumSponsorToTeam = new SponsorsToTeam()
                                            {
                                                Confidence = 5,
                                                ContractLengthInYears = 2,
                                                SponsorName = stadiumSponsor.SponsorName,
                                                PayoutPerYear = SponsorsToTeam.GetCaclulatedPayoutAmountPerYear(teamid, stadiumSponsor.SponsorName),
                                                SponsorType = eSponsorType.Training,
                                                TeamId = teamid
                                            };
                                            DbDataCache.SponsorsToTeams.Add(stadiumSponsorToTeam);
                                        }
                                    }
                                    SponsorsToTeam.SaveAll();
                                    row++;
                                }

                            }
                        }
                    }

                    // Delete File
                    if (System.IO.File.Exists(fileLocation))
                            System.IO.File.Delete(fileLocation);
                }
            }
            catch(KeyNotFoundException knfe)
            {
                ViewBag.Error = true;
                ViewBag.ErrorText = knfe.Message + ". Have you mispelt the header or got a space?";
            }
            catch (FormatException fex)
            {
                ViewBag.Error = true;
                ViewBag.ErrorText = "{Row:" + row.ToString() + "} - " + fex.Message + " This is caused by text being in a numeric field, for example a Money field. ";
            }
            catch (Exception ex)
            {
                ViewBag.Error = true;
                ViewBag.ErrorText = ex.Message;
            }
            finally
            {
                if (System.IO.File.Exists(fileLocation))
                    System.IO.File.Delete(fileLocation);
            }


            return View("ImportFromCSV");
        }
    }
}