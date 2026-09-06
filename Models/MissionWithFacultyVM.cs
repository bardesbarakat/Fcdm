using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cdm.Models
{
    public class MissionWithFacultyVM
    {
        public Mission Mission { get; set; }
        public FacultyMember Faculty { get; set; }
    }
}