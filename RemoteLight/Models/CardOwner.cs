﻿using System;
using System.Collections.Generic;

namespace RemoteLight.Models
{
    public class CardOwner
    {
        public int Id { get; set; } // Read-only ID
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Name { get; set; }

        // Relationships
        public virtual ICollection<RFIDCard> RFIDCards { get; set; }
    }
}
