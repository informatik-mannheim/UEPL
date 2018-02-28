using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ULCWebAPI.Models
{
    /// <summary>
    /// Class for storing file informations associated with 
    /// </summary>
    public class ArtifactStorageItem : StorageItem
    {
        /// <summary>
        /// Reference object to keep track
        /// </summary>
        [JsonIgnore]
        public Artifact ArtifactRef { get; set; }
    }
}
