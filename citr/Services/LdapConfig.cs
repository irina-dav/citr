﻿using System.Collections.Generic;

namespace citr.Models
{
    public class LdapConfig
    {
        public string Url { get; set; }
        public string BindDn { get; set; }
        public string BindCredentials { get; set; }
        public string AuthFilter { get; set; }
        public string SearchBase { get; set; }
        public string SearchFilter { get; set; }
        public string EmployeesQuery { get; set; }
        public string AdminCn { get; set; }
        public List<string> OrgUnits { get; set; }
    }
}
