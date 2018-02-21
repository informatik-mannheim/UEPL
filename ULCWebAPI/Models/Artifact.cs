using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ULCWebAPI.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Artifact : ModelBase<string>
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string InstallAction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string RemoveAction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string SwitchAction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string UnswitchAction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<ArtifactStorageItem> StorageItems { get; set; } = new List<ArtifactStorageItem>();
    }
}
