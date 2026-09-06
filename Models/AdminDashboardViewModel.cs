using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cdm.Models;
namespace cdm.Models
{
    public class AdminDashboardViewModel
    {
        public List<User> Users { get; set; }
        public List<string> Reports { get; set; }
        public List<Department> Departments { get; set; }
        public List<FacultyMember> FacultyMembers { get; set; } // ← لو هتستخدم أعضاء هيئة التدريس

    }

}