﻿using System;
using System.ComponentModel.DataAnnotations;

namespace IP3_8IEN.BL.Domain.Gebruikers
{
    public class Alert
    {
        [Key]
        public int AlertId { get; set; }
        public string AlertContent { get; set; }
        public DateTime CreatedOn { get; set; }
        
        public AlertInstelling AlertInstelling;


        public override string ToString() => AlertContent;
    }
}