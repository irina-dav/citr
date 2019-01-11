using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace citr.Models.ViewModels
{
    public class RequestViewModel
    {

        public Request Request { get; set; }

        /*public int RequestID { get; set; }

        [Required(ErrorMessage = "Выберите сотрудников для предоставления доступа")]
        [Display(Name = "Доступ для сотрудников")]
        public int[] EmployeesForIds { get; set; }

        [Required(ErrorMessage = "Выберите информационный ресурс(ы)")]
        [Display(Name = "Информационный ресурс")]
        public int[] ResourcesIDs { get; set; }

        public int AuthorEmployeeID { get; set; }

        public string ResourceName { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime ChangeDate { get; set; }

        public RequestState State { get; set; }

        public List<HistoryRow> History { get; set; }*/
    }
}
