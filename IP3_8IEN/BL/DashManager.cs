﻿using IP_8IEN.BL.Domain.Dashboard;
using IP3_8IEN.BL.Domain.Dashboard;
using IP3_8IEN.BL.Domain.Data;
using IP3_8IEN.BL.Domain.Gebruikers;
using IP3_8IEN.DAL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace IP3_8IEN.BL
{
    public class DashManager : IDashManager
    {
        private DataManager dataMgr;
        private GebruikerManager gebruikerMgr;
        private UnitOfWorkManager uowManager;
        private DashRepository repo;

        // Deze constructor gebruiken we voor operaties binnen de package
        public DashManager()
        {

        }
        // We roepen deze constructor aan wanneer we met twee repositories gaan werken
        public DashManager(UnitOfWorkManager uowMgr)
        {
            uowManager = uowMgr;
            repo = new DashRepository(uowManager.UnitOfWork);
        }

        public Dashbord GetDashboard(Gebruiker user)
        {
            InitNonExistingRepo();
            
            return repo.ReadDashbord(user);
        }

        public Dashbord GetDashboard(int dashId)
        {
            InitNonExistingRepo();
            
            return repo.ReadDashbord(dashId);
        }

        public Dashbord GetDashboardWithFollows(Gebruiker user)
        {
            InitNonExistingRepo();
            
            return repo.ReadDashbordWithFollows(user);
        }

        public DashItem CreateDashitem(bool adminGraph, string type, string naam = "usergraph")
        {
            InitNonExistingRepo();

            DashItem dashItem = new DashItem() {
                LastModified = DateTime.Now,
                Type = type,
                Naam = naam,
                Active = true
            };

            if (adminGraph)
            {
                dashItem.AdminGraph = true;
            }
            else
            {
                dashItem.AdminGraph = false;
            }

            repo.AddDashItem(dashItem);

            return dashItem;
        }

        public Follow CreateFollow(int dashId, int id)
        {
            InitNonExistingRepo(true);

            dataMgr = new DataManager(uowManager);
            bool UoW = false;
            repo.SetUnitofWork(UoW);

            try
            {
                Follow follow = new Follow()
                {
                    DashItem = repo.ReadDashItem(dashId),
                    Onderwerp = dataMgr.GetPersoon(id)
                };

                if(follow.Onderwerp != null)
                {
                    repo.AddFollow(follow);

                    uowManager.Save();
                    UoW = true;
                    repo.SetUnitofWork(UoW);

                    return follow;
                }                
            }
            catch { }
            try {
                Follow follow = new Follow()
                {
                    DashItem = repo.ReadDashItem(dashId),
                    Onderwerp = dataMgr.GetOrganisatie(id)
                };

                if (follow.Onderwerp != null)
                {
                    repo.AddFollow(follow);

                    uowManager.Save();
                    UoW = true;
                    repo.SetUnitofWork(UoW);

                    return follow;
                }
            } catch { }
            try
            {
                Follow follow = new Follow()
                {
                    DashItem = repo.ReadDashItem(dashId),
                    Onderwerp = dataMgr.GetThema(id)
                };
                if (follow.Onderwerp != null)
                {
                    repo.AddFollow(follow);

                    uowManager.Save();
                    UoW = true;
                    repo.SetUnitofWork(UoW);

                    return follow;
                }
            } catch { }

            throw new Exception("Follow werd niet correct geïnitialiseerd");
        }

        public List<Follow> CreateFollow(int dashId, List<int> listPersoonId)
        {
            InitNonExistingRepo(true);
            
            List<Follow> follows = new List<Follow>();

            dataMgr = new DataManager(uowManager);
            bool UoW = false;
            repo.SetUnitofWork(UoW);
            
            List<Persoon> listPersonen = new List<Persoon>();

            foreach(int persoonId in listPersoonId)
            {
                Follow follow = new Follow()
                {
                    DashItem = repo.ReadDashItem(dashId),
                    Onderwerp = dataMgr.GetPersonen().FirstOrDefault(p => p.OnderwerpId == persoonId)
                };
                follows.Add(follow);
                repo.AddFollow(follow);
            }
            uowManager.Save();

            UoW = true;
            repo.SetUnitofWork(UoW);

            return follows;
        }

        public DashItem SetupDashItem(/*DashItem dashItem,*/ Gebruiker user, Follow follow)
        {
            InitNonExistingRepo(true);

            bool UoW = false;
            repo.SetUnitofWork(UoW);

            follow.DashItem.Follows = new Collection<Follow>
            {
                follow
            };

            Dashbord dashbord = GetDashboard(user);

            TileZone tile = new TileZone()
            {
                Dashbord = dashbord,
                DashItem = follow.DashItem
            };

            repo.AddTileZone(tile);
            follow.DashItem.TileZones.Add(tile);
            repo.UpdateFollow(follow);
            //repo.UpdateDashItem(dashItem);
            AddOneZonesOrder(dashbord);
            uowManager.Save();
            UoW = true;
            repo.SetUnitofWork(UoW);

            return follow.DashItem;
        }

        public DashItem SetupDashItem(Gebruiker user, List<Follow> follows)
        {
            InitNonExistingRepo(true);

            bool UoW = false;
            repo.SetUnitofWork(UoW);

            TileZone tile = new TileZone()
            {
                Dashbord = GetDashboard(user),
                DashItem = follows[0].DashItem
            };
            uowManager.Save();

            foreach (Follow follow in follows)
            {
                repo.AddTileZone(tile);
                follow.DashItem.TileZones.Add(tile);
            }

            uowManager.Save();
            UoW = true;
            repo.SetUnitofWork(UoW);

            return follows[0].DashItem;
        }

        public void LinkGraphsToUser(List<GraphData> graphDataList, int dashId)
        {
            InitNonExistingRepo();

            DashItem dashItem = repo.ReadDashItem(dashId);
            dashItem.Graphdata = new Collection<GraphData>();

            foreach (GraphData graph in graphDataList)
            {
                dashItem.Graphdata.Add(graph);
                graph.DashItem = dashItem;
                repo.UpdateGraphData(graph);
            }

            //UpdateDashItem(dashItem);
        }

        public void AddGraph(GraphData graph)
        {
            InitNonExistingRepo();

            repo.AddGraph(graph);
        }



        public void UpdateDashItem(DashItem dashItem) => repo.UpdateDashItem(dashItem);

        public IEnumerable<Follow> GetFollows(bool admin = false)
        {
            InitNonExistingRepo();
            IEnumerable<Follow> follows = repo.ReadFollows();

            if (admin)
            {
                //enkel vaste grafieken teruggeven
                follows = follows.Where(a => a.DashItem.AdminGraph == true);
                return follows;
            }
            else { return follows; }
        }

        public struct GraphdataValues
        {
            public double[] values;
            public string[] labels;
        }

        public GraphdataValues GetGraphDataRank(int aantal, int interval_uren, bool puntNotatie = false)
        {
            InitNonExistingRepo();

            dataMgr = new DataManager();

            List<GraphData> graphs = dataMgr.GetRankingList(aantal, interval_uren, puntNotatie);

            GraphdataValues graphsVals = new GraphdataValues()
            {
                values = new double[aantal],
                labels = new string[interval_uren]
            };

            int i = 0;
            foreach (GraphData graph in graphs)
            {
                graphsVals.values[i] = graph.Value;
                graphsVals.labels[i] = graph.Label;
                i++;
            }

            return graphsVals;
        }

        public GraphdataValues GetGraphData(int onderwerpId, int aantalDagen, string type, string objType)
        {
            InitNonExistingRepo();

            dataMgr = new DataManager();

            List<GraphData> graphs = new List<GraphData>();

                if (objType == "Persoon")
                {
                    graphs = dataMgr.GetTweetsPerDagList(dataMgr.GetPersoon(onderwerpId), aantalDagen);
                }
                else if (objType == "Organisatie")
                {
                    graphs = dataMgr.GetTweetsPerDagList(dataMgr.GetOrganisatie(onderwerpId), aantalDagen);
                }
                else if (objType == "Thema")
                {
                    graphs = dataMgr.GetTweetsPerDagList(dataMgr.GetThema(onderwerpId), aantalDagen);
                }

            GraphdataValues graphsVals = new GraphdataValues()
            {
                values = new double[aantalDagen+1],
                labels = new string[aantalDagen+1]
            };

            int i = 0;
            foreach(GraphData graph in graphs)
            {
                graphsVals.values[i] = graph.Value;
                graphsVals.labels[i] = graph.Label;
                i++;
            }

            return graphsVals;
        }

        public Dashbord UpdateDashboard(Dashbord dashbord)
        {
            InitNonExistingRepo(true);
            dataMgr = new DataManager(uowManager);
            repo.SetUnitofWork(false);

            DateTime timeNow = DateTime.Now;
            foreach (TileZone tileZone in dashbord.TileZones)
            {
                double hours = (timeNow - tileZone.DashItem.LastModified).TotalHours;
                if (hours > 0.0001)
                {
                    int i = 0;
                    //deze array verwijst naar de personen in GraphData
                    int[] onderwerpId = { 0, 0, 0, 0, 0 };

                    foreach (Follow follow in tileZone.DashItem.Follows)
                    {
                            onderwerpId[i] = follow.Onderwerp.OnderwerpId;
                            i++;
                    }

                    try
                    {
                        GraphdataValues graphs = GetGraphData(onderwerpId[0], 10, tileZone.DashItem.Type, "Persoon");

                        int j = 0;
                        foreach (var graph in tileZone.DashItem.Graphdata)
                        {
                            graph.Label = graphs.labels[j];
                            graph.Value = graphs.values[j];
                            repo.UpdateGraphData(graph);
                            j++;
                        }
                    }
                    catch { }
                    try
                    {
                        //Organisatie organisatie = dataMgr.GetOrganisatie(onderwerpId[0]);
                        GraphdataValues graphs = GetGraphData(onderwerpId[0], 10, tileZone.DashItem.Type, "Organisatie");

                        int j = 0;
                        foreach (var graph in tileZone.DashItem.Graphdata)
                        {
                            graph.Label = graphs.labels[j];
                            graph.Value = graphs.values[j];
                            repo.UpdateGraphData(graph);
                            j++;
                        }
                    } catch { }
                    try
                    {
                        //Thema thema = dataMgr.GetThema(onderwerpId[0]);
                        GraphdataValues graphs = GetGraphData(onderwerpId[0], 10, tileZone.DashItem.Type, "Thema");
                        
                        int j = 0;
                        foreach (var graph in tileZone.DashItem.Graphdata)
                        {
                            graph.Label = graphs.labels[j];
                            graph.Value = graphs.values[j];
                            repo.UpdateGraphData(graph);
                            j++;
                        }
                    } catch { }

                uowManager.Save();
                }
                //LastModified updaten
                tileZone.DashItem.LastModified = timeNow;
                repo.UpdateTileZone(tileZone);
                uowManager.Save();
            }
            repo.SetUnitofWork(true);
            return dashbord;
        }

        public IEnumerable<DashItem> GetDashItems()
        {
            InitNonExistingRepo();

            //enkel 'DashItems' die niet zijn verwijderd teruggeven
            return repo.ReadDashItems().Where(d => d.Active == true);
        }

        public List<DataChart> ExtractGraphList(int id)
        {
            InitNonExistingRepo();

            DashItem dashItem = repo.ReadDashItemWithGraph(id);
            List<DataChart> listData = new List<DataChart>();

            foreach (GraphData graph in dashItem.Graphdata)
            {
                listData.Add(new DataChart
                {
                    //controleren duplicaten DB
                    Label = graph.Label,
                    Value = graph.Value
                });
            }

            return listData;
        }

        public void AddTileZone(TileZone tile)
        {
            repo.AddTileZone(tile);
        }

        public Dashbord AddDashBord(string userId)
        {
            InitNonExistingRepo(true);

            gebruikerMgr = new GebruikerManager(uowManager);
            bool UoW = false;
            repo.SetUnitofWork(UoW);

            Dashbord dashbord = new Dashbord
            {
                //De te associëren gebruiker wordt opgehaald
                User = gebruikerMgr.GetGebruikers().FirstOrDefault(u => u.GebruikerId == userId),
                ZonesOrder = "[0,1,2,3,4,5,6,7,8,9]",
                TileZones = new Collection<TileZone>()
            };
            repo.AddDashBord(dashbord);
            uowManager.Save();
            repo.SetUnitofWork(true);

            return dashbord;
        }

        public void updateTilezonesOrder(int dashId,string zonesOrder)
        {
            InitNonExistingRepo();
            Dashbord dashbord = repo.ReadDashbordWithFollows(dashId);
            dashbord.ZonesOrder = zonesOrder;

            repo.UpdateDashboard(dashbord);
        }

        public void InitializeTileZoneOrder(int DashbordId)
        {
            InitNonExistingRepo();
            Dashbord dashbord = repo.ReadDashbordWithFollows(DashbordId);
            dashbord.TileZones.Count();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[0");
            int i;
            for (i=1;i<= dashbord.TileZones.Count(); i++)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(i);
            }
            stringBuilder.Append("]");
            dashbord.ZonesOrder = stringBuilder.ToString();
            repo.UpdateDashboard(dashbord);
        }

        public void AddOneZonesOrder(Dashbord dashbord)
        {
            InitNonExistingRepo();
            string zones = dashbord.ZonesOrder;
            JArray orde = JArray.Parse(zones);
            int volgendeZone = orde.Count();
            orde.Add(volgendeZone);
            zones = JsonConvert.SerializeObject(orde);
            dashbord.ZonesOrder = zones;
            repo.UpdateDashboard(dashbord);
        }
        public void DeleteOneZonesOrder(Dashbord dashbord)
        {
            InitNonExistingRepo();
            string zones = dashbord.ZonesOrder;
            JArray orde = JArray.Parse(zones);
            int verwijder = orde.Count()-1;
            LinkedList<int> result = new LinkedList<int>();
            foreach (int item in orde)
            {
                if (item != verwijder)
                {
                    result.AddLast(item);
                }
            }
            zones = JsonConvert.SerializeObject(result);
            dashbord.ZonesOrder = zones;
            repo.UpdateDashboard(dashbord);
        }
        public Dashbord DashbordInitGraphs(int dashId)
        {
            InitNonExistingRepo();

            Dashbord dashbord = repo.ReadDashbord(dashId);

            //We halen vaste grafieken op (AdminGraphs) en koppelen deze aan de 
            //nieuw aangemaakte dashboard van de nieuwe gebruiker
            IEnumerable<DashItem> dashItems = GetDashItems().Where(d => d.AdminGraph == true);

            if (dashbord.TileZones == null)
            {
                dashbord.TileZones = new Collection<TileZone>();
            }

            foreach (DashItem item in dashItems)
            {
                TileZone tile = new TileZone()
                {
                    DashItem = item,
                    Dashbord = dashbord
                };
                repo.AddTileZone(tile);
            }
            repo.UpdateDashboard(dashbord);
            return dashbord;
        }

        public void InitializeDashbordNewUsers(string userId)
        {
            InitNonExistingRepo();
            //Dashbord aanmaken en associëren met user
            //en initialiseren met vaste grafieken
            int dashbordId = AddDashBord(userId).DashbordId;
            DashbordInitGraphs(dashbordId);
            InitializeTileZoneOrder(dashbordId);
        }

        public void RemoveDashItem(int id)
        {
            InitNonExistingRepo();

            DashItem dashItem = repo.ReadDashItem(id);
            dashItem.Active = false;
            UpdateDashItem(dashItem);
            
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
                repo = new DashRepository(uowManager.UnitOfWork);
            }
            // Als we niet met UoW willen werken, dan maken we een repo aan als die nog niet bestaat.
            else
            {
                //zien of repo al bestaat
                if (repo == null)
                {
                    repo = new DashRepository();
                }
                else
                {
                    //checken wat voor repo we hebben
                    bool isUoW = repo.IsUnitofWork();
                    if (isUoW)
                    {
                        repo = new DashRepository();
                    }
                    else
                    {
                        // repo behoudt zijn context
                    }
                }
            }
        }
    }
}