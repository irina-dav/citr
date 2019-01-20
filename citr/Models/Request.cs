using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using citr.Infrastructure;
using citr.Services;
using Microsoft.Extensions.Configuration;

namespace citr.Models
{
    public class Request : IHistoryable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestID { get; set; }

        public virtual List<RequestDetail> Details { get; set; }

        public string Comment { get; set; }

        public int AuthorID { get; set; }
        public virtual Employee Author { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime ChangeDate { get; set; }

        public RequestState State { get; set; }

        public virtual List<HistoryRow> History { get; set; }

        public int ObjectID { get => RequestID; }
   
    }

    public enum RequestState
    {
        [Display(Name = "Создание")]
        New = 1,
        [Display(Name = "Согласование")]
        Approving,
        [Display(Name = "Не согласована")]
        NotApproved,
        [Display(Name = "Согласована")]
        Approved,
        [Display(Name = "Отказ")]
        Canceled
    }

}
