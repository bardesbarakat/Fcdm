using cdm.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cdm.Models
{
    public class Conference
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "رقم عضو هيئة التدريس")]
        public int FacultyMemberId { get; set; }

        [Display(Name = "عنوان المؤتمر")]
        public string ConferenceTitle { get; set; }

        [Display(Name = "مكان انعقاد المؤتمر")]
        public string ConferenceLocation { get; set; }

        [Display(Name = "فترة انعقاد المؤتمر")]
        public string ConferencePeriod { get; set; }

        [Display(Name = "البلد")]
        public string ConferenceCountry { get; set; }

        [Display(Name = "المساهمة")]
        public string Contribution { get; set; }

        [Display(Name = "نوع المشاركة")]
        public string ParticipationType { get; set; }

        [Display(Name = "نوع المؤتمر")]
        public string ConferenceType { get; set; }

        [Display(Name = "ملف قرار رئيس الجامعة")]
        public string UniversityPresidentDecisionFile { get; set; }

        [Display(Name = "ملف مذكرة الاعتماد")]
        public string ApprovalMemoFile { get; set; }

        [Display(Name = "ملفات أخرى")]
        public string OtherFiles { get; set; }

        [Display(Name = "ملاحظات")]
        public string Notes { get; set; }

        [Display(Name = "تاريخ الإدخال")]
        public DateTime InsertedDate { get; set; } = DateTime.Now;

        [Display(Name = "المستخدم القائم بالإدخال")]
        public string InsertedBy { get; set; }

        [Display(Name = "تاريخ التعديل")]
        public DateTime? UpdatedDate { get; set; }

        [Display(Name = "المستخدم الذي قام بالتعديل")]
        public string UpdatedBy { get; set; }

        // Navigation property (اختياري)
        [ForeignKey("FacultyMemberId")]
        public virtual FacultyMember FacultyMember { get; set; }


        [Display(Name = "رقم الموافقة الأمنية")]
        public string SecurityApprovalNumber { get; set; }

        [Display(Name = "تاريخ الموافقة الأمنية")]
        [DataType(DataType.Date)]
        public DateTime? SecurityApprovalDate { get; set; }
        [Display(Name = "تاريخ موافقة رئيس الجامعة")]
        [DataType(DataType.Date)]
        public DateTime? UniversityPresidentApprovalDate { get; set; }

        [Display(Name = "تاريخ ورقم قرار رئيس الجامعة")]
       
        public string UniversityPresidentDecision { get; set; }

    }
}
