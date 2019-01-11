using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace citr.Models.ViewModels
{
    public class ResourceCategoryViewModel
    {
        public string Name { get; set; }
        public string ParentName { get; set; }
        public int ParentCategoryId { get; set; }
    }
}
