using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace cdm.Models
{
    public class SecondmentsType
    {
        [Key]
        public int SecondmentsID { get; set; }

        [Required]
        [Display(Name = "نوع الإعارة")]
        public string SecondmentsName { get; set; }

        [Display(Name = "Secondment Type (EN)")]
        public string SecondmentsEnName { get; set; }

        // علاقات الربط
        public virtual ICollection<SecondmentsData> SecondmentsData { get; set; }
    }
}
