using System;
using System.ComponentModel.DataAnnotations.Schema;
using citr.Services;

namespace citr.Models
{
    public class HistoryRow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public DateTime Date { get; set; }

        public int AuthorEmployeeID { get; set; }        

        public string Text { get; set; }

        public Employee AuthorEmployee { get; set; }

    }
}
