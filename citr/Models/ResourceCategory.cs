using System.ComponentModel.DataAnnotations.Schema;

namespace citr.Models
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
