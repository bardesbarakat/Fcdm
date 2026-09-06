using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cdm.Models
{
    public class StudyLeaveWithRenewalVM
    {
        
            public StudyLeaveMember StudyLeave { get; set; }
            public bool HasRenewal { get; set; }
            public List<Renewal> Renewals { get; set; } // ← جديد
       

    }
}