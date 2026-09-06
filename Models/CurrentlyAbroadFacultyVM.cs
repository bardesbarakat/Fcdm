using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cdm.Models
{
    public class CurrentlyAbroadFacultyVM
    {
        public int RowNumber { get; set; }
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public string DepartmentName { get; set; }
        public string Country { get; set; }
        public string WorkType { get; set; } // إعارة - مهمة علمية - إجازة دراسية
        public string NationalID { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

}