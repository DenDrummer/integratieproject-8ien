﻿using System.Web.Mvc;
using IP_8IEN.BL;
using System.IO;
using System.Web;
using IP_8IEN.BL.Domain.Data;
using System.Collections.Generic;
using System.Threading;
using System.Web.Hosting;
using System.Threading.Tasks;

namespace MVC_S.Controllers
{
    public class HomeController : Controller
    {
        private IDataManager dMgr;
        private IGebruikerManager gMgr;

        public HomeController()
        {
            // Hier wordt voorlopig wat testdata doorgegeven aan de 'Managers'
            // Let op: telkens de 'HomeController() aangesproken wordt worden er methodes uitgevoerd
            dMgr = new DataManager();
            gMgr = new GebruikerManager();

            //dMgr.AddPersonen(Path.Combine(HttpRuntime.AppDomainAppPath, "politici.Json"));
            //dMgr.ApiRequestToJson();
            //dMgr.CountSubjMsgsPersoon();
            dMgr.ReadOnderwerpenWithSubjMsgs();

            /*
            dMgr.AddMessages(Path.Combine(HttpRuntime.AppDomainAppPath, "textgaintest2.json"));

            //gMgr.AddGebruikers(Path.Combine(HttpRuntime.AppDomainAppPath, "AddGebruikersInit.Json"));
            //gMgr.AddAlertInstelling(Path.Combine(HttpRuntime.AppDomainAppPath, "AddAlertInstelling.json"));
            //gMgr.AddAlerts(Path.Combine(HttpRuntime.AppDomainAppPath, "AddAlerts.json"));

            //dMgr.GetAlerts();

            HostingEnvironment.QueueBackgroundWorkItem(ct => SendMailAsync(dMgr));

            gMgr.AddGebruikers(Path.Combine(HttpRuntime.AppDomainAppPath, "AddGebruikersInit.Json"));
            gMgr.AddAlertInstelling(Path.Combine(HttpRuntime.AppDomainAppPath, "AddAlertInstelling.json"));
            gMgr.AddAlerts(Path.Combine(HttpRuntime.AppDomainAppPath, "AddAlerts.json"));*/
        }
        private async Task SendMailAsync(IDataManager dMgr)
        {
            while (true)
            {
                await Task.Run(() =>
                {
                    dMgr.GetAlerts();
                });
                Thread.Sleep(10800000);
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Dashboard()
        {
            
            return View();
        }

        public ActionResult Personen()
        {
            Organisatie nva = new Organisatie()
            {
                NaamOrganisatie = "N-VA",
                Tewerkstellingen = new List<Tewerkstelling>()
            };
            Persoon bart = new Persoon()
            {
                Naam = "Bart De Wever",
                Twitter = "@Bart_DeWever",
                Tewerkstellingen = new List<Tewerkstelling>(),

            };
            Tewerkstelling nvaBart = new Tewerkstelling()
            {
                Organisatie = nva,
                Persoon = bart
            };
            nva.Tewerkstellingen.Add(nvaBart);
            bart.Tewerkstellingen.Add(nvaBart);
            return View(bart);
        }

        public ActionResult Themas()
        {

            return View();
        }

        public ActionResult Organisatie()
        {

            return View();
        }

        public ActionResult Alerts()
        {

            return View();
        }

        public ActionResult WeeklyReview()
        {

            return View();
        }

        public ActionResult AdminCRUD()
        {

            return View();
        }

        public ActionResult AdminOmgeving()
        {

            return View();
        }

        public ActionResult Superadmin()
        {

            return View();
        }

        public ActionResult Instellingen()
        {

            return View();
        }

        public ActionResult Zoeken()
        {

            return View();
        }
    }
}