using cdm.Database;
using cdm.Models; // غيّري حسب مكان DbContext و Models عندك
using System;
using System.Collections.Generic;
using System.Linq;

namespace cdm.Helpers
{
    public static class StatisticalSummaryGenerator
    {
        public static List<StatisticalRowVM> Generate(AppDbContext db, DateTime? from, DateTime? to)
        {
            if (from == null) from = DateTime.MinValue;
            if (to == null) to = DateTime.MaxValue;

            var colleges = db.Colleges.ToList();
            var result = new List<StatisticalRowVM>();

            foreach (var college in colleges)
            {
                var row = new StatisticalRowVM
                {
                    CollegeName = college.CollegeName,

                    MissionsCount = db.Missions.Count(m =>
                        m.TravelDate >= from && m.TravelDate <= to &&
                        m.FacultyMember.CollegeID == college.CollegeID),

                    StudyLeavesCount = db.StudyLeaveMembers.Count(l =>
                        l.TravelDate >= from && l.TravelDate <= to &&
                        l.FacultyMember.CollegeID == college.CollegeID),

                    ScientificMissionsCount = db.ScientificMissions.Count(sm =>
                        sm.TravelDate >= from && sm.TravelDate <= to &&
                        sm.FacultyMember.CollegeID == college.CollegeID),

                    ConferencesCount = db.Conferences.Count(c =>
                        c.InsertedDate >= from && c.InsertedDate <= to &&
                        c.FacultyMember.CollegeID == college.CollegeID),

                    SecondmentsCount = db.SecondmentsData.Count(s =>
                        s.SecondmentsStartDate >= from && s.SecondmentsStartDate <= to &&
                        s.FacultyMember.CollegeID == college.CollegeID)
                };

                result.Add(row);
            }

            return result;
        }

    }
}
