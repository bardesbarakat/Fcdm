using cdm.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
namespace cdm.Models
{
    public class FacultyMember
    {

        [Key]
        public int FacultyMemberID { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "الكلية مطلوبة")]
        [Display(Name = "الكلية")]
        public int CollegeID { get; set; }

        [ForeignKey("CollegeID")]
        public virtual College College { get; set; }

        [Required(ErrorMessage = "القسم مطلوب")]
        [Display(Name = "القسم")]
        public int College_DepartmentID { get; set; }
        

        [ForeignKey("College_DepartmentID")]
        public virtual college_Department CollegeDepartment { get; set; }

        [Required(ErrorMessage = "تاريخ التعيين مطلوب")]
        [Display(Name = "تاريخ التعيين")]
        [DataType(DataType.Date)]
        public DateTime? HireDate { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        public string Email { get; set; }

        [Display(Name = "رقم التواصل")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "رقم التأمين مطلوب")]
        [Display(Name = "رقم التأمين")]
        public string InsuranceNumber { get; set; }

        [Required(ErrorMessage = "الرقم القومي مطلوب")]
       
        [StringLength(14, MinimumLength = 14, ErrorMessage = "يجب أن يكون الرقم القومي 14 رقمًا")]
        [RegularExpression(@"^\d{14}$", ErrorMessage = "الرقم القومي يجب أن يحتوي على 14 رقمًا")]
        

        [Display(Name = "الرقم القومي")]
        public string NationalID { get; set; }

        [Required(ErrorMessage = "النوع مطلوب")]
        [Display(Name = "النوع")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
        [Display(Name = "تاريخ الميلاد")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "الدرجة الوظيفية مطلوبة")]
        [Display(Name = "الوظيفة")]
        public int ID_JobTitle { get; set; }

        [ForeignKey("ID_JobTitle")]
        public virtual JobTitle JobTitle { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } // ✅ أضف الخاصية دي

        [DataType(DataType.DateTime)]
        public DateTime? UserInsertedDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? UserUpdatedDate { get; set; }

        [Display(Name = "التخصص")]
        public string Specialization { get; set; }

    }
}