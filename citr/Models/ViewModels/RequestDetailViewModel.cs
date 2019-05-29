namespace citr.Models.ViewModels
{
    public class RequestDetailViewModel : IViewTableRow
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

        public bool IsDeleted { get; set; }

        public bool CanDelete { get; set; } = true;

        public bool CanApprove { get; set; }

        public long? TicketID { get; set; }

        public virtual Ticket Ticket { get; set; }

        public string TicketInfo
        {
            get
            {
                if (TicketID != null)
                {
                    if (Ticket.EndDate.HasValue)
                    {
                        return $"выполнена {Ticket.EndDate.Value.ToString("dd.MM.yy")}";
                    }
                    else
                    {
                        return "выполняется";
                    }
                }
                else
                {
                    return "";
                }
            }
        }

        public string TicketUrl { get; set; }


        public RequestDetailViewModel() { }

        public RequestDetailViewModel(RequestDetail detail)
        {
            ID = detail.ID;
            Resource = detail.Resource;
            ResourceID = detail.ResourceID;
            ResourceOwner = detail.ResourceOwner;
            ResourceOwnerID = detail.ResourceOwnerID;
            Role = detail.Role;
            RoleID = detail.RoleID;
            EmployeeAccessID = detail.EmployeeAccessID;
            EmployeeAccess = detail.EmployeeAccess;
            ApprovingResult = detail.ApprovingResult;
            Ticket = detail.Ticket;
            TicketID = detail.TicketID;
        }

    }
}
