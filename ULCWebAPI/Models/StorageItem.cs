using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ULCWebAPI.Models
{
    /// <summary>
    /// Model class for storing files
    /// </summary>
    public class StorageItem
    {
        [Required]
        public string Filename { get; set; }

        [Required]
        public string Hash { get; set; }
    }
}
