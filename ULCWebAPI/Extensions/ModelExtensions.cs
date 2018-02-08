using Microsoft.EntityFrameworkCore;
using ULCWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ULCWebAPI.Extensions
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_context"></param>
        /// <returns></returns>
        public static IQueryable<T> GetFullTable<T>(this APIDatabaseContext _context)
        {
            Type genericTableType = typeof(T);

            switch(genericTableType.Name)
            {
                case "Lecture":
                    return (IQueryable<T>)_context.Lectures.Include(l => l.Contents).Include(l => l.UserLectures).ThenInclude(ul => ul.User);

                case "Package":
                    return (IQueryable<T>)_context.Packages.Include(p => p.Dependencies);

                case "Artifact":
                    return (IQueryable<T>)_context.Artifacts.Include(a => a.StorageItems);

                case "ApplicationUser":
                    return (IQueryable<T>)_context.Users.Include(u => u.UserLectures).ThenInclude(ul => ul.Lecture);

                case "LoginToken":
                    return (IQueryable<T>)_context.Tokens.Include(lt => lt.User).ThenInclude(user => user.UserLectures).ThenInclude(ul => ul.Lecture);
                default:
                    return default(IQueryable<T>);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_context"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static T GetSingleOrDefault<T>(this APIDatabaseContext _context, Func<T, bool> predicate)
        {
            var queryable = _context.GetFullTable<T>();
            return queryable != default(IQueryable<T>) ? queryable.SingleOrDefault(predicate) : default(T);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_context"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsTokenValid(this APIDatabaseContext _context, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var loginToken = _context.Tokens.SingleOrDefault(t => t.Token == token);
            return loginToken != null && loginToken.Valid > DateTime.Now;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_context"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static LoginToken GetLoginToken(this APIDatabaseContext _context, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return default(LoginToken);

            return _context.GetFullTable<LoginToken>().SingleOrDefault(t => t.Token == token);
        }
    }
}
