using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RemoteLight.Models
{
    public class CardOwner
    {
        public int Id { get; set; }

        [Display(Name = "Created at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Name of card owner")]
        public string Name { get; set; } = string.Empty;

        [ValidateNever]
        public virtual RFIDCard RFIDCard { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, CreatedAt: {CreatedAt}, Name: {Name}";
        }
    }
}
