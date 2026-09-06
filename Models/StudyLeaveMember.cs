using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cdm.Models
{
    public class StudyLeaveMember
    {
        [Key]
        public int LeaveID { get; set; }

        [Required]
        public int FacultyMemberID { get; set; }

        public string CurrentAcademicDegree { get; set; }

        public string MilitaryStatus { get; set; }
        public string DestinationCountry { get; set; }
        public string UniversityName { get; set; }
        public string LeaveType { get; set; }
        public string LastQualification { get; set; }
        public string CurrentYearOfLeave { get; set; }
        public string LeavePurpose { get; set; }
        public DateTime? ClearanceDateFromCollege { get; set; }
        public DateTime? TravelDate { get; set; }
        public string LeaveDuration { get; set; }
        public string GrantValue { get; set; }
        public string SecurityApprovalNumberAndDate { get; set; }
        public DateTime? CollegeCouncilApprovalDate { get; set; }
        public DateTime? UniversityPresidentApprovalDate { get; set; }
        public DateTime? ExecutiveCommitteeApprovalDate { get; set; }
        public string UniversityExecutiveDecision { get; set; }
        public string SalaryStatus { get; set; }
      
        public string GrantDuration { get; set; }
        public string GuarantorNameAndRelation { get; set; }

        public int DepartmentID { get; set; }

        [ForeignKey("DepartmentID")]
        public virtual Department Department { get; set; }


        public int? UserId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? UserInsertedDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? UserUpdatedDate { get; set; }

        [ForeignKey("FacultyMemberID")]
        public virtual FacultyMember FacultyMember { get; set; }


        [Display(Name = "تاريخ استمارة مبعوث عائد")]
        [DataType(DataType.Date)]
        public DateTime? ReturnMissionFormDate { get; set; }

        [Display(Name = "تاريخ استلام العمل")]
        [DataType(DataType.Date)]
        public DateTime? WorkResumptionDate { get; set; }

       

        [StringLength(255)]
        public string UniversityPresidentApprovalFilePath { get; set; }

        [StringLength(255)]
        public string ExecutiveDecisionFilePath { get; set; }
        [StringLength(255)]
        public string AdditionalFilesPath { get; set; } // مسار الملفات الإضافية

        [Display(Name = "تاريخ المناقشة")]
        [DataType(DataType.Date)]
        public DateTime? DiscussionDate { get; set; }
    }
}
