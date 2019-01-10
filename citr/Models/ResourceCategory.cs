using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RequestsAccess.Models
{
    public class ResourceCategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int ParentCategoryID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

    }
}
