using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RequestsAccess.Models
{
    public class AppUser
    {
        public string DisplayName { get; set; }
        public string Username { get; set; }
        public bool IsAdmin { get; set; }
    }
}
