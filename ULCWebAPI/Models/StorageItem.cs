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
    public class StorageItem : ModelBase<int>
    {
        /// <summary>
        /// Name of the File with extension
        /// </summary>
        [Required]
        public string Filename { get; set; }

        /// <summary>
        /// Hash value of the file content to sign and verify data integrity
        /// </summary>
        [Required]
        public string Hash { get; set; }
    }
}
