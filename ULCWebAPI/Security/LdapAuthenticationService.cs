using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using ULCWebAPI.Helper;

namespace ULCWebAPI.Security
{
    /// <summary>
    /// Service for authentication against a directory server (LDAP)
    /// </summary>
    public class LdapAuthenticationService : IAuthenticationService
    {
        private readonly LdapConfig _config;
        private readonly LdapConnection _connection;
        private readonly string[] extractFields = new string[] { "employeeType", "displayName", "uidNumber", "mail" };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public LdapAuthenticationService(LdapConfig config)
        {
            _config = config;
            _connection = new LdapConnection { SecureSocketLayer = _config.UseSSL };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public LdapAuthenticationService(IOptions<LdapConfig> config)
        {
            _config = config.Value;
            _connection = new LdapConnection { SecureSocketLayer = _config.UseSSL};
        }

        public ApplicationUser Login(string username, string password)
        {
            try
            {
                _connection.Connect(_config.Url, _config.Port);
                _connection.Bind($"uid={username};" + _config.BindDN, password);

                if (!_connection.Bound)
                    return null;

                var filter = string.Format(_config.SearchFilter, username);
                var results = _connection.Search(_config.SearchBase, LdapConnection.SCOPE_SUB, filter, extractFields, false);
                var displayName = "";
                var employeeType = "";
                var uidNumber = "";
                var mail = "";
                
                if(results.Count != 0)
                {
                    while (results.hasMore())
                    {
                        try
                        {
                            var entry = results.next();

                            if (entry != null)
                            {
                                displayName = entry.getAttribute(nameof(displayName)).StringValue;
                                employeeType = entry.getAttribute(nameof(employeeType)).StringValue;
                                uidNumber = entry.getAttribute(nameof(uidNumber)).StringValue;
                                mail = entry.getAttribute(nameof(mail)).StringValue;
                            }
                        }
                        catch (LdapException ex)
                        {
                            continue;
                        }
                    }
                }

                return _connection.Bound ? new ApplicationUser { UserName = username, DisplayName = displayName, LdapID = uidNumber, EmployeeType = employeeType, Email = mail } : null;
            }
            catch(Exception e)
            {
                Tracer.TraceMessage(e, TraceLevel.ERROR);
                return null;
            }
        }

        internal static void PrintAttributes(LdapEntry entry)
        {
            foreach (LdapAttribute qs in entry.getAttributeSet())
            {
                Console.Write(qs.Name + " => ");
                var size = qs.size();

                if (size > 1)
                {
                    var loopCount = 1;

                    foreach (var sv in qs.StringValueArray)
                    {
                        if (loopCount++ == size)
                            Console.WriteLine(sv);
                        else
                            Console.Write(sv + ", ");
                    }
                }
                else
                    Console.WriteLine(qs.StringValue);
            }
        }
    }
}
