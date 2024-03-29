﻿using citr.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace citr.Models
{
    public class Resource : IHistoryable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResourceID { get; set; }

        [Required(ErrorMessage = "Введите название ресурса")]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Расположение")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Выберите категорию ресурса")]
        [Range(1, int.MaxValue, ErrorMessage = "Выберите категорию ресурса")]
        [Display(Name = "Категория")]
        public int CategoryID { get; set; }
        public virtual ResourceCategory Category { get; set; }

        [Required(ErrorMessage = "Укажите владельца ресурса")]
        [Display(Name = "Владелец")]
        public int? OwnerEmployeeID { get; set; }
        public virtual Employee OwnerEmployee { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime ChangeDate { get; set; }

        public bool Hidden { get; set; }

        public virtual List<HistoryRow> History { get; set; }

        public int ObjectID => ResourceID;

        public virtual List<AccessRole> Roles { get; set; }
    }
}
