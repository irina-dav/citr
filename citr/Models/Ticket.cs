using System;

namespace citr.Models
{
    public class Ticket
    {
        public long TicketID { get; set; }

        public string TicketNumber { get; set; }

        public DateTime? EndDate { get; set; }

        public string EndByUser { get; set; }
    }
}
