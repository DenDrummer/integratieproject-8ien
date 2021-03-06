﻿using IP3_8IEN.BL.Domain.Data;
using IP3_8IEN.BL.Domain.Gebruikers;
using Newtonsoft.Json;
using System.IO;

using IP3_8IEN.DAL;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace IP3_8IEN.BL
{
    public class GebruikerManager : IGebruikerManager
    {
        private UnitOfWorkManager uowManager;
        private IGebruikerRepository repo;
        private IDataManager dataMgr;
        private IDashManager dashMgr;

        // Deze constructor gebruiken we voor operaties binnen de package
        public GebruikerManager()
        {
            //repo = new GebruikerRepository();
        }

        // We roepen deze constructor aan wanneer we met twee repositories gaan werken
        public GebruikerManager(UnitOfWorkManager uofMgr)
        {
            uowManager = uofMgr;
            repo = new GebruikerRepository(uowManager.UnitOfWork);
        }

        // We zoeken een gebruiker op basis van 'Username'
        public Gebruiker FindUser(string username)
        {
            InitNonExistingRepo();

            return repo.ReadGebruikers().FirstOrDefault(x => x.Username == username);
        }
        public void DeleteGebruiker(string username)
        {
            InitNonExistingRepo();
            repo.DeleteGebruiker(repo.ReadGebruikers().FirstOrDefault(x => x.Username == username));

        }

        public IEnumerable<Gebruiker> GetGebruikers()
        {
            InitNonExistingRepo();
            return repo.ReadGebruikers();
        }

        public IEnumerable<Gebruiker> GetGebruikersWithDash()
        {
            InitNonExistingRepo();
            return repo.ReadGebruikersWithDashbord();
        }

        // Hier werken we met 'Unit of Work'
        // omdat we informatie uit de data package nodig hebben
        public void AddAlertInstelling(string filePath)
        {
            InitNonExistingRepo(true);

            //sourceUrl /relatief path
            string json = new StreamReader(filePath).ReadToEnd();
            List<Message> alertConfigs = new List<Message>();

            dynamic alertInstellingen = JsonConvert.DeserializeObject(json);

            //string user = null;
            //bool notificationWeb;
            //bool email;
            //bool mobileNotification;
            //bool state;
            //int onderwerpId;
            //int onderwerpId2;
            //int thresh;
            //bool negatief;


            dataMgr = new DataManager(uowManager);
            //We laten de transactie eve denken dat we geen 'UoW' gebruiken zodat er niet
            //van repo gewisseld wordt bij het aanroepen van een nieuwe methode
            bool UoW = false;
            repo.SetUnitofWork(UoW);

            IEnumerable<Onderwerp> onderwerpen = dataMgr.ReadOnderwerpen();

            foreach (var item in alertInstellingen.records)
            {
                if (item.Threshold != null)
                {
                    ValueFluctuation vf = new ValueFluctuation()
                    {
                        Gebruiker = FindUser((String)item.Username),
                        NotificationWeb = (bool)item.NotificationWeb,
                        Email = (bool)item.Email,
                        MobileNotification = (bool)item.MobileNotification,
                        AlertState = true,
                        Onderwerp = onderwerpen.FirstOrDefault(x => x.OnderwerpId == (int)item.OnderwerpId),
                        ThresholdValue = item.Threshold
                    };
                    repo.AddingAlertInstelling(vf);
                }else if (item.OnderwerpId2 != null) {
                    HogerLager hl = new HogerLager()
                    {
                        Gebruiker = FindUser((String)item.Username),
                        NotificationWeb = (bool)item.NotificationWeb,
                        Email = (bool)item.Email,
                        MobileNotification = (bool)item.MobileNotification,
                        AlertState = true,
                        Onderwerp = onderwerpen.FirstOrDefault(x => x.OnderwerpId == (int)item.OnderwerpId),
                        Onderwerp2 = onderwerpen.FirstOrDefault(x => x.OnderwerpId == (int)item.OnderwerpId2)
                    };
                    repo.AddingAlertInstelling(hl);
                }else
                { 
                    PositiefNegatief pn = new PositiefNegatief()
                    {
                        Gebruiker = FindUser((String)item.Username),
                        NotificationWeb = (bool)item.NotificationWeb,
                        Email = (bool)item.Email,
                        MobileNotification = (bool)item.MobileNotification,
                        AlertState = true,
                        Onderwerp = onderwerpen.FirstOrDefault(x => x.OnderwerpId == (int)item.OnderwerpId),
                        negatief = (bool)item.Negatief
                    };
                    repo.AddingAlertInstelling(pn);
                }


                uowManager.Save();


            }
            //we zetten 'UoW' boolian terug op true
            UoW = true;
            repo.SetUnitofWork(UoW);
        }

        // We initialiseren een 'Alert' met het toewijzen van een 'AlertInstelling' adhv een 'Id' 
        // ook voegen het moment van creatie toe ('CreatedOn')
        // 'AlertContent' kan een string zijn met informatie om te verzenden naar een gebruiker


        public void AddAlert(string alertContent, int alertInstellingId)
        {
            InitNonExistingRepo();

            IEnumerable<AlertInstelling> fluctuations = repo.ReadValueFluctuations();
            List<AlertInstelling> Ais = fluctuations.ToList();
            Ais.AddRange(repo.ReadHogerLagers().ToList());
            Ais.AddRange(repo.ReadPositiefNegatiefs().ToList());

            AlertInstelling ai = Ais.FirstOrDefault(v => v.AlertInstellingId == alertInstellingId);
            Alert alert = new Alert()
            {
                AlertContent = alertContent,
                AlertInstelling = ai,
                CreatedOn = DateTime.Now
            };
            //alert toevoegen aan de ICollection van 'AlertInstelling'
            var alertColl = ai.Alerts;
            if (alertColl != null)
            {
                ai.Alerts = alertColl.ToList();
            }
            else
            {
                ai.Alerts = new Collection<Alert>();
            }

            ai.Alerts.Add(alert);

            //eerst alert creëren zodat deze een PK toegewegen krijgt
            repo.AddingAlert(alert);
            //dan de AlertInstelling updaten met de nieuwe 'Alert'
            repo.UpdateAlertInstelling(ai);
        }


        // Alerts inlezen via json bestand
        public void AddAlerts(string filePath)
        {
            InitNonExistingRepo();

            StreamReader r = new StreamReader(filePath);
            string json = r.ReadToEnd();
            List<Message> alertList = new List<Message>();

            dynamic Alerts = JsonConvert.DeserializeObject(json);

            string alertContent;
            int alertInstellingId;

            foreach (var item in Alerts.records)
            {
                alertContent = item.AlertContent;
                alertInstellingId = item.AlertInstellingId;

                AddAlert(alertContent, alertInstellingId);
            };
        }

        public IEnumerable<Alert> GetAlerts()
        {
            InitNonExistingRepo();
            return repo.ReadAlerts();
        }

        public Alert GetAlert(int alertId)
        {
            InitNonExistingRepo();

            Alert alert = repo.ReadAlert(alertId);
            return alert;
        }

        public void AddGebruiker(string userName, string userId, string naam, string voornaam, string email, DateTime joindate, string role = "User")
        {
            InitNonExistingRepo();
            dashMgr = new DashManager();

            Gebruiker gebruiker = new Gebruiker
            {
                GebruikerId = userId,
                Username = userName,
                Voornaam = voornaam,
                Naam = naam,
                Role = role,
                Email = email,
                Joindate = joindate,
                Active = true
            };
            repo.AddingGebruiker(gebruiker);

            dashMgr = new DashManager();
            //Dashboard initialiseren voor nieuwe gebruiker en opvullen met vaste grafieken
            dashMgr.InitializeDashbordNewUsers(gebruiker.GebruikerId);

        }

        public void UpdateGebruiker(Gebruiker gebruiker)
        {
            InitNonExistingRepo();

            repo.UpdateGebruiker(gebruiker);
        }

        public void DeleteUser(string userId)
        {
            InitNonExistingRepo();

            //IdentityUser wordt verwijderd, data gebruiker wordt overschreven
            Gebruiker user = repo.ReadGebruikers().FirstOrDefault(u => u.GebruikerId == userId);

            user.Username = "Deleted";
            user.Naam = "Deleted";
            user.Voornaam = "Deleted";
            user.Email = "Deleted";
            user.Geboortedatum = DateTime.Now;
            user.Role = "User";
            //We geven aan dat de account 'inactive' is
            user.Active = false;

            UpdateGebruiker(user);
        }

        public IEnumerable<Domain.Dashboard.GraphData> GetUserstatsList()
        {
            //UserCount afgelopen 10 dagen
            InitNonExistingRepo();

            List<Gebruiker> users = repo.ReadGebruikers().ToList();
            DateTime lastJoin = users.OrderBy(m => m.Joindate).ToList().Last().Joindate;
            DateTime stop = users.OrderBy(m => m.Joindate).ToList().Last().Joindate;
            stop.AddDays(-10);


            List<Domain.Dashboard.GraphData> GraphDataList = new List<Domain.Dashboard.GraphData>();
            for (int i = 0; i < 10; i++)
            {
                string date = lastJoin.Date.Year + "-" + lastJoin.Date.Month + "-" + lastJoin.Date.Day;
                int count = 0;
                IEnumerable<Gebruiker> usrs = users.Where(s => s.Joindate.Day == lastJoin.Date.Day).ToList();

                foreach(Gebruiker u in usrs)
                {
                    count++;
                }
                Domain.Dashboard.GraphData graph = new Domain.Dashboard.GraphData(date, count);
                GraphDataList.Add(graph);

                lastJoin = lastJoin.AddDays(-1);
            }
            return GraphDataList;
        }

        public IEnumerable<Gebruiker> GetUsers()
        {
            InitNonExistingRepo();

            return repo.ReadUsers();
        }

        public IEnumerable<ApplicationUser> GetUsersInRoles(IEnumerable<ApplicationUser> appUsers, string role)
        {
            InitNonExistingRepo();

            List<ApplicationUser> appUsersInRole = new List<ApplicationUser>();
            IEnumerable<Gebruiker> users = repo.ReadGebruikers().Where(u => u.Role == role && u.Active == true);
            foreach (Gebruiker user in users)
            {
                appUsersInRole.Add(appUsers.FirstOrDefault(u => u.Id == user.GebruikerId));
            }
            return appUsersInRole;
        }

        public string ExportToCSV(IEnumerable<Gebruiker> gebruikers)
        {
            string json = JsonConvert.SerializeObject(gebruikers, Formatting.Indented);
            return json;
        }

        public IEnumerable<Domain.Dashboard.GraphData> GetTotalUsersList()
        {
            InitNonExistingRepo();
            //enkel actieve gebruikers meegeven
            int count = repo.ReadGebruikers().Where(g => g.Active == true).Count();
            List<Domain.Dashboard.GraphData> cijferlist = new List<Domain.Dashboard.GraphData>();
            Domain.Dashboard.GraphData graph = new Domain.Dashboard.GraphData()
            {
                Label = "Aantal actieve gebruikers",
                Value = count
            };
            cijferlist.Add(graph);
            return cijferlist;
        }

        //Unit of Work related
        public void InitNonExistingRepo(bool withUnitOfWork = false)
        {
            // Als we een repo met UoW willen gebruiken en als er nog geen uowManager bestaat:
            // Dan maken we de uowManager aan en gebruiken we de context daaruit om de repo aan te maken.
            if (withUnitOfWork)
            {
                if (uowManager == null)
                {
                    uowManager = new UnitOfWorkManager();
                }
                repo = new DAL.GebruikerRepository(uowManager.UnitOfWork);
            }
            // Als we niet met UoW willen werken, dan maken we een repo aan als die nog niet bestaat.
            else
            {
                //zien of repo al bestaat
                if (repo == null)
                {
                    repo = new DAL.GebruikerRepository();
                }
                else
                {
                    //checken wat voor repo we hebben
                    bool isUoW = repo.IsUnitofWork();
                    if (isUoW)
                    {
                        repo = new DAL.GebruikerRepository();
                    }
                    else
                    {
                        // repo behoudt zijn context
                    }
                }
            }
        }


        public void GetAlertHogerLagers()
        {
            InitNonExistingRepo();
            dataMgr = new DataManager();

            List<HogerLager> hogerLagers = repo.ReadHogerLagers().ToList();

            foreach (HogerLager hl in hogerLagers)
            {
                //Check if onderwerp is een peroon
                if (hl.Onderwerp is Persoon && hl.Onderwerp2 is Persoon)
                {
                    if (hl.OneHigherThanTwo)
                    {
                        if (CalculateZscore(hl.Onderwerp) < CalculateZscore(hl.Onderwerp2))
                        {
                            Persoon p1 = (Persoon)hl.Onderwerp;
                            Persoon p2 = (Persoon)hl.Onderwerp2;
                            repo.AddingAlert(new Alert()
                            {
                                AlertContent = p2.Naam + "is nu populairder dan " + p1.Naam,
                                AlertInstelling = hl,
                                CreatedOn = DateTime.Now
                            });
                        }
                    }
                    else
                    {
                        if (CalculateZscore(hl.Onderwerp) > CalculateZscore(hl.Onderwerp2))
                        {
                            Persoon p1 = (Persoon)hl.Onderwerp;
                            Persoon p2 = (Persoon)hl.Onderwerp2;
                            repo.AddingAlert(new Alert()
                            {
                                AlertContent = p1.Naam + "is nu populairder dan " + p2.Naam,
                                AlertInstelling = hl,
                                CreatedOn = DateTime.Now
                            });
                        }
                    }
                }
                //als onderwerp een organistatie is
                else
                {
                    if (hl.OneHigherThanTwo)
                    {
                        if (CalculateZscore(hl.Onderwerp) < CalculateZscore(hl.Onderwerp2))
                        {
                            Organisatie o1 = (Organisatie)hl.Onderwerp;
                            Organisatie o2 = (Organisatie)hl.Onderwerp2;
                            repo.AddingAlert(new Alert()
                            {
                                AlertContent = o2.Afkorting + "is nu populairder dan " + o1.Afkorting,
                                AlertInstelling = hl,
                                CreatedOn = DateTime.Now
                            });
                        }
                    }
                    else
                    {
                        if (CalculateZscore(hl.Onderwerp) > CalculateZscore(hl.Onderwerp2))
                        {
                            Organisatie o1 = (Organisatie)hl.Onderwerp;
                            Organisatie o2 = (Organisatie)hl.Onderwerp2;
                            repo.AddingAlert(new Alert()
                            {
                                AlertContent = o2.Afkorting + "is nu populairder dan " + o2.Afkorting,
                                AlertInstelling = hl,
                                CreatedOn = DateTime.Now
                            });
                        }
                    }
                }
            }
        }

        public void GetAlertValueFluctuations()
        {
            InitNonExistingRepo();
            dataMgr = new DataManager();

            List<ValueFluctuation> valueFluctuations = repo.ReadValueFluctuations().ToList();
            List<Message> messages = dataMgr.ReadMessagesWithSubjMsgs().ToList();

            foreach (ValueFluctuation vf in valueFluctuations)
            {
                if (vf.Onderwerp is Persoon)
                {
                    if (messages.Where(m => m.IsFromPersoon((Persoon)vf.Onderwerp) && m.Date.Date == DateTime.Now.Date).Count() > vf.CurrentValue + vf.ThresholdValue)
                    {
                        Persoon p = (Persoon)vf.Onderwerp;
                        repo.AddingAlert(new Alert()
                        {

                            AlertContent = "Thresholdvalue voor " + p.Naam + " is overschreden",
                            AlertInstelling = vf,
                            CreatedOn = DateTime.Now
                        });
                    }
                }
                else
                {
                    if (messages.Where(m => m.IsFromOrganisatie((Organisatie)vf.Onderwerp) && m.Date.Date == DateTime.Now.Date).Count() > vf.CurrentValue + vf.ThresholdValue)
                    {
                        Organisatie o = (Organisatie)vf.Onderwerp;
                        repo.AddingAlert(new Alert()
                        {

                            AlertContent = "Thresholdvalue voor " + o.Afkorting + " is overschreden",
                            AlertInstelling = vf,
                            CreatedOn = DateTime.Now
                        });
                    }
                }
            }
        }

        public void GetAlertPositiefNegatiefs()
        {
            InitNonExistingRepo();
            dataMgr = new DataManager();
            double total = 1;

            List<PositiefNegatief> positiefNegatiefs = repo.ReadPositiefNegatiefs().ToList();
            List<Message> messages = dataMgr.ReadMessagesWithSubjMsgs().ToList();

            foreach (PositiefNegatief pn in positiefNegatiefs)
            {
                if (pn.Onderwerp is Persoon)
                {
                    messages = messages.Where(m => m.IsFromPersoon((Persoon)pn.Onderwerp)).ToList();
                    total = messages.Sum(m => m.Polarity);

                    if (pn.negatief == true)
                    {
                        if (total / messages.Count() > 0)
                        {
                            Persoon p = (Persoon)pn.Onderwerp;
                            repo.AddingAlert(new Alert()
                            {
                                AlertContent = p.Naam + " is nu positief",
                                AlertInstelling = pn,
                                CreatedOn = DateTime.Now
                            });
                        }
                    }
                    else
                    {
                        if (total / messages.Count() < 0)
                        {
                            Persoon p = (Persoon)pn.Onderwerp;
                            repo.AddingAlert(new Alert()
                            {
                                AlertContent = p.Naam + " is nu negatief",
                                AlertInstelling = pn,
                                CreatedOn = DateTime.Now
                            });
                        }
                    }
                }
                else
                {
                    messages = messages.Where(m => m.IsFromOrganisatie((Organisatie)pn.Onderwerp)).ToList();
                    total = messages.Sum(m => m.Polarity);

                    if (pn.negatief == true)
                    {
                        if (total / messages.Count() > 0)
                        {
                            Organisatie o = (Organisatie)pn.Onderwerp;
                            repo.AddingAlert(new Alert()
                            {
                                AlertContent = o.Afkorting + " is nu positief",
                                AlertInstelling = pn,
                                CreatedOn = DateTime.Now
                            });
                        }
                    }
                    else
                    {
                        if (total / messages.Count() < 0)
                        {
                            Organisatie o = (Organisatie)pn.Onderwerp;
                            repo.AddingAlert(new Alert()
                            {
                                AlertContent = o.Afkorting + " is nu negatief",
                                AlertInstelling = pn,
                                CreatedOn = DateTime.Now
                            });
                        }
                    }
                }
            }

        }

        double CalculateZscore(Onderwerp onderwerp)
        {
            InitNonExistingRepo();
            int totaalTweets = 0;
            //totaalTweets = messages.Where(Message => Message.Politician == s).Count();
            bool test;
            List<Message> messages = dataMgr.ReadMessagesWithSubjMsgs().ToList();
            List<Message> ms = new List<Message>();
            List<int> tweetsPerDag = new List<int>();
            double gemiddelde;
            DateTime laatsteTweet = messages.OrderBy(m => m.Date).ToList().Last().Date;

            if (onderwerp is Persoon p)
            {
                foreach (Message m in messages)
                {
                    test = false;
                    foreach (SubjectMessage sm in m.SubjectMessages)
                    {
                        if (sm.Persoon != null && sm.Persoon.Naam == p.Naam)
                        {
                            test = true;
                        }
                    }
                    if (test)
                    {
                        totaalTweets++;
                        ms.Add(m);
                    }
                }

                //Message mm = messages.Where(Message => Message.Politician == s).OrderBy(o=>o.Date).First();
                DateTime start = messages.OrderBy(m => m.Date).ToList().First().Date;
                tweetsPerDag.Clear();
                do
                {
                    tweetsPerDag.Add(ms.Where(m => m.Date.Date.Equals(start.Date)).Count());
                    //tweetsPerDag.Add(messages.Where(Message => Message.Politician == s).Where(Message => Message.Date.Date == start).Count());
                    start = start.AddDays(1);

                } while (start <= laatsteTweet);
                double totaal = 0;
                foreach (int i in tweetsPerDag)
                {
                    totaal = totaal + i;
                }

                gemiddelde = totaal / tweetsPerDag.Count();



                double average = tweetsPerDag.Average();
                double sumOfSquaresOfDifferences = tweetsPerDag.Select(val => (val - average) * (val - average)).Sum();
                double sd = Math.Sqrt(sumOfSquaresOfDifferences / tweetsPerDag.Count());

                return ((tweetsPerDag.Last() - gemiddelde) / sd);
            }
            else
            {
                Organisatie o = (Organisatie)onderwerp;
                foreach (Message m in messages)
                {
                    test = false;
                    foreach (SubjectMessage sm in m.SubjectMessages)
                    {
                        bool test3 = false;
                        foreach (Tewerkstelling t in sm.Persoon.Tewerkstellingen)
                        {
                            if (t.Organisatie.Afkorting == o.Afkorting)
                            {
                                test3 = true;
                            }
                        }
                        if (sm.Persoon != null && test3)
                        {
                            test = true;
                        }
                    }
                    if (test)
                    {
                        totaalTweets++;
                        ms.Add(m);
                    }
                }

                //Message mm = messages.Where(Message => Message.Politician == s).OrderBy(o=>o.Date).First();
                DateTime start = messages.OrderBy(m => m.Date).ToList().First().Date;
                tweetsPerDag.Clear();
                do
                {
                    tweetsPerDag.Add(ms.Where(m => m.Date.Date == start.Date).Count());
                    //tweetsPerDag.Add(messages.Where(Message => Message.Politician == s).Where(Message => Message.Date.Date == start).Count());
                    start = start.AddDays(1);

                } while (start <= laatsteTweet);
                double totaal = 0;
                foreach (int i in tweetsPerDag)
                {
                    totaal += i;
                }

                gemiddelde = totaal / tweetsPerDag.Count();



                double average = tweetsPerDag.Average();
                double sumOfSquaresOfDifferences = tweetsPerDag.Select(val => (val - average) * (val - average)).Sum();
                double sd = Math.Sqrt(sumOfSquaresOfDifferences / tweetsPerDag.Count());

                return ((tweetsPerDag.Last() - gemiddelde) / sd);
            }

        }

        public void WeeklyReview()
        {
            InitNonExistingRepo();
            List<Gebruiker> gebruikers = new List<Gebruiker>();
            gebruikers = repo.ReadGebruikersWithAlertInstellingen().ToList();
            List<Alert> dezeWeek = new List<Alert>();
            StringBuilder sb = new StringBuilder();

            foreach (Gebruiker g in gebruikers)
            {
                sb.Clear();
                sb.Append(@"<img src='https://i.imgur.com/mxv6a2j.png' alt='Smiley face' style='width:620px;display:block;padding: 10px 10px 10px 10px;margin:0 auto;'> <div id=""wrapper"" style=""width:600px;margin:0 auto; border:1px solid black; 
                            overflow:hidden; padding: 10px 10px 10px 10px;"" ><p><i>");
                // Voor- en Achternaam kunnen voorlopig leeg zijn
                //sb.Append(g.Voornaam + " " + g.Naam);
                sb.Append(g.Username);
                sb.Append(@", </i></p>
                            <p>Via de Weekly Review wordt u op de hoogte gehouden van alle trending Onderwerpen die </br>
                            u volgt. Indien u op de hoogte gehouden wilt worden van nog meer onderwerpen, kan u 
                            </br> steeds extra onderwerpen volgen op <a href=""www.8ien.be""> Weekly Reviews </a>. </p>
                            <h3>Personen</h3> <div style=""margin: 0px;""> <p>Naam : Bart De Wever </p> <ul>");
                if (g.AlertInstellingen.Count() != 0) {
                foreach (AlertInstelling al in g.AlertInstellingen)
                {
                    if (al.Alerts != null) {
                    foreach (Alert a in al.Alerts)
                    {
                        if (DatesAreInTheSameWeek(a.CreatedOn, DateTime.Now))
                        {
                            dezeWeek.Add(a);
                                    sb.Append("<li>" +  a.ToString() + "</li>");
                        }
                    }
                    }
                }
                }
                sb.Append(@"</ul></div></div>");
                SendMail(dezeWeek, g.Email, sb.ToString());
            }
        }

        public List<HogerLager> GetHogerLagersByUser()
        {
            InitNonExistingRepo();

            //List<HogerLager> hogerLagers = repo.ReadHogerLagers().ToList();
            //hogerLagers = hogerLagers.Where(hl => hl.Gebruiker == gebruiker).ToList();

            return repo.ReadHogerLagers().ToList();
        }

        public List<ValueFluctuation> GetValueFluctuationsByUser()
        {
            InitNonExistingRepo();

            //List<ValueFluctuation> valueFluctuations = repo.ReadValueFluctuations().ToList();
            //valueFluctuations = valueFluctuations.Where(vf => vf.Gebruiker == gebruiker).ToList();

            return repo.ReadValueFluctuations().ToList();
        }

        public List<PositiefNegatief> GetPositiefNegatiefsByUser()
        {
            InitNonExistingRepo();

            //List<PositiefNegatief> positiefNegatiefs = repo.ReadPositiefNegatiefs().ToList();
            //positiefNegatiefs = positiefNegatiefs.Where(pn => pn.Gebruiker == gebruiker).ToList();

            return repo.ReadPositiefNegatiefs().ToList();
        }

        public List<Alert> GetAlertsByUser(Gebruiker gebruiker)
        {
            InitNonExistingRepo();

            IEnumerable<AlertInstelling> fluctuations = repo.ReadValueFluctuations();
            List<AlertInstelling> ais = fluctuations.ToList();
            ais.AddRange(repo.ReadHogerLagers().ToList());
            ais.AddRange(repo.ReadPositiefNegatiefs().ToList());
            List<Alert> alerts = new List<Alert>();

            foreach (AlertInstelling ai in ais)
            {
                if (ai.Gebruiker == gebruiker && ai.Alerts != null)
                {
                    alerts.AddRange(ai.Alerts);
                }
            }

            return alerts;
        }

        bool DatesAreInTheSameWeek(DateTime date1, DateTime date2)
        {
            var cal = System.Globalization.DateTimeFormatInfo.CurrentInfo.Calendar;
            var d1 = date1.Date.AddDays(-1 * (int)cal.GetDayOfWeek(date1));
            var d2 = date2.Date.AddDays(-1 * (int)cal.GetDayOfWeek(date2));

            return d1.Equals(d2);
        }
        void SendMail(List<Alert> alerts, string email, string body)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("integratieproject.8ien@gmail.com");
                mail.To.Add(email);
                mail.Subject = "Test";
                mail.Body = body;
                mail.IsBodyHtml = true;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("integratieproject.8ien@gmail.com", "integratieproject");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Mail did not send" + ex);
            }
        }

        //SendMail(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
        public void SendMail(string userId, string subject, string body)
        {
            InitNonExistingRepo();
            try
            {
                string userEmail = repo.ReadGebruiker(userId).Email;

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("integratieproject.8ien@gmail.com");
                mail.To.Add(userEmail);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("integratieproject.8ien@gmail.com", "integratieproject");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Mail did not send" + ex);
            }
        }

        public void SendMail(string naam, string email, string tekst, string onderwerp)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress(email);
                mail.To.Add(new MailAddress("integratieproject.8ien@gmail.com"));
                mail.Subject = onderwerp;
                mail.Body = tekst + "</br> Deze mail komt van: </br> " + naam + "</br>" + email;
                mail.IsBodyHtml = true;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("integratieproject.8ien@gmail.com", "integratieproject");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Mail did not send" + ex);
            }
        }

        public string WeeklyReview(Gebruiker g)
        {
            InitNonExistingRepo();
            List<Alert> dezeWeek = new List<Alert>();
            StringBuilder sb = new StringBuilder();

            sb.Clear();
                sb.Append(@"<img src='https://i.imgur.com/mxv6a2j.png' alt='Smiley face' style='width:620px;display:block;padding: 10px 10px 10px 10px;margin:0 auto;'> <div id=""wrapper"" style=""width:600px;margin:0 auto; border:1px solid black; 
                            overflow:hidden; padding: 10px 10px 10px 10px;"" ><p><i>");
                // Voor- en Achternaam kunnen voorlopig leeg zijn
                //sb.Append(g.Voornaam + " " + g.Naam);
                sb.Append(g.Username);
                sb.Append(@", </i></p>
                            <p>Via de Weekly Review wordt u op de hoogte gehouden van alle trending Onderwerpen die </br>
                            u volgt. Indien u op de hoogte gehouden wilt worden van nog meer onderwerpen, kan u 
                            </br> steeds extra alerts toevoegen! </a>. </p>
                            <h3>Personen</h3> <div style=""margin: 0px;""> <p>Naam : Bart De Wever </p> <ul>");
                if (g.AlertInstellingen != null)
                {
                    foreach (AlertInstelling al in g.AlertInstellingen)
                    {
                        if (al.Alerts != null)
                        {
                            foreach (Alert a in al.Alerts)
                            {
                                if (DatesAreInTheSameWeek(a.CreatedOn, DateTime.Now))
                                {
                                    dezeWeek.Add(a);
                                    sb.Append("<li>" + a.ToString() + "</li>");
                                }
                            }
                        }
                    }
                }
                sb.Append(@"</ul></div></div>");
                return sb.ToString();
        }
    }
}