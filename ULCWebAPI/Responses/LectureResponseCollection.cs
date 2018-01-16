using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectAPI.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class LectureReferenceResponse : ModelBase<string>
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Url { get; set; }
    }
}
