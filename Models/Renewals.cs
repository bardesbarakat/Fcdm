using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cdm.Models
{
    [Table("Renewals")]
    public class Renewal
    {
     

        [Key]
        public int RenewalID { get; set; }

        [Required]
        [StringLength(50)]
        public string EntityType { get; set; } // "StudyLeave" أو "Secondment"

        [Required]
        public int EntityID { get; set; } // المعرف المرتبط بالإجازة أو الإعارة

   

        [Required]
        [DataType(DataType.Date)]
        public DateTime RenewalDate { get; set; } // تاريخ التجديد

        [Required]
        public bool IsSameLocation { get; set; } // هل نفس المكان؟ true = نعم

        [StringLength(255)]
        public string NewLocation { get; set; } // المكان الجديد في حال تغييره

        public string Notes { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User CreatedByUser { get; set; }


    }
}
