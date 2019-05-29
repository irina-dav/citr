namespace citr.Models
{
    public class RequestDetail
    {
        public int ID { get; set; }

        public int ResourceID { get; set; }
        public virtual Resource Resource { get; set; }

        public int ResourceOwnerID { get; set; }
        public virtual Employee ResourceOwner { get; set; }

        public int RoleID { get; set; }
        public virtual AccessRole Role { get; set; }

        public int EmployeeAccessID { get; set; }
        public virtual Employee EmployeeAccess { get; set; }

        public ResourceApprovingResult ApprovingResult { get; set; }

        public long? TicketID { get; set; }
        public virtual Ticket Ticket { get; set; }
    }
}
