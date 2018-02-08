﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ULCWebAPI.Security;

namespace ULCWebAPI.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UserLecture : ModelBase<int>
    {
        [JsonIgnore]
        public int UserID { get; set; }   
        /// <summary>
        /// 
        /// </summary>
        public ApplicationUser User { get; set; }
 
        [JsonIgnore]
        public string LectureID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Lecture Lecture { get; set; }
    }
}
