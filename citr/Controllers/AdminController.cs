﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RequestsAccess.Services;


namespace RequestsAccess.Controllers
{
    public class AdminController : Controller
    {
        private ILdapService ldapService;

        public AdminController(ILdapService ldapServ)
        {
            ldapService = ldapServ;
        }

        public ViewResult PopulateEmployees()
        {
            var result = ldapService.PopulateEmployees().ToString();
            
            return View(model: result);
        }
    }
}