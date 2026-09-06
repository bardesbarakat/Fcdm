using System;
using System.ComponentModel.DataAnnotations;

namespace cdm.Models
{
    public class Department
    {
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "اسم القسم مطلوب")]
        [Display(Name = "اسم القسم")]
        public string DepartmentName { get; set; }

        [Display(Name = "الاسم الإنجليزي")]
        public string DepartmentNameEN { get; set; }

        [Display(Name = "الوصف")]
        public string Description { get; set; }

        [Display(Name = "تاريخ الإضافة")]
        public DateTime? CreatedAt { get; set; }
    }
}