using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using System.Threading.Tasks;

namespace ULCWebAPI.Models
{
    /// <summary>
    /// Basis model with key identifier
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ModelBase<T>
    {
        /// <summary>
        /// Global identifier
        /// </summary>
        [Key, Required]
        public T ID { get; set; }
    }
}
