using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
