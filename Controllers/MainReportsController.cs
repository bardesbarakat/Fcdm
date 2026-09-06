using cdm.Database;
using cdm.Helpers;
using cdm.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Org.BouncyCastle.Pqc.Crypto.Frodo;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using static System.Data.Entity.Infrastructure.Design.Executor;


public class MainReportsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult Index()
        {
            ViewBag.Colleges = db.Colleges
                .Select(c => c.CollegeName)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            return View();
        }

    public ActionResult GetFiltersPartial(string reportType)
    {
        try
        {
            // لو التقرير محتاج قائمة كليات نحضّرها
            if (reportType == "SecondmentsDetails" ||
                reportType == "SecondmentsCounts" ||
                reportType == "AllAbroadMembers" ||
                reportType == "CurrentlyAbroad" ||
                reportType == "LeaveStatusAll")
            {
                var colleges = db.Colleges
                    .OrderBy(c => c.CollegeID)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CollegeID.ToString(),
                        Text = c.CollegeName
                    }).ToList();

                ViewBag.Colleges = colleges;
            }

            switch (reportType)
            {
                case "SecondmentsDetails": return PartialView("_SecondmentsDetailsFilters");
                case "SecondmentsCounts": return PartialView("_SecondmentsCountsFilters");
                case "AllAbroadMembers": return PartialView("_AbroadAllCombinedFilters");
                case "CurrentlyAbroad": return PartialView("_CurrentlyAbroadFilters");
                case "LeaveStatusAll": return PartialView("_LeaveStatusFilters");
                case "StatisticalSummary": return PartialView("_StatisticalSummaryFilters");
                case "ExpandedSummary": return PartialView("_ExpandedSummaryFilters"); // ✅ فلاتر فقط
                default: return Content("❌ تقرير غير معروف");
            }
        }
        catch (Exception ex)
        {
            return Content($"ERROR in GetFiltersPartial: {ex.Message}<br>{ex.InnerException?.Message}");
        }
    }


    public ActionResult Generate(string reportType, int? collegeId = null, DateTime? fromDate = null, DateTime? toDate = null, int? academicYearStart = null, int? academicYearEnd = null)
        {
            try
            {
                if (reportType == "SecondmentsDetails")
                {
                    var query = db.SecondmentsData
                        .Include(s => s.FacultyMember)
                        .AsQueryable();

                    if (collegeId.HasValue)
                        query = query.Where(s => s.FacultyMember.CollegeID == collegeId.Value);

                    if (fromDate.HasValue)
                        query = query.Where(s => s.SecondmentsStartDate >= fromDate);

                    if (toDate.HasValue)
                        query = query.Where(s => s.SecondmentsEndDate <= toDate);

                    var colleges = db.Colleges.ToDictionary(c => c.CollegeID, c => c.CollegeName);

                    var result = query.ToList().Select(s => new SecondmentReportVM
                    {
                        FullName = s.FacultyMember?.FullName,
                        NationalID = s.FacultyMember?.NationalID,
                        CollegeName = s.FacultyMember != null && colleges.ContainsKey(s.FacultyMember.CollegeID)
                            ? colleges[s.FacultyMember.CollegeID]
                            : "",
                        SecurityApprovalDate = s.SecurityApprovalDate,
                        SecondmentsStartDate = s.SecondmentsStartDate,
                        SecondmentsEndDate = s.SecondmentsEndDate,
                        SecondmentLocation = s.SecondmentLocation
                    }).ToList();

                    return PartialView("_SecondmentsDetailsReport", result);
                }

                else if (reportType == "SecondmentsCounts")
                {
                    var query = db.SecondmentsData
                        .Include(s => s.FacultyMember)
                        .Include(s => s.FacultyMember.JobTitle)
                        .Where(s => s.SecondmentsStartDate.HasValue)
                        .Where(s => s.FacultyMember != null && s.FacultyMember.JobTitle != null && s.FacultyMember.Gender != null);

                    if (collegeId.HasValue)
                        query = query.Where(s => s.FacultyMember.CollegeID == collegeId.Value);

                    if (academicYearStart.HasValue && academicYearEnd.HasValue)
                    {
                        var start = new DateTime(academicYearStart.Value, 8, 1);
                        var end = new DateTime(academicYearEnd.Value, 7, 31);
                        query = query.Where(s => s.SecondmentsStartDate >= start && s.SecondmentsStartDate <= end);
                    }

                    var grouped = query
                        .GroupBy(s => s.FacultyMember.JobTitle.JobTitleName ?? "غير محدد")
                        .Select(g => new SecondmentCountByJobVM
                        {
                            JobTitle = g.Key,
                            MaleCount = g.Count(x => x.FacultyMember.Gender == "ذكر"),
                            FemaleCount = g.Count(x => x.FacultyMember.Gender == "أنثى")
                        }).ToList();

                    ViewBag.TotalMale = grouped.Sum(g => g.MaleCount);
                    ViewBag.TotalFemale = grouped.Sum(g => g.FemaleCount);
                    ViewBag.TotalAll = grouped.Sum(g => g.Total);
                    ViewBag.Year = $"{academicYearStart}/{academicYearEnd}";
                    ViewBag.College = collegeId.HasValue
                        ? db.Colleges.FirstOrDefault(c => c.CollegeID == collegeId)?.CollegeName
                        : "جميع الكليات";

                    return PartialView("_SecondmentsCountsReport", grouped);
                }
                else if (reportType == "AllAbroadMembers")
                {
                    if (!academicYearStart.HasValue || !academicYearEnd.HasValue)
                        return Content("❌ يجب تحديد العام الجامعي.");

                    return GenerateAbroadCombinedReport(collegeId, academicYearStart.Value, academicYearEnd.Value);
                }
            else if (reportType == "CurrentlyAbroad")
            {
                var result = new List<CurrentlyAbroadFacultyVM>();

                var facultyQuery = db.FacultyMembers
                    .Include(f => f.College)
                    .Include(f => f.JobTitle)
                    .Where(f =>
                        db.SecondmentsData.Any(s => s.SecondmentsMemberId == f.FacultyMemberID && s.ReturnWorkDate == null) ||
                        db.Missions.Any(m => m.FacultyMemberID == f.FacultyMemberID && m.ReturneeFormDate == null) ||
                        db.StudyLeaveMembers.Any(l => l.FacultyMemberID == f.FacultyMemberID && l.ReturnMissionFormDate == null)
                    );

                if (collegeId.HasValue)
                {
                    facultyQuery = facultyQuery.Where(f => f.CollegeID == collegeId);
                }

                var facultyList = facultyQuery.ToList();
                int rowNum = 1;

                foreach (var f in facultyList)
                {
                    // نحاول نحدد النوع والدولة
                    string type = "";
                    string country = "";

                    var secondment = db.SecondmentsData
                        .FirstOrDefault(s => s.SecondmentsMemberId == f.FacultyMemberID && s.ReturnWorkDate == null);
                    if (secondment != null)
                    {
                        type = "إعارة";
                        country = secondment.SecondmentsCountry;
                    }

                    var mission = db.Missions
                        .FirstOrDefault(m => m.FacultyMemberID == f.FacultyMemberID && m.ReturneeFormDate == null);
                    if (mission != null)
                    {
                        type = "مهمة علمية";
                        country = mission.Country;
                    }

                    var leave = db.StudyLeaveMembers
                        .FirstOrDefault(l => l.FacultyMemberID == f.FacultyMemberID && l.ReturnMissionFormDate == null);
                    if (leave != null)
                    {
                        type = "إجازة دراسية";
                        country = leave.DestinationCountry;
                    }

                    result.Add(new CurrentlyAbroadFacultyVM
                    {
                        RowNumber = rowNum++,
                        FullName = f.FullName,
                        JobTitle = f.JobTitle?.JobTitleName,
                        DepartmentName = f.Specialization,
                        Country = country,
                        WorkType = type,
                        NationalID = f.NationalID,
                        PhoneNumber = f.PhoneNumber,
                        Email = f.Email
                    });
                }

                ViewBag.College = collegeId.HasValue
                    ? db.Colleges.FirstOrDefault(c => c.CollegeID == collegeId)?.CollegeName
                    : "جميع الكليات";

                return PartialView("_CurrentlyAbroadReport", result);
            }
            else if (reportType == "LeaveStatusAll")
            {
                var result = new List<FacultyLeaveStatusReportVM>();

                var studyLeaves = db.StudyLeaveMembers
                    .Include(l => l.FacultyMember)
                    .Include(l => l.FacultyMember.College)
                    .Include(l => l.FacultyMember.JobTitle)
                    .Include(l => l.FacultyMember.CollegeDepartment);

                if (collegeId.HasValue)
                    studyLeaves = studyLeaves.Where(l => l.FacultyMember.CollegeID == collegeId);

                result.AddRange(studyLeaves.ToList().Select((l, i) => new FacultyLeaveStatusReportVM
                {
                    RowNumber = 0,
                    CollegeName = l.FacultyMember.College?.CollegeName,
                    FullName = l.FacultyMember.FullName,
                    JobTitle = l.FacultyMember.JobTitle?.JobTitleName,
                    DepartmentName = l.FacultyMember.CollegeDepartment?.DepartmentName,

                    Country = l.DestinationCountry,
                    LeaveType = "إجازة دراسية",
                    CurrentLeaveYear = l.CurrentYearOfLeave
                }));

                // إضافة المهمات العلمية
                var missions = db.Missions
                    .Include(m => m.FacultyMember)
                    .Include(m => m.FacultyMember.College)
                    .Include(m => m.FacultyMember.JobTitle)
                    .Include(m => m.FacultyMember.CollegeDepartment);
                if (collegeId.HasValue)
                    missions = missions.Where(m => m.FacultyMember.CollegeID == collegeId);
                result.AddRange(missions.ToList().Select((m, i) => new FacultyLeaveStatusReportVM
                {
                    RowNumber = 0,
                    CollegeName = m.FacultyMember.College?.CollegeName,
                    FullName = m.FacultyMember.FullName,
                    JobTitle = m.FacultyMember.JobTitle?.JobTitleName,
                    DepartmentName = m.FacultyMember.CollegeDepartment?.DepartmentName,
                    Country = m.Country,
                    LeaveType = m.MissionType,
                    CurrentLeaveYear = m.MissionCurrentYear.ToString()
                }));

                int counter = 1;
                foreach (var item in result.OrderBy(r => r.CollegeName).ThenBy(r => r.FullName))
                    item.RowNumber = counter++;

                ViewBag.College = collegeId.HasValue
                    ? db.Colleges.FirstOrDefault(c => c.CollegeID == collegeId)?.CollegeName ?? ""
                    : "جميع الكليات";

                return PartialView("_FacultyLeaveStatusReport", result);
            }

            if (reportType == "StatisticalSummary")
            {
                var data = StatisticalSummaryGenerator.Generate(db, fromDate, toDate); // هنجهز الكلاس دا
                return PartialView("_StatisticalSummaryResult", data);
            }
            else if (reportType == "ExpandedSummary")
            {
                if (!fromDate.HasValue || !toDate.HasValue)
                    return Content("❌ يرجى اختيار (من تاريخ) و(إلى تاريخ).");

                var data = ExpandedSummaryGenerator.Generate(db, fromDate.Value, toDate.Value);
                return PartialView("_ExpandedSummaryResult", data);
            }


            return Content("❌ نوع التقرير غير معروف.");
            }
            catch (Exception ex)
            {
                return Content($"❌ Exception: {ex.Message}<br><pre>{ex.StackTrace}</pre>", "text/html");
            }
        }
    public ActionResult GenerateAbroadCombinedReport(int? collegeId, int academicYearStart, int academicYearEnd)
    {
        try
        {
            var start = new DateTime(academicYearStart, 8, 1);
            var end = new DateTime(academicYearEnd, 7, 31);

            // ================= إعــــــارات =================
            var secondmentsQ = db.SecondmentsData
                .Include(s => s.FacultyMember)
                .Include(s => s.FacultyMember.College)
                .Include(s => s.FacultyMember.JobTitle)
                .Where(s => s.SecondmentsStartDate >= start && s.SecondmentsStartDate <= end);

            if (collegeId.HasValue)
                secondmentsQ = secondmentsQ.Where(s => s.FacultyMember.CollegeID == collegeId.Value);

            var secondments = secondmentsQ
                .Where(s => s.FacultyMember != null)
                .AsNoTracking()
                .Select(s => new AbroadFacultyReportVM
                {
                    RowNumber = 0,
                    FullName = s.FacultyMember.FullName,
                    JobTitle = s.FacultyMember.JobTitle.JobTitleName,
                    CollegeName = s.FacultyMember.College.CollegeName,
                    NationalID = s.FacultyMember.NationalID,
                    Specialization = s.FacultyMember.Specialization,
                    Country = s.SecondmentsCountry,
                    WorkType = "إعارة",
                    TravelDate = s.SecondmentsStartDate,
                    ReturnDate = s.ReturnWorkDate
                })
                .ToList();

            // ============== مهمــــات علـمية ==============
            var missionsQ = db.Missions
                .Include(m => m.FacultyMember)
                .Include(m => m.FacultyMember.College)
                .Include(m => m.FacultyMember.JobTitle)
                .Include(m => m.FacultyMember.CollegeDepartment)
                .Where(m => m.TravelDate >= start && m.TravelDate <= end);

            if (collegeId.HasValue)
                missionsQ = missionsQ.Where(m => m.FacultyMember.CollegeID == collegeId.Value);

            var missions = missionsQ
                .Where(m => m.FacultyMember != null)
                .AsNoTracking()
                .Select(m => new AbroadFacultyReportVM
                {
                    RowNumber = 0,
                    FullName = m.FacultyMember.FullName,
                    JobTitle = m.FacultyMember.JobTitle.JobTitleName,
                    CollegeName = m.FacultyMember.College.CollegeName,
                    NationalID = m.FacultyMember.NationalID,
                    Specialization = m.FacultyMember.Specialization,
                    Country = m.Country,
                    WorkType = "مهمة علمية",
                    TravelDate = m.TravelDate,
                    ReturnDate = m.ReturneeFormDate
                })
                .ToList();

            // ============== إجازات دراســية ==============
            var leavesQ = db.StudyLeaveMembers
                .Include(l => l.FacultyMember)
                .Include(l => l.FacultyMember.College)
                .Include(l => l.FacultyMember.JobTitle)
                .Where(l => l.TravelDate >= start && l.TravelDate <= end);

            if (collegeId.HasValue)
                leavesQ = leavesQ.Where(l => l.FacultyMember.CollegeID == collegeId.Value);

            var leaves = leavesQ
                .Where(l => l.FacultyMember != null)
                .AsNoTracking()
                .Select(l => new AbroadFacultyReportVM
                {
                    RowNumber = 0,
                    FullName = l.FacultyMember.FullName,
                    JobTitle = l.FacultyMember.JobTitle.JobTitleName,
                    CollegeName = l.FacultyMember.College.CollegeName,
                    NationalID = l.FacultyMember.NationalID,
                    Specialization = l.FacultyMember.Specialization,
                    Country = l.DestinationCountry,
                    WorkType = "إجازة دراسية",
                    TravelDate = l.TravelDate,
                    ReturnDate = l.ReturnMissionFormDate
                })
                .ToList();

            // ============== دمج + ترتيب + ترقيم ==============
            var result = secondments
                .Concat(missions)
                .Concat(leaves)
                .OrderBy(r => r.FullName)
                .ToList();

            int i = 1;
            foreach (var row in result) row.RowNumber = i++;

            ViewBag.AcademicYear = $"{academicYearStart}/{academicYearEnd}";
            ViewBag.College = collegeId.HasValue
                ? (db.Colleges.FirstOrDefault(c => c.CollegeID == collegeId.Value)?.CollegeName ?? "")
                : "جميع الكليات";

            return PartialView("_AbroadAllCombinedReport", result);
        }
        catch (Exception ex)
        {
            return Content($"خطأ: {ex.Message}<br><pre>{ex.StackTrace}</pre>", "text/html");
        }
    }

    public ActionResult GenerateCurrentlyAbroadReport(int? collegeId)
    {
        try
        {
            var result = new List<CurrentlyAbroadFacultyVM>();

            // إعارات بالخارج بدون تاريخ عودة (وخارجية فقط TypeId=1)
            var secondments = db.SecondmentsData
                .Include(s => s.FacultyMember)
                .Include(s => s.FacultyMember.College)
                .Include(s => s.FacultyMember.JobTitle)
                .Where(s => s.ReturnWorkDate == null && s.SecondmentsTypeId == 1);

            if (collegeId.HasValue)
                secondments = secondments.Where(s => s.FacultyMember.CollegeID == collegeId.Value);

            result.AddRange(
                secondments.AsNoTracking().ToList().Select(s => new CurrentlyAbroadFacultyVM
                {
                    FullName = s.FacultyMember.FullName,
                    JobTitle = s.FacultyMember.JobTitle?.JobTitleName,
                    DepartmentName = s.FacultyMember.Specialization,   // نص عادي، مش محتاج Include
                    Country = s.SecondmentsCountry,
                    WorkType = "إعارة",
                    NationalID = s.FacultyMember.NationalID,
                    PhoneNumber = s.FacultyMember.PhoneNumber,
                    Email = s.FacultyMember.Email
                })
            );

            // مهمات علمية بالخارج بدون تاريخ عودة
            var missions = db.Missions
                .Include(m => m.FacultyMember)
                .Include(m => m.FacultyMember.College)
                .Include(m => m.FacultyMember.JobTitle)
                .Where(m => m.ReturneeFormDate == null);

            if (collegeId.HasValue)
                missions = missions.Where(m => m.FacultyMember.CollegeID == collegeId.Value);

            result.AddRange(
                missions.AsNoTracking().ToList().Select(m => new CurrentlyAbroadFacultyVM
                {
                    FullName = m.FacultyMember.FullName,
                    JobTitle = m.FacultyMember.JobTitle?.JobTitleName,
                    DepartmentName = m.FacultyMember.Specialization,
                    Country = m.Country,
                    WorkType = "مهمة علمية",
                    NationalID = m.FacultyMember.NationalID,
                    PhoneNumber = m.FacultyMember.PhoneNumber,
                    Email = m.FacultyMember.Email
                })
            );

            // إجازات دراسية بالخارج بدون تاريخ استلام عمل
            var leaves = db.StudyLeaveMembers
                .Include(l => l.FacultyMember)
                .Include(l => l.FacultyMember.College)
                .Include(l => l.FacultyMember.JobTitle)
                .Where(l => l.ReturnMissionFormDate == null);

            if (collegeId.HasValue)
                leaves = leaves.Where(l => l.FacultyMember.CollegeID == collegeId.Value);

            result.AddRange(
                leaves.AsNoTracking().ToList().Select(l => new CurrentlyAbroadFacultyVM
                {
                    FullName = l.FacultyMember.FullName,
                    JobTitle = l.FacultyMember.JobTitle?.JobTitleName,
                    DepartmentName = l.FacultyMember.Specialization,
                    Country = l.DestinationCountry,
                    WorkType = "إجازة دراسية",
                    NationalID = l.FacultyMember.NationalID,
                    PhoneNumber = l.FacultyMember.PhoneNumber,
                    Email = l.FacultyMember.Email
                })
            );

            // ترقيم وترتيب
            int counter = 1;
            foreach (var item in result.OrderBy(r => r.FullName))
                item.RowNumber = counter++;

            ViewBag.College = collegeId.HasValue
                ? (db.Colleges.FirstOrDefault(c => c.CollegeID == collegeId.Value)?.CollegeName ?? "")
                : "جميع الكليات";

            return PartialView("_CurrentlyAbroadReport", result);
        }
        catch (Exception ex)
        {
            return Content($"❌ خطأ: {ex.Message}<br><pre>{ex.StackTrace}</pre>", "text/html");
        }
    }

    public ActionResult SummaryByCollege(DateTime fromDate, DateTime toDate)
    {
        var colleges = db.Colleges.ToList();
        var result = new List<StatisticalRowVM>();

        foreach (var college in colleges)
        {
            var row = new StatisticalRowVM
            {
                CollegeName = college.CollegeName,
                MissionsCount = db.Missions.Count(m =>
               m.TravelDate >= fromDate && m.TravelDate <= toDate
 &&
                    m.FacultyMember.CollegeID == college.CollegeID),

                StudyLeavesCount = db.StudyLeaveMembers.Count(l =>
                   l.TravelDate >= fromDate && l.TravelDate <= toDate &&

                    l.FacultyMember.CollegeID == college.CollegeID),

                ScientificMissionsCount = db.ScientificMissions.Count(sm =>
           sm.TravelDate >= fromDate && sm.TravelDate <= toDate &&
           sm.FacultyMember.CollegeID == college.CollegeID),

                ConferencesCount = db.Conferences.Count(c =>
      c.InsertedDate >= fromDate && c.InsertedDate <= toDate &&
      c.FacultyMember.CollegeID == college.CollegeID),


                SecondmentsCount = db.SecondmentsData.Count(s =>
      s.SecondmentsStartDate >= fromDate && s.SecondmentsStartDate <= toDate &&
      s.FacultyMember.CollegeID == college.CollegeID),



            };

            result.Add(row);
        }

        return View(result);
    }

}

