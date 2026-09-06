using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cdm.Models
{
    public class ExpandedSummaryRowVM
    {
        public string CollegeName { get; set; }

        public int InternalMissions { get; set; }
        public int ExternalMissions { get; set; }
        public int JointSupervisionMissions { get; set; }

        public int InternalStudyLeaves { get; set; }
        public int ExternalStudyLeaves { get; set; }
        public int GrantsCount { get; set; }

        public int ScientificMissionsCount { get; set; }

        public int InternalSecondments { get; set; }
        public int ExternalSecondments { get; set; }

        public int VisitingFromInsideToOutside { get; set; }
        public int VisitingFromOutsideToInside { get; set; }
    }
}