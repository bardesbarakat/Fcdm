using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace cdm.Models
{
    public class ScientificMission
    {
        [Key]
        public int MissionID { get; set; }

        [Required]
        [Display(Name = "رقم عضو هيئة التدريس")]
        public int FacultyMemberID { get; set; }

        [Display(Name = "الدرجة العلمية")]
        public string AcademicDegree { get; set; }

        [Display(Name = "الموقف من التجنيد")]
        public string MilitaryStatus { get; set; }

        [Display(Name = "الدولة")]
        public string Country { get; set; }

        [Display(Name = "مكان المهمة العلمية")]
        public string MissionLocation { get; set; }

        [Display(Name = "نوع المهمة العلمية")]
        public string MissionType { get; set; }

        [Display(Name = "الغرض من المهمة العلمية")]
        public string MissionPurpose { get; set; }

        [Display(Name = "العام الحالي للمهمة العلمية")]
        public string CurrentYearOfMission { get; set; }

        [Display(Name = "تاريخ إخلاء الطرف")]
        [DataType(DataType.Date)]
        public DateTime? ClearanceDateFromCollege { get; set; }

        [Display(Name = "تاريخ السفر")]
        [DataType(DataType.Date)]
        public DateTime? TravelDate { get; set; }

        [Display(Name = "رقم وتاريخ الموافقة الأمنية")]
        public string SecurityApprovalNumberAndDate { get; set; }

        [Display(Name = "تاريخ موافقة مجلس الكلية")]
        [DataType(DataType.Date)]
        public DateTime? CollegeCouncilApprovalDate { get; set; }

        [Display(Name = "تاريخ موافقة اللجنة التنفيذية")]
        [DataType(DataType.Date)]
        public DateTime? ExecutiveCommitteeApprovalDate { get; set; }

        [Display(Name = "تاريخ موافقة رئيس الجامعة")]
        [DataType(DataType.Date)]
        public DateTime? UniversityPresidentApprovalDate { get; set; }

        [Display(Name = "رقم وتاريخ القرار التنفيذي")]
        public string ExecutiveDecisionNumberAndDate { get; set; }

        [Display(Name = "نوع الإجازة (بمرتب / بدون مرتب)")]
        public string LeaveType { get; set; }

        [Display(Name = "اسم الضامن وصلة القرابة")]
        public string GuarantorName { get; set; }

      

        [Display(Name = "تاريخ استمارة مبعوث عائد")]
        [DataType(DataType.Date)]
        public DateTime? ReturnMissionFormDate { get; set; }

        [Display(Name = "تاريخ استلام العمل")]
        [DataType(DataType.Date)]
        public DateTime? WorkResumptionDate { get; set; }

        [Display(Name = "تاريخ الإدخال")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "تم الإدخال بواسطة")]
        public int? CreatedBy { get; set; }

        [Display(Name = "تاريخ التعديل")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "تم التعديل بواسطة")]
        public int? UpdatedBy { get; set; }

        // Navigation property (اختياري)
        [ForeignKey("FacultyMemberID")]
        public virtual FacultyMember FacultyMember { get; set; }

        [Display(Name = "ملف قرار رئيس الجامعة")]
        public string UniversityPresidentDecisionFile { get; set; }

        [Display(Name = "ملف مذكرة الاعتماد")]
        public string AccreditationMemoFile { get; set; }
       

        // لاستخدامهم في رفع الملفات (NotMapped)
        [NotMapped]
        public HttpPostedFileBase UniversityPresidentDecisionFileUpload { get; set; }

        [NotMapped]
        public HttpPostedFileBase AccreditationMemoFileUpload { get; set; }


    }
}
