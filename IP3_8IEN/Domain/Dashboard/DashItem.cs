﻿using IP3_8IEN.BL.Domain.Dashboard;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IP3_8IEN.BL.Domain.Dashboard
{
    public class DashItem
    {
        [Key]
        public int DashItemId { get; set; }
        public bool AdminGraph { get; set; }
        public int AantalDagen { get; set; }
        public int aantalUren { get; set; }
        public string Naam { get; set; }
        public string Titel { get; set; }
        public string Persoon1 { get; set; }
        public string Persoon2 { get; set; }
        public string Persoon3 { get; set; }
        public string Persoon4 { get; set; }
        public string Persoon5 { get; set; }
        public string Type { get; set; }
        public string Town { get; set; }
        //Active -> bij 'false' wordt de 'DashItem' beschouwd als verwijderd
        public bool Active { get; set; }

        public DateTime LastModified { get; set; }

        public ICollection<GraphData> Graphdata { get; set; }
        public ICollection<GraphData2> Graphdata2 { get; set; }

        public ICollection<TileZone> TileZones { get; set; }
        public ICollection<Follow> Follows { get; set; }
    }
}