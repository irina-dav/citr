﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Reflection;
using RequestsAccess.Models;

namespace RequestsAccess.Infrastructure.TagHelpers
{
    [HtmlTargetElement("select", Attributes = "model-for")]
    public class SelectEmployeeOptionTagHelper : TagHelper
    {
        private IEmployeeRepository repository;

        public SelectEmployeeOptionTagHelper(IEmployeeRepository repo)
        {
            repository = repo;
        }

        public ModelExpression ModelFor { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.Content.AppendHtml((await output.GetChildContentAsync(false)).GetContent());
            Employee selected = ModelFor.Model as Employee;
            //PropertyInfo property = typeof(Employee).GetTypeInfo().GetDeclaredProperty(ModelFor.Name);

            foreach (Employee empl in repository.Employees)
            {
                if (selected != null && selected.Equals(empl))
                {
                    output.Content.AppendHtml($"<option selected>{empl}</option>");
                }
                else
                {
                    output.Content.AppendHtml($"<option value={empl.EmployeeID}>{empl.FullName}</option>");
                }
            }
            output.Attributes.SetAttribute("Name", ModelFor.Name);
            output.Attributes.SetAttribute("Id", ModelFor.Name);
        }
    }
}
