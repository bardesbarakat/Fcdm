using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cdm.Models
{
    public class DeleteLog
    {
        [Key]
        public int LogID { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; }  // مثال: "StudyLeave", "FacultyMember"

        [Required]
        public int EntityID { get; set; }       // الرقم الأساسي للعنصر المحذوف

        [StringLength(255)]
        public string EntityName { get; set; }  // الاسم أو العنوان الظاهر للعنصر المحذوف

        public string AdditionalInfo { get; set; }  // وصف إضافي أو JSON إن أحببت

        public int? DepartmentID { get; set; }

        [StringLength(255)]
        public string DepartmentName { get; set; }

        [Required]
        public int DeletedByUserId { get; set; }

        public DateTime DeletedAt { get; set; }

        [ForeignKey("DeletedByUserId")]
        public virtual User DeletedBy { get; set; } // إن كان عندك جدول Users
    }
}
                            