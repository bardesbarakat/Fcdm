using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace cdm.Models
{
    public class Mission
    {
        [Key]
        public int MissionID { get; set; }

        [Required]
        public int FacultyMemberID { get; set; }
        public virtual FacultyMember FacultyMember { get; set; }

        public string MilitaryStatus { get; set; }
        public string LastQualification { get; set; }
        public string Country { get; set; }
        public string UniversityName { get; set; }
        public string MissionType { get; set; }
        public string MissionPurpose { get; set; }
        public int? MissionCurrentYear { get; set; }
        public DateTime? DepartureClearanceDate { get; set; }
        public DateTime? TravelDate { get; set; }
        public string MissionDuration { get; set; }
        public string MissionValue { get; set; }
        [Display(Name = "تاريخ موافقة رئيس الجامعة على إقامة قناة علمية")]
        [DataType(DataType.Date)]
        public DateTime? UniPresidentChannelApprovalDate { get; set; }

        public string SecurityApprovalNo { get; set; }
        public DateTime? SecurityApprovalDate { get; set; }
        public DateTime? FacultyCouncilApprovalDate { get; set; }
        public DateTime? ExecCommitteeApprovalDate { get; set; }
        public DateTime? UniversityPresidentApprovalDate { get; set; }
        public string ExecDecisionNo { get; set; }
        public DateTime? ExecDecisionDate { get; set; }

        public string UniPresidentChannelFile { get; set; }
        public string UniPresidentDecisionFile { get; set; }
        public string AccreditationMemoFile { get; set; }

        public string VacationType { get; set; }
        public string GuarantorName { get; set; }
     
        public DateTime? ReturneeFormDate { get; set; }
        public DateTime? WorkResumptionDate { get; set; }
     

        public string MainMissionType { get; set; }

        public string EnteredBy { get; set; }
        public DateTime? EntryDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        [Display(Name = "تاريخ المناقشة")]
        [DataType(DataType.Date)]
        public DateTime? DiscussionDate { get; set; }

        [Display(Name = "مدة الدراسة داخل البلاد")]
        public string StudyDurationInside { get; set; }

        [Display(Name = "مدة الدراسة خارج البلاد")]
        public string StudyDurationOutside { get; set; }

    }

}