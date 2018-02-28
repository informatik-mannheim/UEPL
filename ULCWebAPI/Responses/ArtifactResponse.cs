using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ULCWebAPI.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ArtifactResponse : ModelBase<string>
    {
        /// <summary>
        /// 
        /// </summary>
        public string InstallAction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RemoveAction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SwitchAction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UnswitchAction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<ArtifactStorageResponse> StorageItems { get; set; }
    }
}
