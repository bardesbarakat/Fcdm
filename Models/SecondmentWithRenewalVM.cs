using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cdm.Models
{
    public class SecondmentWithRenewalVM
    {
        public SecondmentsData Secondment { get; set; }
        public bool HasRenewal { get; set; }
        public List<Renewal> Renewals { get; set; }
    }
}