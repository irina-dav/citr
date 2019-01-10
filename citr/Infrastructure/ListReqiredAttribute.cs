using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using RequestsAccess.Models;

namespace RequestsAccess.Infrastructure
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
