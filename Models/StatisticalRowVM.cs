using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cdm.Models
{
    public class StatisticalRowVM
    {
        public string CollegeName { get; set; }
        public int MissionsCount { get; set; }
        public int StudyLeavesCount { get; set; }
        public int ScientificMissionsCount { get; set; }
        public int ConferencesCount { get; set; }
        public int SecondmentsCount { get; set; }
        public int Total => MissionsCount + StudyLeavesCount + ScientificMissionsCount + ConferencesCount + SecondmentsCount;
       
    }

}