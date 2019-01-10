using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RequestsAccess.Models
{
    public interface IViewTableRow
    {
       bool IsDeleted { get; set; }
    }

    public class ResourceAccess : IViewTableRow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int ResourceID { get; set; }

        public virtual Resource Resource { get; set; }

        public ResourceApprovingResult ApprovingResult { get; set; }

        [NotMapped]
        public bool IsDeleted { get; set; }

        [NotMapped]
        public bool CanDelete { get; set; } = true;

        [NotMapped]
        public bool CanApprove { get; set; }
    }

    public enum ResourceApprovingResult
    {
        [Display(Name = "")]
        None,
        [Display(Name = "Согласовано")]
        Approved,
        [Display(Name = "Отказ")]
        Declined
    }
}
