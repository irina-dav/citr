using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RequestsAccess.Models
{
    public class Employee
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
    }
}
