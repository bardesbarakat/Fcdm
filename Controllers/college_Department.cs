using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cdm.Models
{

    public class college_Departments
    {
        [Key]
        public int college_DepartmentID { get; set; }

        public string DepartmentName { get; set; }

        public string DepartmentNameEN { get; set; }

        // المفتاح الأجنبي
        public int CollegeID { get; set; }

        // العلاقة مع جدول الكليات
        [ForeignKey("CollegeID")]
        public virtual College College { get; set; }
    }
}
