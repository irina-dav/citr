using System.ComponentModel.DataAnnotations;

namespace citr.Models.ViewModels
{
    public class ResourceViewModel
    {
        public int ResourceID { get; set; }

        [Required(ErrorMessage = "Введите название ресурса")]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Владелец")]
        public int? OwnerId { get; set; }

        public int ResourceCategoryID { get; set; }

        public ResourceCategory ResourceCategory { get; set; }
    }
}
