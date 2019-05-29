using System.ComponentModel.DataAnnotations.Schema;

namespace citr.Models
{
    public class AccessRole : IViewTableRow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public string Name { get; set; }

        [NotMapped]
        public bool IsDeleted { get; set; }

    }
}
