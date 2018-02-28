using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ULCWebAPI.Models
{
    /// <summary>
    /// Contains a Lecture object with string based key and content references
    /// </summary>
    public class Lecture : ModelBase<string>
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<Package> Contents { get; set; } = new List<Package>();

        /// <summary>
        /// 
        /// </summary>
        public List<LectureStorageItem> StorageItems { get; set; } = new List<LectureStorageItem>();

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public List<UserLecture> UserLectures { get; internal set; } = new List<UserLecture>();

    }
}
