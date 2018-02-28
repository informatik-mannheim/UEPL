using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ULCWebAPI.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ArtifactBackupStorageItem : StorageItem
    {
        /// <summary>
        /// Reference to the backup
        /// </summary>
        [JsonIgnore]
        public ArtifactBackup ArtifactBackupRef { get; set; }
    }
}
