using System.Collections.Generic;

namespace cdm.Models
{
    public class College
    {
       

        public int CollegeID { get; set; }
        public string CollegeName { get; set; }

        public virtual ICollection<FacultyMember> FacultyMembers { get; set; }
    }
}
