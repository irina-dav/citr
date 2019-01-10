using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RequestsAccess.Models.ViewModels
{
    public class EmailViewModel
    {
        public Employee Recipient { get; set; }
        public Request Request { get; set; }
        public IEnumerable<Resource> Resources { get; set; }
        public string Url { get; set; }
    }
}
