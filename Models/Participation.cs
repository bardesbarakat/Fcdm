using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace cdm.Models
{
    public class Participation
    {
        public int Id { get; set; }
        public string Type { get; set; }  // Workshop, Meeting, Training
        public string Title { get; set; }
        public string Location { get; set; }
        public string Period { get; set; }

        public int FacultyMemberId { get; set; }
        public virtual FacultyMember FacultyMember { get; set; }
        [Display(Name = "ملاحظات")]
        public string Notes { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

}