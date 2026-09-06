using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cdm.Models
{
    public class CurrentlyAbroadReportVM
    {
        public int RowNumber { get; set; }
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public string CollegeDepartment { get; set; }
        public string Country { get; set; }
        public string LeaveType { get; set; }
        public string NationalID { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

}