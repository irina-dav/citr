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
        [Display(Name = "Номер заявки")]
        public int RequestID { get; set; }

        [ListReqiredAttribute(ErrorMessage = "Заполните детали запроса")]
        public virtual List<RequestDetail> Details { get; set; }

        //[ListReqiredAttribute(ErrorMessageResourceName = "RequesResourcesRequired", ErrorMessageResourceType = typeof(ValidationRes))]
        //public virtual List<EmployeeAccess> EmployeeAccesses { get; set; }

       // [NotMapped]
       // public Guid[] ResourceIDs { get; set; }

        //[ListReqiredAttribute(ErrorMessageResourceName = "RequestEmployeesRequired", ErrorMessageResourceType = typeof(ValidationRes))]
       // public virtual List<ResourceAccess> ResourceAccesses { get; set; }

        [Display(Name = "Комментарий")]
        public string Comment { get; set; }

        public int AuthorID { get; set; }        

        public DateTime CreateDate { get; set; }

        public DateTime ChangeDate { get; set; }

        [Display(Name = "Статус заявки")]
        public RequestState State { get; set; }

        public Employee Author { get; set; }

        public virtual List<HistoryRow> History { get; set; }

        public int ObjectID { get => RequestID; }

        //public string TicketNumber { get; set; }

        //[NotMapped]
        //public string TicketUrl { get; set; }     
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
        [Display(Name = "Отменена")]
        Canceled
    }

}
