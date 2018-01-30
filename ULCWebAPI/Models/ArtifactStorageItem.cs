using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ULCWebAPI.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ArtifactStorageItem : ModelBase<int>
    {
        /// <summary>
        /// 
        /// </summary>
        public Artifact ArtifactRef { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Filename { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Hash { get; set; }
    }
}
