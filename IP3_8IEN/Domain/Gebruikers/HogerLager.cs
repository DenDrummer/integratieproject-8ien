﻿using IP3_8IEN.BL.Domain.Data;
using IP3_8IEN.BL.Domain.Gebruikers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IP3_8IEN.BL.Domain.Gebruikers
{
    public class HogerLager : AlertInstelling
    {
        public Onderwerp Onderwerp2 { get; set; }
        public bool OneHigherThanTwo { get; set; }
    }
}
