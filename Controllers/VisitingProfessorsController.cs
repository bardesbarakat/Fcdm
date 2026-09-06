using cdm.Database;
using cdm.Helpers;
using cdm.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using X.PagedList;


namespace cdm.Controllers
{
    public class VisitingProfessorsController : Controller
    {
        private AppDbContext db = new AppDbContext();
      

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
      VisitingProfessor model,
      HttpPostedFileBase AccreditationMemoFileUpload,
      HttpPostedFileBase PresidentApprovalFileUpload)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Faculty = db.FacultyMembers
                    .Include(f => f.JobTitle)
                    .FirstOrDefault(f => f.FacultyMemberID == model.FacultyMemberID);

                ViewBag.VisitType = Request["VisitType"]; // يعرضه تاني لو حصل خطأ
                ViewBag.Countries = new SelectList(GetArabicCountriesList());
                return View(model);
            }

            model.VisitType = Request["VisitType"]; // ✅ هذا السطر يملأ القيمة من الـ hidden input
            model.UserInsertedBy = User.Identity.Name;
            model.UserInsertedDate = DateTime.Now;
            model.CreatedAt = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("FacultyMemberID: " + model.FacultyMemberID);
            db.VisitingProfessors.Add(model);
            db.SaveChanges();

            // إعداد مجلد الحفظ
            string folderPath = Server.MapPath($"~/uploads/visitingprofessors/{model.Id}/");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // حفظ ملف مذكرة الاعتماد
            if (AccreditationMemoFileUpload != null && AccreditationMemoFileUpload.ContentLength > 0)
            {
                string fileName = "AccreditationMemo_" + Path.GetFileName(AccreditationMemoFileUpload.FileName);
                string fullPath = Path.Combine(folderPath, fileName);
                AccreditationMemoFileUpload.SaveAs(fullPath);
                model.AccreditationMemoFile = $"/uploads/visitingprofessors/{model.Id}/{fileName}";
            }

            // حفظ ملف موافقة رئيس الجامعة
            if (PresidentApprovalFileUpload != null && PresidentApprovalFileUpload.ContentLength > 0)
            {
                string fileName = "PresidentApproval_" + Path.GetFileName(PresidentApprovalFileUpload.FileName);
                string fullPath = Path.Combine(folderPath, fileName);
                PresidentApprovalFileUpload.SaveAs(fullPath);
                model.PresidentApprovalFile = $"/uploads/visitingprofessors/{model.Id}/{fileName}";
            }

            // تحديث المسارات
            db.Entry(model).State = EntityState.Modified;
          
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        [HttpGet]
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
            ViewBag.Countries = new SelectList(GetArabicCountriesList());

            ViewBag.VisitType = type; // ✅ هذا هو المهم هنا

            var model = new VisitingProfessor
            {
                FacultyMemberID = faculty.FacultyMemberID,
                VisitType = type
            };

            return View(model);
        }


        public ActionResult Index(string FacultyName)
        {
            var query = db.VisitingProfessors.Include(v => v.FacultyMember).AsQueryable();

            if (!string.IsNullOrWhiteSpace(FacultyName))
            {
                query = query.Where(v => v.FacultyMember.FullName.Contains(FacultyName));
            }

            var result = query.ToList(); // لا ترجع null

            return View(result);
        }

        public ActionResult Details(int id)
        {
            var professor = db.VisitingProfessors
                .Include(v => v.FacultyMember)
                .Include(v => v.FacultyMember.College)
                .Include(v => v.FacultyMember.CollegeDepartment)
                .Include(v => v.FacultyMember.JobTitle)
                .FirstOrDefault(v => v.Id == id);

            if (professor == null)
                return HttpNotFound();

            var viewModel = new VisitingProfessorDetailsVM
            {
                VisitingProfessor = professor
            };

            return View(viewModel);
        }

      
        // [1] Edit GET
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var professor = db.VisitingProfessors
                              .Include(p => p.FacultyMember.College)
                              .Include(p => p.FacultyMember.CollegeDepartment)
                              .Include(p => p.FacultyMember.JobTitle)
                              .FirstOrDefault(p => p.Id == id);

            if (professor == null)
                return HttpNotFound();

            var vm = new VisitingProfessorDetailsVM
            {
                VisitingProfessor = professor,
                FacultyMember = professor.FacultyMember
            };
            ViewBag.Countries = new SelectList(GetArabicCountriesList(), professor.Country);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VisitingProfessorDetailsVM model, HttpPostedFileBase PresidentApprovalFile)
        {
            System.Diagnostics.Debug.WriteLine("🟢 وصلت للـ POST Edit"); // ✅ هنا
            if (!ModelState.IsValid)
            {
                ViewBag.Countries = new SelectList(GetArabicCountriesList(), model.VisitingProfessor.Country);
                TempData["Error"] = "حدث خطأ في التحقق من البيانات.";
                return View(model);
            }

            var existing = db.VisitingProfessors.Find(model.VisitingProfessor.Id);
            if (existing == null)
                return HttpNotFound();

            // تحديث الحقول
            existing.Country = model.VisitingProfessor.Country;
            existing.VisitStartDate = model.VisitingProfessor.VisitStartDate;
            existing.VisitEndDate = model.VisitingProfessor.VisitEndDate;
            existing.VisitType = model.VisitingProfessor.VisitType;

            // ✅ حفظ ملف قرار الرئيس إن وجد
            if (PresidentApprovalFile != null && PresidentApprovalFile.ContentLength > 0)
            {
                var folder = Server.MapPath($"~/uploads/visitingprofessors/{existing.Id}/");
                Directory.CreateDirectory(folder);
                var fileName = "PresidentApproval.pdf";
                var path = Path.Combine(folder, fileName);
                PresidentApprovalFile.SaveAs(path);
                existing.PresidentApprovalFile = $"/uploads/visitingprofessors/{existing.Id}/{fileName}";
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            var professor = db.VisitingProfessors
                .Include(v => v.FacultyMember)
                .FirstOrDefault(v => v.Id == id);

            if (professor == null)
                return HttpNotFound();

            return View(professor); // View اسمها Delete.cshtml
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var professor = db.VisitingProfessors
                .Include(v => v.FacultyMember)
                .FirstOrDefault(v => v.Id == id);

            if (professor == null)
                return HttpNotFound();

            // تسجيل بيانات الحذف في جدول DeleteLogs
            var log = new DeleteLog
            {
                EntityType = "VisitingProfessor",
                EntityID = professor.Id,
                EntityName = professor.FacultyMember.FullName,
                AdditionalInfo = "حذف أستاذ زائر",
                DepartmentID = 9,
                DepartmentName ="الاساتذه الزائرين",
             //   DeletedByUserId = User.Identity.Name,
                DeletedAt = DateTime.Now
            };
            db.DeleteLogs.Add(log);

            db.VisitingProfessors.Remove(professor);
            db.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}