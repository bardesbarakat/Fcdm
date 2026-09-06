using System.Collections.Generic;

namespace cdm.Areas.DataManager.Models
{
    // DTOs خفيفة داخل الـArea – لا تعتمد على موديلات المشروع الأصلي
    public class CollegeLite
    {
        public int CollegeID { get; set; }
        public string CollegeName { get; set; }
  
    }

    public class DepartmentLite
    {
        public int College_DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public int CollegeID { get; set; }
    }
    public class JobTitleLite
    {
        public int JobTitleID { get; set; }
        public string JobTitleName { get; set; }
        public string JobTitleNameEN { get; set; }
    }

    public class FacultyExcelUploadVM
    {
        public List<CollegeLite> Colleges { get; set; } = new List<CollegeLite>();
        public List<DepartmentLite> Departments { get; set; } = new List<DepartmentLite>();
        public List<JobTitleLite> JobTitles { get; set; } = new List<JobTitleLite>();

    }
}
