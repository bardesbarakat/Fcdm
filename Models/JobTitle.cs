using System.Collections.Generic;

namespace cdm.Models
{
    public class JobTitle
    {
        public int JobTitleID { get; set; }
        //public string TitleName { get; set; }
        public string JobTitleName { get; internal set; }

        // إذا كان مربوطًا بأعضاء هيئة التدريس:
        public virtual ICollection<FacultyMember> FacultyMembers { get; set; }
    }

}