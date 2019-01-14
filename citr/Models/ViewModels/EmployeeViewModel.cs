using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace citr.Models.ViewModels
{
    public class EmployeeViewModel
    {      
        public int EmployeeID { get; set; }

        [Required(ErrorMessage = "Укажите ФИО сотрудника")]
        [Display(Name = "ФИО (полностью)")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Укажите должность сотрудника")]
        [Display(Name = "Должность")]
        public string Position { get; set; }

        [Remote("IsAccountExist", controller: "Employee", AdditionalFields = "EmployeeID", ErrorMessage = "Уже существует сотрудник с указанный логином")]
        [Required(ErrorMessage = "Укажите логин сотрудника")]
        public string Account { get; set; }

        [Required(ErrorMessage = "Укажите email сотрудника")]
        [EmailAddress(ErrorMessage = "Укажите email в корректном формате")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Укажите роль доступа сотрудника")]
        [Display(Name = "Роль доступа")]
        public int UserRoleID { get; set; }

        public IEnumerable<SelectListItem> Roles { get; set; }

        public EmployeeViewModel()
        {
        }

        public EmployeeViewModel(Employee empl)
        {
            this.Account = empl.Account;
            this.Email = empl.Email;
            this.EmployeeID = empl.EmployeeID;
            this.FullName = empl.FullName;
            this.Position = empl.Position;
            this.UserRoleID = empl.UserRoleID;             
        }
    }
}
