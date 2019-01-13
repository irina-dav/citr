using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace citr.Models
{
    public class RequestDetail : IViewTableRow
    {
        public int ID { get; set; }

        public int ResourceID { get; set; }
        public virtual Resource Resource { get; set; }
       
        public int ResourceOwnerID { get; set; }
        public virtual Employee ResourceOwner { get; set; }

        public int RoleID { get; set; }
        public virtual AccessRole Role { get; set; }

        public int EmployeeAccessID { get; set; }
        public Employee EmployeeAccess { get; set; }

        public ResourceApprovingResult ApprovingResult { get; set; }

        [NotMapped]
        public bool IsDeleted { get; set; }

        [NotMapped]
        public bool CanDelete { get; set; } = true;

        [NotMapped]
        public bool CanApprove { get; set; }

        public string TicketNumber { get; set; }  

    }
}
