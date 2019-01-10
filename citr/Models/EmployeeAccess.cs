using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RequestsAccess.Models
{
    public class EmployeeAccess : IViewTableRow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int EmployeeID { get; set; }

        public AccessLevel AccessLevel { get; set; }        

        public virtual Employee Employee { get; set; }

        [NotMapped]
        public bool IsDeleted { get; set; }

        [NotMapped]
        public bool CanDelete { get; set; }
    }

    public enum AccessLevel
    {
        Read,
        Write
    }
}
