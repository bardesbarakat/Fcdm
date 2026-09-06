using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cdm.Models
{

    public class SecondmentReportVM
    {
        public string FullName { get; set; }
        public string NationalID { get; set; }
        public string CollegeName { get; set; }

        public DateTime? SecurityApprovalDate { get; set; }
        public DateTime? SecondmentsStartDate { get; set; }
        public DateTime? SecondmentsEndDate { get; set; }

        public string SecondmentLocation { get; set; }
    }

}