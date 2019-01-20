using citr.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace citr.Models.ViewModels
{
    public class RequestViewModel
    {
        [Display(Name = "Номер заявки")]
        public int RequestID { get; set; }

        [ListReqiredAttribute(ErrorMessage = "Заполните детали запроса")]
        public virtual List<RequestDetailViewModel> Details { get; set; } = new List<RequestDetailViewModel>();

        [Display(Name = "Комментарий")]
        public string Comment { get; set; }        

        [Display(Name = "Статус заявки")]
        public RequestState State { get; set; }

        public Employee Author { get; set; }

        public virtual List<HistoryRow> History { get; set; }

        public RequestViewModel()
        {

        }

        public RequestViewModel(Request request)
        {            
            RequestID = request.RequestID;
            Comment = request.Comment;
            State = request.State;
            Author = request.Author;
            History = request.History;
            foreach (RequestDetail detail in request.Details)
            {
                Details.Add(new RequestDetailViewModel(detail));
            }
        }
    }
}
