using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cdm.Models
{
    [Table("college_Departments")]
    public class college_Department
    {
        [Key]
        [Display(Name = "معرّف القسم")]
        public int college_DepartmentID { get; set; }

        [Required(ErrorMessage = "اسم القسم مطلوب")]
        [Display(Name = "اسم القسم")]
        public string DepartmentName { get; set; }

        [Required(ErrorMessage = "معرّف الكلية مطلوب")]
        [Display(Name = "الكلية")]
        public int CollegeID { get; set; }

        [ForeignKey("CollegeID")]
        public virtual College College { get; set; } // العلاقة مع الكلية

        public virtual ICollection<FacultyMember> FacultyMembers { get; set; } // أعضاء هيئة التدريس المرتبطين
    }
}
