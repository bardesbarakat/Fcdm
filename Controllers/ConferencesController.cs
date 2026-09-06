using cdm.Database;
using cdm.Helpers;
using cdm.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using X.PagedList;

namespace cdm.Controllers
{
    [Authorize]
    public class ConferencesController : Controller
    {
        private AppDbContext db = new AppDbContext();
        public ActionResult Index(string FacultyName)
        {
            var conferences = db.Conferences
                .Include(c => c.FacultyMember)
                .OrderByDescending(c => c.Id) // ترتيب تنازلي
                .AsQueryable();

            var participations = db.Participations
                .Include(p => p.FacultyMember)
                .OrderByDescending(p => p.Id) // ترتيب تنازلي
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(FacultyName))
            {
                conferences = conferences.Where(c => c.FacultyMember.FullName.Contains(FacultyName));
                participations = participations.Where(p => p.FacultyMember.FullName.Contains(FacultyName));
            }

            var vm = new ConferenceAndParticipationVM
            {
                Conferences = conferences.ToList(),
                Participations = participations.ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            var conf = db.Conferences
                         .Include(c => c.FacultyMember)
                         .FirstOrDefault(c => c.Id == id);

            if (conf == null)
                return HttpNotFound();

            return View(conf); // <-- يجب أن يكون هناك View بإسم Details.cshtml
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
                .Where(f => f.CollegeID == collegeId &&
                            f.College_DepartmentID == departmentId &&
                            f.ID_JobTitle == jobTitleId)
                .Select(f => new SelectListItem
                {
                    Value = f.FacultyMemberID.ToString(),
                    Text = f.FullName
                }).ToList();
            System.Diagnostics.Debug.WriteLine($"Received: College={collegeId}, Dept={departmentId}, Job={jobTitleId}");

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
        public ActionResult InsertData(
            Conference model,
            string Type, // ← نقرأ النوع
            HttpPostedFileBase UniversityDecisionFile,
            HttpPostedFileBase ApprovalMemoFile,
            IEnumerable<HttpPostedFileBase> OtherFiles)
        {
           

            if (ModelState.IsValid)
            {
                // بيانات أساسية
                model.InsertedDate = DateTime.Now;
                model.InsertedBy = Session["UserID"]?.ToString();
                model.ConferenceType = Type;

                switch (Type)
                {
                    case "ConferenceInternal":
                        model.ConferenceCountry = "مصر";
                        model.ConferenceType = "مؤتمر داخلي";
                        break;

                    case "ConferenceExternal":
                        model.ConferenceType = "مؤتمر خارجي";
                        break;

                    case "Workshop":
                        model.ConferenceType = "ورشة عمل";
                        model.ConferenceCountry = "مصر";
                        break;

                    case "Training":
                        model.ConferenceType = "دورة تدريبية";
                        model.ConferenceCountry = "مصر";
                        break;

                    case "Meeting":
                        model.ConferenceType = "اجتماع";
                        model.ConferenceCountry = "مصر";
                        break;

                    default:
                        model.ConferenceType = "مشاركة";
                        model.ConferenceCountry = "مصر";
                        break;
                }

                db.Conferences.Add(model);
                db.SaveChanges(); // لحفظ ID

                // إنشاء مجلد لحفظ الملفات
                string folderPath = Path.Combine(Server.MapPath("~/uploads/conferences"), model.Id.ToString());
                Directory.CreateDirectory(folderPath);

                // ملف قرار رئيس الجامعة
                if (UniversityDecisionFile != null && UniversityDecisionFile.ContentLength > 0)
                {
                    var fileName = "UniversityDecision.pdf";
                    var fullPath = Path.Combine(folderPath, fileName);
                    UniversityDecisionFile.SaveAs(fullPath);
                    model.UniversityPresidentDecisionFile = $"/uploads/conferences/{model.Id}/{fileName}";
                }

                // مذكرة الاعتماد
                if (ApprovalMemoFile != null && ApprovalMemoFile.ContentLength > 0)
                {
                    var fileName = "ApprovalMemo.pdf";
                    var fullPath = Path.Combine(folderPath, fileName);
                    ApprovalMemoFile.SaveAs(fullPath);
                    model.ApprovalMemoFile = $"/uploads/conferences/{model.Id}/{fileName}";
                }

                // ملفات أخرى
                if (OtherFiles != null && OtherFiles.Any())
                {
                    var savedFiles = new List<string>();
                    foreach (var file in OtherFiles)
                    {
                        if (file?.ContentLength > 0 && Path.GetExtension(file.FileName).ToLower() == ".pdf")
                        {
                            string uniqueName = Guid.NewGuid().ToString() + ".pdf";
                            string fullPath = Path.Combine(folderPath, uniqueName);
                            file.SaveAs(fullPath);
                            savedFiles.Add($"/uploads/conferences/{model.Id}/{uniqueName}");
                        }
                    }

                    model.OtherFiles = string.Join(";", savedFiles);
                }

                // تحديث السجل بعد حفظ المسارات
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "تم حفظ بيانات المؤتمر والملفات بنجاح.";
                return RedirectToAction("Index");
            }

            return View(model);
        }


        [HttpGet]
        public ActionResult Create(int facultyId, string type)
        {
            var faculty = db.FacultyMembers
                .Include(f => f.College)
                .Include(f => f.CollegeDepartment)
                .Include(f => f.JobTitle)
                .FirstOrDefault(f => f.FacultyMemberID == facultyId);

            if (faculty == null)
                return HttpNotFound();

            ViewBag.Faculty = faculty;
            ViewBag.FacultyId = facultyId;
            ViewBag.Type = type;
            ViewBag.FacultyName = faculty.FullName;
            ViewBag.Countries = new SelectList(GetArabicCountriesList());

            return View("create");
        }




        [HttpPost]
        public ActionResult SaveParticipation(Participation model, string type)
        {
            if (ModelState.IsValid)
            {
                model.Type = type; // مؤقتًا نحطها بالإنجليزي

                switch (type)
                {
                    case "Meeting":
                        model.Type = "اجتماع";
                      
                        break;
                    case "Training":
                        model.Type = "دورة تدريبية";
                        break;
                    case "Workshop":
                        model.Type = "ورشة عمل";
                        break;
                    case "ConferenceInternal":
                        model.Type = "مؤتمر داخلي";
                        break;
                    case "ConferenceExternal":
                        model.Type = "مؤتمر خارجي";
                        break;
                    default:
                        model.Type = "مشاركة";
                        break;
                }

                model.CreatedBy = Session["UserID"]?.ToString();
                model.CreatedAt = DateTime.Now;

                db.Participations.Add(model);
                db.SaveChanges();

                TempData["Success"] = "تم حفظ بيانات المشاركة بنجاح.";
                return RedirectToAction("Index");
            }

            return View(model);
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            var conference = db.Conferences.Include("FacultyMember").FirstOrDefault(c => c.Id == id);
            if (conference == null)
                return HttpNotFound();

            return View(conference); // View باسم Delete.cshtml
        }


        [HttpGet]
        public ActionResult Edit(int id)
        {
            var conference = db.Conferences.Include("FacultyMember").FirstOrDefault(c => c.Id == id);
            if (conference == null)
                return HttpNotFound();

            ViewBag.Countries = new SelectList(GetArabicCountriesList(), conference.ConferenceCountry);
            return View(conference);
        }
        
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            Conference model,
            HttpPostedFileBase UniversityDecisionFile,
            HttpPostedFileBase ApprovalMemoFile,
            IEnumerable<HttpPostedFileBase> OtherFiles)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Countries = new SelectList(GetArabicCountriesList(), model.ConferenceCountry);
                return View(model);
            }

            var original = db.Conferences.Find(model.Id);
            if (original == null)
                return HttpNotFound();

            // تحديث الحقول الأساسية
            original.ConferenceTitle = model.ConferenceTitle;
            original.ConferenceLocation = model.ConferenceLocation;
            original.ConferencePeriod = model.ConferencePeriod;
            original.ConferenceCountry = model.ConferenceCountry;
            original.Contribution = model.Contribution;
            original.ParticipationType = model.ParticipationType;
            original.ConferenceType = model.ConferenceType;
            original.Notes = model.Notes;
            original.UpdatedDate = DateTime.Now;
            original.UpdatedBy = Session["UserID"]?.ToString();
            // ✅ التحديث للحقول الجديدة
            original.UniversityPresidentApprovalDate = model.UniversityPresidentApprovalDate;
            original.UniversityPresidentDecision = model.UniversityPresidentDecision;

            string folderPath = Server.MapPath($"~/uploads/conferences/{model.Id}");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // ملف قرار رئيس الجامعة
            if (UniversityDecisionFile != null && UniversityDecisionFile.ContentLength > 0)
            {
                string fileName = "UniversityDecision.pdf";
                string fullPath = Path.Combine(folderPath, fileName);
                UniversityDecisionFile.SaveAs(fullPath);
                original.UniversityPresidentDecisionFile = $"/uploads/conferences/{model.Id}/{fileName}";
            }

            // مذكرة الاعتماد
            if (ApprovalMemoFile != null && ApprovalMemoFile.ContentLength > 0)
            {
                string fileName = "ApprovalMemo.pdf";
                string fullPath = Path.Combine(folderPath, fileName);
                ApprovalMemoFile.SaveAs(fullPath);
                original.ApprovalMemoFile = $"/uploads/conferences/{model.Id}/{fileName}";
            }

            // إضافة ملفات أخرى جديدة فقط (لا نحذف القديمة)
            if (OtherFiles != null && OtherFiles.Any())
            {
                var newFiles = new List<string>();
                if (!string.IsNullOrEmpty(original.OtherFiles))
                    newFiles.AddRange(original.OtherFiles.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

                foreach (var file in OtherFiles)
                {
                    if (file?.ContentLength > 0 && Path.GetExtension(file.FileName).ToLower() == ".pdf")
                    {
                        var uniqueName = Guid.NewGuid().ToString() + ".pdf";
                        var fullPath = Path.Combine(folderPath, uniqueName);
                        file.SaveAs(fullPath);
                        newFiles.Add($"/uploads/conferences/{model.Id}/{uniqueName}");
                    }
                }

                original.OtherFiles = string.Join(";", newFiles);
            }

            db.SaveChanges();

            TempData["Message"] = "تم تحديث بيانات المؤتمر بنجاح.";
            return RedirectToAction("Index");
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var conference = db.Conferences
                .Include(c => c.FacultyMember)
                .FirstOrDefault(c => c.Id == id);

            if (conference == null)
                return HttpNotFound();

            // حذف الملفات والمجلد (اختياري)
            var folderPath = Server.MapPath($"~/uploads/conferences/{id}");
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true); // حذف المجلد وما يحتويه
            }

            // سجل الحذف
            int userId = Session["UserID"] != null && int.TryParse(Session["UserID"].ToString(), out var uid) ? uid : 0;
            var log = new DeleteLog
            {
                EntityType = "Conference",
                EntityID = conference.Id,
                EntityName = conference.FacultyMember?.FullName,
                AdditionalInfo = $"العنوان: {conference.ConferenceTitle}, النوع: {conference.ConferenceType}, البلد: {conference.ConferenceCountry}",
                DepartmentID = 6,
                DepartmentName ="المؤتمرات",
                DeletedByUserId = userId,
                DeletedAt = DateTime.Now
            };
            db.DeleteLogs.Add(log);

            db.Conferences.Remove(conference);
            db.SaveChanges();

            TempData["Message"] = "تم حذف المؤتمر وتسجيل العملية بنجاح.";
            return RedirectToAction("Index");
        }

        public ActionResult DetailsPart(int id)
        {
            var participation = db.Participations
                .Include(p => p.FacultyMember)
                .FirstOrDefault(p => p.Id == id);

            if (participation == null)
                return HttpNotFound();

            return View("DetailsPart", participation);
        }


    }
}