﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;

namespace RemoteLight.Models
{
    public class AccessLog
    {
        public long Id { get; set; } // Read-only ID
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Data { get; set; }

        // Relationships
        public string RFIDCardId { get; set; }
        [ValidateNever]
        public virtual RFIDCard RFIDCard { get; set; }
    }
}