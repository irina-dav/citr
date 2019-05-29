using citr.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace citr.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ListReqiredAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            IEnumerable<IViewTableRow> rows = (value as IEnumerable<IViewTableRow>);
            bool r = (rows != null && rows.Count() > 0 && rows.Any(c => !c.IsDeleted));
            return r;
        }
    }
}
