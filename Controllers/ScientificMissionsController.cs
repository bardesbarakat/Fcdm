using cdm.Database;
using cdm.Helpers;
using cdm.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer;
using X.PagedList;


namespace cdm.Controllers
{
    [Authorize]
    public class ScientificMissionsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult Index(string FacultyName)
        {
            var query = db.ScientificMissions
                .Include(m => m.FacultyMember)
                .Include(m => m.FacultyMember.College)
                .Include(m => m.FacultyMember.CollegeDepartment)
                .Include(m => m.FacultyMember.JobTitle)
                .OrderByDescending(m => m.MissionID)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(FacultyName))
            {
                query = query.Where(m => m.FacultyMember.FullName.Contains(FacultyName));
            }

            var result = query.ToList();
            return View(result);
        }

        public ActionResult InsertData()
        {
            if (Session["UserID"] == null || Session["DepartmentID"] == null)
            {
                TempData["Error"] = "انتهت الجلسة، الرجاء تسجيل الدخول مجددًا.";
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserID"];
            int departmentId = (int)Session["DepartmentID"];

            bool hasAccess = db.UserDepartments.Any(ud => ud.UserID == userId && ud.DepartmentID == departmentId);
            if (!hasAccess)
            {
                return RedirectToAction("NoAccess", "Account");
            }

            ViewBag.Colleges = new SelectList(db.Colleges.ToList(), "CollegeID", "CollegeName");
            ViewBag.Departments = new SelectList(db.Departments.ToList(), "DepartmentID", "DepartmentName");
            return View();
        }

        public JsonResult GetAllJobTitles()
        {
            var jobTitles = db.JobTitles
                .Select(j => new SelectListItem
                {
                    Value = j.JobTitleID.ToString(),
                    Text = j.JobTitleName
                }).ToList();

            return Json(jobTitles, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDepartmentsByCollege(int collegeId)
        {
            var departments = db.college_Departments
                .Where(d => d.CollegeID == collegeId)
                .Select(d => new SelectListItem
                {
                    Value = d.college_DepartmentID.ToString(),
                    Text = d.DepartmentName
                }).ToList();

            return Json(departments, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFacultyByDeptAndTitle(int departmentId, int jobTitleId)
        {
            var members = db.FacultyMembers
                .Where(f => f.College_DepartmentID == departmentId && f.ID_JobTitle == jobTitleId)
                .Select(f => new SelectListItem
                {
                    Value = f.FacultyMemberID.ToString(),
                    Text = f.FullName
                }).ToList();

            return Json(members, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFacultyMembers(int collegeId, int departmentId, int jobTitleId)
        {
            var facultyList = db.FacultyMembers
                .Where(f => f.CollegeID == collegeId && f.College_DepartmentID == departmentId && f.ID_JobTitle == jobTitleId)
                .Select(f => new SelectListItem
                {
                    Value = f.FacultyMemberID.ToString(),
                    Text = f.FullName
                }).ToList();

            return Json(facultyList, JsonRequestBehavior.AllowGet);
        }



        private List<string> GetArabicCountriesList()
        {
            return new List<string>
    {
        "أفغانستان", "ألبانيا", "الجزائر", "أندورا", "أنغولا", "أنتيغوا وبربودا", "الأرجنتين", "أرمينيا", "أستراليا",
        "النمسا", "أذربيجان", "البهاما", "البحرين", "بنغلاديش", "باربادوس", "بيلاروس", "بلجيكا", "بليز", "بنين", "بوتان",
        "بوليفيا", "البوسنة والهرسك", "بوتسوانا", "البرازيل", "بروناي", "بلغاريا", "بوركينا فاسو", "بوروندي", "كمبوديا",
        "الكاميرون", "كندا", "الرأس الأخضر", "جمهورية أفريقيا الوسطى", "تشاد", "تشيلي", "الصين", "كولومبيا", "جزر القمر",
        "جمهورية الكونغو", "جمهورية الكونغو الديمقراطية", "كوستاريكا", "ساحل العاج", "كرواتيا", "كوبا", "قبرص", "التشيك",
        "الدنمارك", "جيبوتي", "دومينيكا", "جمهورية الدومينيكان", "تيمور الشرقية", "الإكوادور", "مصر", "السلفادور",
        "غينيا الاستوائية", "إريتريا", "إستونيا", "إسواتيني", "إثيوبيا", "فيجي", "فنلندا", "فرنسا", "الغابون", "غامبيا",
        "جورجيا", "ألمانيا", "غانا", "اليونان", "غرينادا", "غواتيمالا", "غينيا", "غينيا بيساو", "غويانا", "هايتي",
        "هندوراس", "المجر", "آيسلندا", "الهند", "إندونيسيا", "إيران", "العراق", "إيرلندا", "إسرائيل", "إيطاليا",
        "جامايكا", "اليابان", "الأردن", "كازاخستان", "كينيا", "كيريباتي", "كوريا الشمالية", "كوريا الجنوبية", "الكويت",
        "قيرغيزستان", "لاوس", "لاتفيا", "لبنان", "ليسوتو", "ليبيريا", "ليبيا", "ليختنشتاين", "ليتوانيا", "لوكسمبورغ",
        "مدغشقر", "مالاوي", "ماليزيا", "جزر المالديف", "مالي", "مالطا", "جزر مارشال", "موريتانيا", "موريشيوس", "المكسيك",
        "ولايات ميكرونيسيا المتحدة", "مولدوفا", "موناكو", "منغوليا", "الجبل الأسود", "المغرب", "موزمبيق", "ميانمار",
        "ناميبيا", "ناورو", "نيبال", "هولندا", "نيوزيلندا", "نيكاراغوا", "النيجر", "نيجيريا", "مقدونيا الشمالية", "النرويج",
        "عمان", "باكستان", "بالاو", "بنما", "بابوا غينيا الجديدة", "باراغواي", "بيرو", "الفلبين", "بولندا", "البرتغال",
        "قطر", "رومانيا", "روسيا", "رواندا", "سانت كيتس ونيفيس", "سانت لوسيا", "سانت فنسنت والغرينادين", "ساموا",
        "سان مارينو", "ساو تومي وبرينسيبي", "السعودية", "السنغال", "صربيا", "سيشل", "سيراليون", "سنغافورة", "سلوفاكيا",
        "سلوفينيا", "جزر سليمان", "الصومال", "جنوب أفريقيا", "جنوب السودان", "إسبانيا", "سريلانكا", "السودان", "سورينام",
        "السويد", "سويسرا", "سوريا", "طاجيكستان", "تنزانيا", "تايلاند", "توغو", "تونغا", "ترينيداد وتوباغو", "تونس",
        "تركيا", "تركمانستان", "توفالو", "أوغندا", "أوكرانيا", "الإمارات العربية المتحدة", "المملكة المتحدة", "الولايات المتحدة",
        "أوروغواي", "أوزبكستان", "فانواتو", "فنزويلا", "فيتنام", "اليمن", "زامبيا", "زيمبابوي"
    };
        }
        public ActionResult Create(int facultyId, string type)
        {
            var faculty = db.FacultyMembers
                .Include(f => f.JobTitle)
                .Include(f => f.College)
                .Include(f => f.CollegeDepartment)
                .FirstOrDefault(f => f.FacultyMemberID == facultyId);

            if (faculty == null)
                return HttpNotFound();

            ViewBag.Faculty = faculty;
            ViewBag.MilitaryStatusList = new SelectList(new[] { "أدى الخدمة", "مؤجل", "معفى", "غير مطلوب" });
            ViewBag.Country = new SelectList(GetArabicCountriesList());

            var mission = new ScientificMission
            {
                FacultyMemberID = facultyId,
                MissionType = type
            };

            return View(mission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            ScientificMission model,
            HttpPostedFileBase UniversityPresidentDecisionFileUpload,
            HttpPostedFileBase AccreditationMemoFileUpload)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.CreatedAt = DateTime.Now;
            model.CreatedBy = Convert.ToInt32(Session["UserID"]);
            db.ScientificMissions.Add(model);
            db.SaveChanges();

            string baseFolder = Server.MapPath($"~/Uploads/ScientificMissions/Mission_{model.MissionID}");

            Directory.CreateDirectory(baseFolder);

            if (UniversityPresidentDecisionFileUpload != null && UniversityPresidentDecisionFileUpload.ContentLength > 0)
            {
                var fileName = "PresidentApproval.pdf";
                string filePath = Path.Combine(baseFolder, fileName);
                UniversityPresidentDecisionFileUpload.SaveAs(filePath);
                model.UniversityPresidentDecisionFile = $"/Uploads/ScientificMissions/Mission_{model.MissionID}/{fileName}";
            }

            if (AccreditationMemoFileUpload != null && AccreditationMemoFileUpload.ContentLength > 0)
            {
                var fileName = "AccreditationMemo.pdf";
                string filePath = Path.Combine(baseFolder, fileName);
                AccreditationMemoFileUpload.SaveAs(filePath);
                model.AccreditationMemoFile = $"/Uploads/ScientificMissions/Mission_{model.MissionID}/{fileName}";
            }

            db.Entry(model).State = EntityState.Modified;
            db.SaveChanges();

            TempData["Success"] = "تم حفظ المهمة العلمية والملفات بنجاح.";
            return RedirectToAction("Index");
        }

        public ActionResult Details(int id)
        {
            var mission = db.ScientificMissions
                .Include(m => m.FacultyMember.College)
                .Include(m => m.FacultyMember.CollegeDepartment)
                .Include(m => m.FacultyMember.JobTitle)
                .FirstOrDefault(m => m.MissionID == id);

            if (mission == null)
                return HttpNotFound();

            var vm = new ScientificMissionWithFacultyVM
            {
                Mission = mission,
                Faculty = mission.FacultyMember
            };

            return View(vm);
        }

     

        // POST: ScientificMissions/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int MissionID)
        {
            var mission = db.ScientificMissions
                .Include(m => m.FacultyMember)
                .FirstOrDefault(m => m.MissionID == MissionID);

            if (mission == null)
                return HttpNotFound();

            // ✅ حفظ اللوج قبل الحذف
            var log = new DeleteLog
            {
                EntityType = "ScientificMission",
                EntityID = mission.MissionID,
                EntityName = mission.FacultyMember.FullName ?? "",
                AdditionalInfo = $"  {mission.MissionType}- {mission.MissionPurpose}",
                DepartmentID =9,
                DepartmentName = "المهمات العلمية",
                DeletedByUserId = Convert.ToInt32(Session["UserID"]),
                DeletedAt = DateTime.Now
            };

            db.DeleteLogs.Add(log);
            db.ScientificMissions.Remove(mission);
            db.SaveChanges();

            TempData["Success"] = "تم حذف المهمة العلمية بنجاح.";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var mission = db.ScientificMissions
                .Include(m => m.FacultyMember.College)
                .Include(m => m.FacultyMember.CollegeDepartment)
                .Include(m => m.FacultyMember.JobTitle)
                .FirstOrDefault(m => m.MissionID == id);

            if (mission == null)
                return HttpNotFound();

            var viewModel = new ScientificMissionWithFacultyVM
            {
                Mission = mission,
                Faculty = mission.FacultyMember
            };
            ViewBag.AcademicDegreeList = new SelectList(new List<string>
{
    "معيد",
    "مدرس مساعد",
    "مدرس",
    "أستاذ مساعد", "أستاذ متفرغ", "أستاذ "
}, mission.AcademicDegree); // ✅ هذا السطر يعين القيمة المختارة تلقائياً


            ViewBag.CountryList = new SelectList(GetArabicCountriesList(), mission.Country);
            ViewBag.MilitaryStatusList = new SelectList(new List<string>
{
    "مؤجل",
    "أدى الخدمة",
    "معفى",
    "غير مطلوب"
}, mission.MilitaryStatus); // ده بيخلي القيمة المختارة تظهر تلقائيًا

            return View("Edit", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ScientificMission mission, HttpPostedFileBase UniversityPresidentDecisionFileUpload, HttpPostedFileBase AccreditationMemoFileUpload)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Country = new SelectList(GetArabicCountriesList(), mission.Country);
                ViewBag.MilitaryStatusList = new SelectList(new[] { "مؤجل", "أدى الخدمة", "معفى", "غير مطلوب" });

                var faculty = db.FacultyMembers
                                .Include(f => f.College)
                                .Include(f => f.CollegeDepartment)
                                .Include(f => f.JobTitle)
                                .FirstOrDefault(f => f.FacultyMemberID == mission.FacultyMemberID);

                var viewModel = new ScientificMissionWithFacultyVM
                {
                    Mission = mission,
                    Faculty = faculty
                };

                return View("Edit", viewModel);
            }

            var dbMission = db.ScientificMissions.Find(mission.MissionID);
            if (dbMission == null)
                return HttpNotFound();

            // تحديث البيانات
            db.Entry(dbMission).CurrentValues.SetValues(mission);

            // حفظ الملفات داخل مجلد خاص بالمهمة والعضو
            string folderPath = Server.MapPath($"~/Uploads/ScientificMission/Faculty_{mission.FacultyMemberID}_Mission_{mission.MissionID}");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            if (UniversityPresidentDecisionFileUpload != null && UniversityPresidentDecisionFileUpload.ContentLength > 0)
            {
                string path = Path.Combine(folderPath, "PresidentApproval.pdf");
                UniversityPresidentDecisionFileUpload.SaveAs(path);
                dbMission.UniversityPresidentDecisionFile = $"~/Uploads/ScientificMission/Faculty_{mission.FacultyMemberID}_Mission_{mission.MissionID}/PresidentApproval.pdf";
            }

            if (AccreditationMemoFileUpload != null && AccreditationMemoFileUpload.ContentLength > 0)
            {
                string path = Path.Combine(folderPath, "AccreditationMemo.pdf");
                AccreditationMemoFileUpload.SaveAs(path);
                dbMission.AccreditationMemoFile = $"~/Uploads/ScientificMission/Faculty_{mission.FacultyMemberID}_Mission_{mission.MissionID}/AccreditationMemo.pdf";
            }

            db.SaveChanges();
            TempData["SuccessMessage"] = "تم تعديل المهمة العلمية بنجاح.";
            return RedirectToAction("Edit", new { id = mission.MissionID });
        }
    
        
        // GET: ScientificMissions/Delete/5
   
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var mission = db.ScientificMissions
                .Include(m => m.FacultyMember)
                .FirstOrDefault(m => m.MissionID == id);

            if (mission == null)
            {
                return HttpNotFound();
            }

            return View(mission); // تأكد أن لديك View باسم Delete.cshtml
        }

    }
}
