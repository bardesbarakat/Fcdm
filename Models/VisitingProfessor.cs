using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cdm.Models
{
    public class VisitingProfessor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "عضو هيئة التدريس")]
        public int FacultyMemberID { get; set; }

        [Display(Name = "الدولة")]
        public string Country { get; set; }

        [Display(Name = "تاريخ بداية الزيارة")]
        [DataType(DataType.Date)]
        public DateTime? VisitStartDate { get; set; }

        [Display(Name = "تاريخ نهاية الزيارة")]
        [DataType(DataType.Date)]
        public DateTime? VisitEndDate { get; set; }

        [Display(Name = "الغرض من الزيارة")]
        public string VisitPurpose { get; set; }

        [Display(Name = "رقم وتاريخ الموافقة الأمنية")]
        public string SecurityApprovalNoAndDate { get; set; }

        [Display(Name = "تاريخ موافقة مجلس الكلية")]
        [DataType(DataType.Date)]
        public DateTime? CollegeCouncilApprovalDate { get; set; }

        [Display(Name = "تاريخ موافقة رئيس الجامعة")]
        [DataType(DataType.Date)]
        public DateTime? UniversityPresidentApprovalDate { get; set; }

        [Display(Name = "رقم وتاريخ القرار التنفيذي")]
        public string ExecutiveDecisionNoAndDate { get; set; }

        [Display(Name = "اسم الجامعة المسجل بها الزائر")]
        public string VisitorUniversityName { get; set; }

        [Display(Name = "الكلية المضيفة للزائر")]
        public string HostCollege { get; set; }

        [Display(Name = "ملف مذكرة الاعتماد")]
        public string AccreditationMemoFile { get; set; }

        [Display(Name = "ملف موافقة رئيس الجامعة")]
        public string PresidentApprovalFile { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "آخر تعديل")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "أنشئ بواسطة")]
        public string UserInsertedBy { get; set; }

        [Display(Name = "تاريخ الإدخال")]
        public DateTime? UserInsertedDate { get; set; } = DateTime.Now;

        [Display(Name = "عدّل بواسطة")]
        public string UserUpdatedBy { get; set; }

        [Display(Name = "تاريخ التعديل")]
        public DateTime? UserUpdatedDate { get; set; }
        // ✅ الجديد
        public string VisitType { get; set; }

        // Navigation property
        public virtual FacultyMember FacultyMember { get; set; }
    }
}
