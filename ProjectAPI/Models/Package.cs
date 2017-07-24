using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Package : ModelBase<int>
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string ArtifactRefID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ICollection<Package> Dependencies { get; set; } = new List<Package>();
    }
}
