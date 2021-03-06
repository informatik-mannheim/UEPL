﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ULCWebAPI.Models
{
    /// <summary>
    /// Class to hold context handling informations and scripts, aswell as files
    /// </summary>
    public class Artifact : ModelBase<string>
    {
        /// <summary>
        /// Action to install this artifact
        /// </summary>
        [Required]
        public string InstallAction { get; set; }

        /// <summary>
        /// Action to remove this artifact
        /// </summary>
        [Required]
        public string RemoveAction { get; set; }

        /// <summary>
        /// Action to switch to this artifact context
        /// </summary>
        [Required]
        public string SwitchAction { get; set; }

        /// <summary>
        /// Action to unload this artifact context
        /// </summary>
        [Required]
        public string UnswitchAction { get; set; }

        /// <summary>
        /// Action to upgrade from a lower version of this artifact
        /// </summary>
        [DefaultValue("")]
        public string UpgradeAction { get; set; } = string.Empty;

        /// <summary>
        /// Storage items (files) associated with this artifact
        /// </summary>
        public List<ArtifactStorageItem> StorageItems { get; set; } = new List<ArtifactStorageItem>();

        /// <summary>
        /// Previous versions
        /// </summary>
        [JsonIgnore]
        public List<ArtifactBackup> Backups { get; set; } = new List<ArtifactBackup>();
    }
}
