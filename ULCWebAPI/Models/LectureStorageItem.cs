using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ULCWebAPI.Models
{
    /// <summary>
    /// Class to hold file information associated with a lecture
    /// </summary>
    public class LectureStorageItem : StorageItem
    {
        /// <summary>
        /// Reference to the corresponding lecture
        /// </summary>
        [JsonIgnore]
        public Lecture LectureRef { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Url { get; set; } = string.Empty;
    }
}
