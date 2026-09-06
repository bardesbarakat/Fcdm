using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cdm.Models
{
    public class AbroadFacultyReportVM
    {
        public int RowNumber { get; set; }
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public string CollegeName { get; set; }
        public string NationalID { get; set; }
        public string Specialization { get; set; }
        public string Country { get; set; }
        public string WorkType { get; set; } // إعارة، مهمة علمية، إجازة دراسية
        public DateTime? TravelDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }

}