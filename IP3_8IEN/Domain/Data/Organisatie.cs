﻿using System;
using System.Collections.Generic;

namespace IP_8IEN.BL.Domain.Data
{
    public class Organisatie : Onderwerp
    {
        public string NaamOrganisatie { get; set; }
        public string Afkorting { get; set; }
        public string Twitter { get; set; }
        public string Facebook { get; set; }
        //DateTime kan niet 'null' zijn
        //public DateTime Oprichtingsdatum { get; set; }
        public Persoon Oprichter { get; set; }
        public Persoon Leider { get; set; }
        public string Ideologie { get; set; }

        public ICollection<Tewerkstelling> Tewerkstellingen { get; set; }
    }
}
