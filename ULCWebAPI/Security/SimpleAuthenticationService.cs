﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectAPI.Security
{
    public class SimpleAuthenticationService : IAuthenticationService
    {
        public ApplicationUser Login(string username, string password)
        {
            return new ApplicationUser { UserName = "demo" };
        }
    }
}
