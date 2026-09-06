using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cdm.ViewModels
{
    public class StudyLeaveReportDTO
    {
        public int LeaveID { get; set; }
        public string FacultyMemberName { get; set; }
        public string FacultyEmail { get; set; }
        public string FacultyPhoneNumber { get; set; }
        public DateTime? FacultyHireDate { get; set; }
        public string Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string JobTitleName { get; set; }
        public string LeaveType { get; set; }
        public string DestinationCountry { get; set; }
        public string UniversityName { get; set; }
        public DateTime? TravelDate { get; set; }
        public string LeaveDuration { get; set; }
        public string LeavePurpose { get; set; }
        public string GrantValue { get; set; }
        public string GrantDuration { get; set; }
        public string UniversityExecutiveDecision { get; set; }
    }
}