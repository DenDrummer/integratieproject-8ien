﻿using IP_8IEN.BL.Domain.Gebruikers;

namespace IP_8IEN.BL.Domain.Data
{
    public class Alert
    {
        public int AlertId { get; set; }
        public int VolgId { get; private set; }
        public AlertInstelling AlertInstelling;

    }
}
