using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectAPI.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class LectureResponse : ModelBase<string>
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<Package> Content { get; set; }
    }
}
