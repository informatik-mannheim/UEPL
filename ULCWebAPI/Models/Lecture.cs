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
        public ICollection<Package> Contents { get; set; } = new List<Package>();
    }
}
