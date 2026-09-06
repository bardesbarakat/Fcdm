using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cdm.Models
{
    public class SecondmentsData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required(ErrorMessage = "اختيار نوع الإعارة مطلوب")]
        [Display(Name = "نوع الإعارة")]
        public int SecondmentsTypeId { get; set; }


        [Required]
        [Display(Name = "عضو هيئة التدريس")]
        public int SecondmentsMemberId { get; set; }

        [Display(Name = "الدولة")]
        public string SecondmentsCountry { get; set; }

        [Display(Name = "السنة")]
        public string SecondmentsYear { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "تاريخ البداية")]
        public DateTime? SecondmentsStartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "تاريخ النهاية")]
        public DateTime? SecondmentsEndDate { get; set; }

        [Display(Name = "المستخدم الذي أضاف")]
        public int? UserID { get; set; }

        [Display(Name = "تاريخ الإدخال")]
        public DateTime? InsertedDatetime { get; set; }

        [Display(Name = "تاريخ التحديث")]
        public DateTime? updatedDatetime { get; set; }

        // العلاقات
        [ForeignKey("SecondmentsTypeId")]
        public virtual SecondmentsType SecondmentsType { get; set; }

        [ForeignKey("SecondmentsMemberId")]
        public virtual FacultyMember FacultyMember { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [Display(Name = "رقم الموافقة الأمنية")]
        public string SecurityApprovalNumber { get; set; }

        [Display(Name = "تاريخ الموافقة الأمنية")]
        [DataType(DataType.Date)]
        public DateTime? SecurityApprovalDate { get; set; }

        [Display(Name = "تاريخ إخلاء الطرف")]
        [DataType(DataType.Date)]
        public DateTime? ClearanceDate { get; set; }

        [Display(Name = "رقم قرار رئيس الجامعة")]
        public string UniversityDecisionNumber { get; set; }

        [Display(Name = "تاريخ قرار رئيس الجامعة")]
        [DataType(DataType.Date)]
        public DateTime? UniversityDecisionDate { get; set; }

        [Display(Name = "مكان الإعارة")]
        public string SecondmentLocation { get; set; }

        [Display(Name = "تاريخ استلام العمل بعد العودة")]
        [DataType(DataType.Date)]
        public DateTime? ReturnWorkDate { get; set; }

        [Display(Name = "ملف موافقة رئيس الجامعة")]
        public string PresidentApprovalFile { get; set; }

        [Display(Name = "مذكرة الاعتماد")]
        public string AccreditationMemo { get; set; }
        // داخل كلاس SecondmentsData:
        [NotMapped]
        public HttpPostedFileBase PresidentApprovalUpload { get; set; }

        [NotMapped]
        public HttpPostedFileBase AccreditationMemoUpload { get; set; }

        // للمساعدة في عرض البيانات
        [NotMapped]
        public FacultyMemberDetailsVM FacultyDetails { get; set; }

        //public int? CollegeID { get; set; }
        //public int? College_DepartmentID { get; set; }
        //public int? ID_JobTitle { get; set; }
       

    }
    public class FacultyMemberDetailsVM
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime HireDate { get; set; }
        public string NationalID { get; set; }
        public string InsuranceNo { get; set; }
    }
}
