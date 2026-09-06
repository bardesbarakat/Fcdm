using cdm.Database;
using cdm.Helpers;
using cdm.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using X.PagedList;

namespace cdm.Controllers
{
    public class MissionsController : Controller
    {
        private void PopulateMilitaryStatusList()
        {
            ViewBag.MilitaryStatusList = new List<SelectListItem>
    {
        new SelectListItem { Value = "مؤجل", Text = "مؤجل" },
        new SelectListItem { Value = "أدى الخدمة", Text = "أدى الخدمة" },
        new SelectListItem { Value = "معفى", Text = "معفى" },
        new SelectListItem { Value = "غير مطلوب", Text = "غير مطلوب" },
        new SelectListItem { Value = "إعفاء مؤقت", Text = "إعفاء مؤقت" }
    };
        }

        private AppDbContext db = new AppDbContext();

        public ActionResult Index(string FacultyName)
        {
            var missions = db.Missions.Include(m => m.FacultyMember).AsQueryable();

            if (!string.IsNullOrEmpty(FacultyName))
            {
                missions = missions.Where(m => m.FacultyMember.FullName.Contains(FacultyName));
            }

            missions = missions.OrderByDescending(m => m.MissionID); // تأكيد الترتيب

            return View(missions.ToList());
        }

        public ActionResult Details(int id)
        {
            var mission = db.Missions.Find(id);
            var faculty = db.FacultyMembers.FirstOrDefault(f => f.FacultyMemberID == mission.FacultyMemberID);

            var vm = new MissionWithFacultyVM
            {
                Mission = mission,
                Faculty = faculty
            };

            return View(vm);
        }
        private string SaveFile(HttpPostedFileBase file)
        {
            var allowedExtensions = new[] { ".pdf" };
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(ext))
            {
                throw new InvalidOperationException("يُسمح فقط برفع ملفات PDF.");
            }

            string basePath = Server.MapPath("~/Uploads/Missions/");
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            var fileName = Guid.NewGuid().ToString() + ext;
            var filePath = Path.Combine(basePath, fileName);
            file.SaveAs(filePath);

            return "/Uploads/Missions/" + fileName;
        }


        [HttpGet]
        public ActionResult Create(string encryptedId, string type)
        {
            int facultyId;

            try
            {
                if (string.IsNullOrEmpty(encryptedId))
                    throw new ArgumentNullException("encryptedId", "المعرف المشفر فارغ.");

                var decodedBytes = Convert.FromBase64String(encryptedId);
                var decodedString = Encoding.UTF8.GetString(decodedBytes);

                if (!int.TryParse(decodedString, out facultyId))
                    throw new FormatException("المعرف غير صالح.");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء معالجة بيانات عضو هيئة التدريس.";
                return RedirectToAction("InsertData", new { type = type });
            }

            var faculty = db.FacultyMembers
                            .Include(f => f.JobTitle)
                            .FirstOrDefault(f => f.FacultyMemberID == facultyId);

            if (faculty == null)
                return HttpNotFound();

            PopulateMilitaryStatusList();


            ViewBag.Faculty = faculty;


            ViewBag.Country = new SelectList(GetArabicCountriesList());
            ViewBag.FacultyList = new SelectList(db.FacultyMembers, "FacultyMemberID", "FullName", facultyId);
            string missionTypeForDb = type == "ExternalMission" ? "بعثات خارجية" :
                                      type == "InternalMission" ? "بعثات داخلية" :
                                       type == "JointSupervision" ? "بعثة إشراف مشترك" :
                                      type;

            var model = new Mission
            {
                FacultyMemberID = facultyId,

                MainMissionType = missionTypeForDb
            };
            if (missionTypeForDb == "بعثات داخلية")
            {
                model.Country = "مصر";
                model.UniPresidentChannelFile = null; // أو ignore field لو مش مستخدم
                model.UniPresidentChannelApprovalDate = null;
            }

            if (missionTypeForDb == "بعثات خارجية")
            {

                model.UniPresidentChannelFile = null; // أو ignore field لو مش مستخدم
                model.UniPresidentChannelApprovalDate = null;
            }// إذا كان نوع البعثة ليس داخلية أو خارجية فقط، سجّل تاريخ القناة
          

            return View(model);
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

            return View(); // ✅ تفتح View باسم InsertData.cshtml
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
        [ValidateAntiForgeryToken]
        public ActionResult Create(Mission mission,
       HttpPostedFileBase UniPresidentChannelFileUpload,
       HttpPostedFileBase UniPresidentDecisionFileUpload,
       HttpPostedFileBase AccreditationMemoFileUpload)
        {
            if (!ModelState.IsValid)
            {
                // إعادة تعبئة القوائم عند حدوث خطأ
                PopulateMilitaryStatusList();

                ViewBag.Country = GetArabicCountriesList().Select(c => new SelectListItem
                {
                    Value = c,
                    Text = c
                });

                var faculty = db.FacultyMembers
                    .Include(f => f.JobTitle)
                    .Include(f => f.College)
                    .Include(f => f.CollegeDepartment)
                    .FirstOrDefault(f => f.FacultyMemberID == mission.FacultyMemberID);

                ViewBag.Faculty = faculty;
                ViewBag.Type = mission.MainMissionType;

                TempData["Error"] = "البيانات غير مكتملة.";
                return View(mission);
            }

            var allowedExtensions = new[] { ".pdf" };
            string basePath = Server.MapPath("~/Uploads/Missions/" + mission.FacultyMemberID);

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            // ✅ حفظ الملفات إن وجدت
            if (UniPresidentChannelFileUpload != null && UniPresidentChannelFileUpload.ContentLength > 0)
            {
                var ext = Path.GetExtension(UniPresidentChannelFileUpload.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("", "يُسمح فقط برفع ملفات PDF.");
                    return View(mission);
                }
                var fileName = "UniPresidentChannel_" + Guid.NewGuid() + ext;
                var filePath = Path.Combine(basePath, fileName);
                UniPresidentChannelFileUpload.SaveAs(filePath);
                mission.UniPresidentChannelFile = "/Uploads/Missions/" + mission.FacultyMemberID + "/" + fileName;
            }

            if (UniPresidentDecisionFileUpload != null && UniPresidentDecisionFileUpload.ContentLength > 0)
            {
                var ext = Path.GetExtension(UniPresidentDecisionFileUpload.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("", "يُسمح فقط برفع ملفات PDF.");
                    return View(mission);
                }
                var fileName = "UniPresidentDecision_" + Guid.NewGuid() + ext;
                var filePath = Path.Combine(basePath, fileName);
                UniPresidentDecisionFileUpload.SaveAs(filePath);
                mission.UniPresidentDecisionFile = "/Uploads/Missions/" + mission.FacultyMemberID + "/" + fileName;
            }

            if (AccreditationMemoFileUpload != null && AccreditationMemoFileUpload.ContentLength > 0)
            {
                var ext = Path.GetExtension(AccreditationMemoFileUpload.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("", "يُسمح فقط برفع ملفات PDF.");
                    return View(mission);
                }
                var fileName = "AccreditationMemo_" + Guid.NewGuid() + ext;
                var filePath = Path.Combine(basePath, fileName);
                AccreditationMemoFileUpload.SaveAs(filePath);
                mission.AccreditationMemoFile = "/Uploads/Missions/" + mission.FacultyMemberID + "/" + fileName;
            }

            // ✅ التعامل مع القناة العلمية حسب نوع البعثة
            if (mission.MainMissionType == "بعثات داخلية" || mission.MainMissionType == "بعثات خارجية")
            {
                mission.UniPresidentChannelApprovalDate = null;
                mission.UniPresidentChannelFile = null;
            }

            // ✅ بيانات الإدخال
            mission.EntryDate = DateTime.Now;
            mission.EnteredBy = Session["UserID"]?.ToString();

            db.Missions.Add(mission);
            db.SaveChanges();

            TempData["Success"] = "تم حفظ البعثة بنجاح.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var mission = db.Missions
               .Include(m => m.FacultyMember)
               

                .FirstOrDefault(l => l.MissionID == id);

            if (mission == null)
                return HttpNotFound();

            return View(mission); // تأكد أن لديك View اسمه Delete.cshtml
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var mission = db.Missions
                .Include(m => m.FacultyMember.CollegeDepartment)
              
                .FirstOrDefault(m => m.MissionID == id);

            if (mission == null)
            {
                return HttpNotFound();
            }
            int userId = Session["UserID"] != null && int.TryParse(Session["UserID"].ToString(), out var uid) ? uid : 0;
            // إنشاء سجل في DeleteLogs
            var log = new DeleteLog
            {
                EntityType = "Mission",
                EntityID = mission.MissionID,
                EntityName = mission.FacultyMember.FullName,
                AdditionalInfo = $"بعثة إلى: {mission.Country}، الغرض: {mission.MissionPurpose}",
                DepartmentID = 4,
                DepartmentName = "البعثات",
                DeletedByUserId = userId,
                DeletedAt = DateTime.Now
            };

            db.DeleteLogs.Add(log);
            db.Missions.Remove(mission);
            db.SaveChanges();

            TempData["Success"] = "تم حذف البعثة وتسجيل العملية في السجلات.";
            return RedirectToAction("Index");
        }


        public ActionResult Edit(int id)
        {
            var mission = db.Missions.Include("FacultyMember")
                                     .FirstOrDefault(m => m.MissionID == id);

            if (mission == null)
            {
                return HttpNotFound();
            }
          
            var viewModel = new MissionWithFacultyVM
            {
                Mission = mission,
                Faculty = mission.FacultyMember
            };
  ViewBag.CountryList = GetArabicCountriesList()
    .Select(c => new SelectListItem { Value = c, Text = c })
    .ToList();

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Mission mission, HttpPostedFileBase UniPresidentDecisionFileUpload, HttpPostedFileBase AccreditationMemoFileUpload, HttpPostedFileBase UniPresidentChannelFileUpload)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CountryList = GetArabicCountriesList();

                var faculty = db.FacultyMembers
                                .Include(f => f.College)
                                .Include(f => f.CollegeDepartment)
                                .Include(f => f.JobTitle)
                                .FirstOrDefault(f => f.FacultyMemberID == mission.FacultyMemberID);

                var viewModel = new MissionWithFacultyVM
                {
                    Mission = mission,
                    Faculty = faculty
                };

                return View("Edit", viewModel);
            }

            var dbMission = db.Missions.Find(mission.MissionID);
            if (dbMission == null) return HttpNotFound();

            // تحديث البيانات
            db.Entry(dbMission).CurrentValues.SetValues(mission);

            // ملفات جديدة إن وُجدت
            if (UniPresidentDecisionFileUpload != null)
            {
                var path = SaveFile(UniPresidentDecisionFileUpload);
                dbMission.UniPresidentDecisionFile = path;
            }

            if (AccreditationMemoFileUpload != null)
            {
                var path = SaveFile(AccreditationMemoFileUpload);
                dbMission.AccreditationMemoFile = path;
            }

            if (UniPresidentChannelFileUpload != null)
            {
                var path = SaveFile(UniPresidentChannelFileUpload);
                dbMission.UniPresidentChannelFile = path;
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }


    }
}