using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace RequestsAccess.Models.ViewModels
{
    public class ResourceViewModel
    {
        public int ResourceID { get; set; }

        [Required(ErrorMessage = "Введите название ресурса")]
        [Display(Name="Название")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        //[Required(ErrorMessage = "Укажите владельца ресурса")]      
        [Display(Name = "Владелец")]    
        public int? OwnerId { get; set; }

        public int ResourceCategoryID { get; set; }

        public ResourceCategory ResourceCategory { get; set; }

        //public List<Employee> Employees { get; set; }

    }
}
