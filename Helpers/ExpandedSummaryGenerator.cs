using cdm.Database;
using cdm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cdm.Helpers
{
    public static class ExpandedSummaryGenerator
    {
        public static List<ExpandedSummaryRowVM> Generate(AppDbContext db, DateTime? from, DateTime? to)
        {
            if (from == null) from = DateTime.MinValue;
            if (to == null) to = DateTime.MaxValue;

            var colleges = db.Colleges.ToList();
            var result = new List<ExpandedSummaryRowVM>();

            foreach (var college in colleges)
            {
                var row = new ExpandedSummaryRowVM
                {
                    CollegeName = college.CollegeName,

                    InternalMissions = db.Missions.Count(m =>
                        m.MainMissionType == "داخلية" &&
                        m.TravelDate >= from && m.TravelDate <= to &&
                        m.FacultyMember.CollegeID == college.CollegeID),

                    ExternalMissions = db.Missions.Count(m =>
                        m.MainMissionType == "خارجية" &&
                        m.TravelDate >= from && m.TravelDate <= to &&
                        m.FacultyMember.CollegeID == college.CollegeID),

                    JointSupervisionMissions = db.Missions.Count(m =>
                        m.MainMissionType == "إشراف مشترك" &&
                        m.TravelDate >= from && m.TravelDate <= to &&
                        m.FacultyMember.CollegeID == college.CollegeID),

                    InternalStudyLeaves = db.StudyLeaveMembers.Count(l =>
                        l.LeaveType.StartsWith("إجازة") &&
                        l.DestinationCountry == "مصر" &&
                        l.TravelDate >= from && l.TravelDate <= to &&
                        l.FacultyMember.CollegeID == college.CollegeID),

                    ExternalStudyLeaves = db.StudyLeaveMembers.Count(l =>
                        l.LeaveType.StartsWith("إجازة") &&
                        l.DestinationCountry != "مصر" &&
                        l.TravelDate >= from && l.TravelDate <= to &&
                        l.FacultyMember.CollegeID == college.CollegeID),

                    GrantsCount = db.StudyLeaveMembers.Count(l =>
                     l.LeaveType != null && l.LeaveType.Contains("منحة") &&
                        l.TravelDate >= from && l.TravelDate <= to &&
                        l.FacultyMember.CollegeID == college.CollegeID),

                    ScientificMissionsCount = db.ScientificMissions.Count(s =>
                        s.TravelDate >= from && s.TravelDate <= to &&
                        s.FacultyMember.CollegeID == college.CollegeID),

                    InternalSecondments = db.SecondmentsData.Count(s =>
                        s.SecondmentsCountry == "مصر" &&
                        s.SecondmentsStartDate >= from && s.SecondmentsStartDate <= to &&
                        s.FacultyMember.CollegeID == college.CollegeID),

                    ExternalSecondments = db.SecondmentsData.Count(s =>
                        s.SecondmentsCountry != "مصر" &&
                        s.SecondmentsStartDate >= from && s.SecondmentsStartDate <= to &&
                        s.FacultyMember.CollegeID == college.CollegeID),

                    VisitingFromInsideToOutside = db.VisitingProfessors.Count(v =>
                        v.VisitType == "من الداخل إلى الخارج" &&
                        v.UserInsertedDate >= from && v.UserInsertedDate <= to &&
                        v.FacultyMember.CollegeID == college.CollegeID),

                    VisitingFromOutsideToInside = db.VisitingProfessors.Count(v =>
                        v.VisitType == "من الخارج إلى الداخل" &&
                        v.UserInsertedDate >= from && v.UserInsertedDate <= to &&
                        v.FacultyMember.CollegeID == college.CollegeID)
                };

                result.Add(row);
            }

            return result;
        }
    }


}