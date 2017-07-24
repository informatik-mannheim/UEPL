using ProjectAPI.Models;
using System;
using System.Reflection;

namespace ProjectAPI.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="values"></param>
        public static void AssignProperties<T>(this ModelBase<T> o, object values)
        {
            var type = values.GetType();

            if (o.GetType() != type)
                throw new ArgumentException($"Types of {nameof(o)} and {nameof(values)} are not identical.");

            var props = type.GetProperties();

            foreach (var prop in props)
            {
                var value = prop.GetValue(o);

                if (value != null)
                    prop.SetValue(o, value);
            }
        }
    }
}
