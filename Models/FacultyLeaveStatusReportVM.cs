using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cdm.Models
{
    public class FacultyLeaveStatusReportVM
    {
        public int RowNumber { get; set; }
        public string CollegeName { get; set; }
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public string DepartmentName { get; set; }
        public string Country { get; set; }
        public string LeaveType { get; set; } // إجازة دراسية، مهمة علمية، منحة، بعثة، متفرغ
        public string CurrentLeaveYear { get; set; }
    }

}