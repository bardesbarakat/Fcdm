using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cdm.Models
{
    public class ConferenceAndParticipationVM
    {
        public IEnumerable<Conference> Conferences { get; set; }
        public IEnumerable<Participation> Participations { get; set; }
    }

}