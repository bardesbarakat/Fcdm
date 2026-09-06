using cdm.Database;
using cdm.Helpers;
using cdm.Models;
using Microsoft.Ajax.Utilities;
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
    [Authorize]
    public class SecondmentsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Secondments/SelectType
        public ActionResult SelectType()
        {
            var types = db.SecondmentsTypes.ToList();
            return View("SelectSecondmentType", types); // عرض الكروت
        }
        public ActionResult Index(string FacultyName)
        {
            var query = db.SecondmentsData
                .Include(s => s.FacultyMember) // ضروري
                .Include(s => s.FacultyMember.JobTitle) // لو فيه رتبة علمية
                .Include(s => s.SecondmentsType) // ضروري لعرض نوع الإعارة
                 .OrderByDescending(s => s.SecondmentsStartDate) // ← هنا الترتيب
    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(FacultyName))
            {
                query = query.Where(s => s.FacultyMember.FullName.Contains(FacultyName));
            }

            var result = query
      .Select(s => new SecondmentWithRenewalVM
      {
          Secondment = s,
          HasRenewal = db.Renewals.Any(r => r.EntityType == "Secondment" && r.EntityID == s.ID),
          Renewals = db.Renewals
              .Where(r => r.EntityType == "Secondment" && r.EntityID == s.ID)
              .ToList()
      }).ToList();

            return View(result);
        }


        public ActionResult InsertData(string encryptedId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { ReturnUrl = Request.Url.PathAndQuery });
            }

            // تحميل بيانات الصفحة الخاصة بالإدخال
            ViewBag.Colleges = new SelectList(db.Colleges.ToList(), "CollegeID", "CollegeName");
            ViewBag.JobTitles = new SelectList(db.JobTitles.ToList(), "JobTitleID", "JobTitleName");

            return View();
        }

        public ActionResult InsertSecondment(int SecondmentsTypeId, int SecondmentsMemberId)
        {
            var model = new SecondmentsData
            {
                SecondmentsTypeId = SecondmentsTypeId,
                SecondmentsMemberId = SecondmentsMemberId
            };

            if (SecondmentsTypeId == 2)
            {
                model.SecondmentsCountry = "مصر"; // ← هنا نحطها تلقائيًا
            }

            ViewBag.Faculty = db.FacultyMembers
                .Include("College")
                .Include("CollegeDepartment")
                .Include("JobTitle")
                .FirstOrDefault(f => f.FacultyMemberID == SecondmentsMemberId);

         
            return View(model);
        }


        [HttpGet]
        public ActionResult InsertSecondmentForm(int id)
        {
            var secondment = db.SecondmentsData
                .Include(s => s.FacultyMember)
                .Include(s => s.SecondmentsType)
                .FirstOrDefault(s => s.ID == id);

            if (secondment == null)
            {
                TempData["Error"] = "بيانات الإعارة غير موجودة.";
                return RedirectToAction("Index");
            }

            // تعبئة ViewBag للقوائم التفصيلية
            ViewBag.Colleges = new SelectList(db.Colleges, "CollegeID", "CollegeName", secondment.FacultyMember.CollegeID);
            ViewBag.Departments = new SelectList(db.Departments, "DepartmentID", "DepartmentName", secondment.FacultyMember.CollegeDepartment);
            ViewBag.Countries = new SelectList(GetArabicCountriesList(), secondment.SecondmentsCountry);
            ViewBag.FacultyName = secondment.FacultyMember?.FullName ?? "غير معروف";
            ViewBag.TypeName = secondment.SecondmentsType?.SecondmentsName ?? "غير معروف";

            return View("InsertSecondmentForm", secondment);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InsertSecondment(SecondmentsData secondment)
        {
            if (ModelState.IsValid)
            {
                // ✅ جلب بيانات عضو هيئة التدريس
                var faculty = db.FacultyMembers
                    .Include(f => f.JobTitle)
                    .FirstOrDefault(f => f.FacultyMemberID == secondment.SecondmentsMemberId);
                ViewBag.SecondmentTypes = new SelectList(db.SecondmentsTypes, "SecondmentsTypeId", "SecondmentsTypeName", secondment.SecondmentsTypeId);

                ViewBag.Faculty = faculty;
                ViewBag.Colleges = new SelectList(db.Colleges.ToList(), "CollegeID", "CollegeName");

                ViewBag.MilitaryStatusList = new SelectList(new List<string>
        {
            "مؤجل",
            "أدى الخدمة",
            "معفى"
        }); var model = new SecondmentsData();

                if (secondment.SecondmentsTypeId == 2 )
                {
                    model.SecondmentsCountry = "مصر";
                    ViewBag.Countries = new SelectList(GetArabicCountriesList(), "مصر");
                }

                if ( secondment.SecondmentsTypeId == 3)
                {
                    model.SecondmentsCountry = "مصر";
                    ViewBag.Countries = new SelectList(GetArabicCountriesList(), "مصر");
                    model.SecurityApprovalDate = null;
                    model.SecurityApprovalNumber = null;
                   
                }
                else
                {
                    ViewBag.Countries = new SelectList(GetArabicCountriesList());
                }
                if (faculty != null)
                {
                    ViewBag.FacultyName = faculty.FullName;
                    ViewBag.JobTitle = faculty.JobTitle?.JobTitleName ?? "غير محدد";

                    // 💡 تعبئة الدرجة العلمية تلقائيًا
                 //   secondment.CurrentAcademicDegree = faculty.JobTitle?.JobTitleName;
                }
                else
                {
                    ViewBag.FacultyName = "غير محدد";
                    ViewBag.JobTitle = "غير محدد";
                }

                // القوائم المنسدلة
                ViewBag.Departments = db.Departments.ToList();
                ViewBag.DestinationCountry = new SelectList(GetArabicCountriesList(), secondment.SecondmentsCountry);

                // ✅ يعرض صفحة إدخال بيانات الإعارة: InsertData.cshtml
                return View("InsertSecondment", secondment);
            }

            // ⚠️ لو البيانات غير صالحة، يرجع لاختيار العضو
            ViewBag.Colleges = new SelectList(db.Colleges.ToList(), "CollegeID", "CollegeName");
            ViewBag.JobTitles = new SelectList(db.JobTitles.ToList(), "JobTitleID", "JobTitleName");
            ViewBag.Departments = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.SecondmentsTypes = new SelectList(db.SecondmentsTypes.ToList(), "SecondmentsID", "SecondmentsName");

            return View("InsertData", secondment);
        }

        private void PrepareSecondmentViewBags()
        {
            ViewBag.MilitaryStatusList = new SelectList(new List<string> { "مؤجل", "أدى الخدمة", "معفى" });

            var countries = GetArabicCountriesList();
            ViewBag.DestinationCountry = new SelectList(countries.Select(c => new SelectListItem
            {
                Value = c,
                Text = c
            }), "Value", "Text");

            ViewBag.Colleges = new SelectList(db.Colleges.ToList(), "CollegeID", "CollegeName");
            ViewBag.Departments = new SelectList(db.Departments.ToList(), "DepartmentID", "DepartmentName");
            ViewBag.JobTitles = new SelectList(db.JobTitles.ToList(), "JobTitleID", "JobTitleName");
            ViewBag.SecondmentsTypes = new SelectList(db.SecondmentsTypes.ToList(), "SecondmentsID", "SecondmentsName");
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

        public PartialViewResult LoadForm(int typeId)
        {
            var model = new SecondmentsData
            {
                SecondmentsTypeId = typeId
            };

            ViewBag.Colleges = new SelectList(db.Colleges, "CollegeID", "CollegeName");
            ViewBag.JobTitles = new SelectList(db.JobTitles, "JobTitleID", "JobTitleName");
            ViewBag.Countries = new SelectList(GetArabicCountriesList());
            ViewBag.FacultyMembers = new SelectList(Enumerable.Empty<SelectListItem>());

            return PartialView("_SecondmentForm", model);
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


        public ActionResult Renew(int id)
        {
            ViewBag.Colleges = new SelectList(db.Colleges.ToList(), "CollegeID", "CollegeName");

            var secondment = db.SecondmentsData
                .Include(s => s.FacultyMember)
                .Include(s => s.SecondmentsType)
                .FirstOrDefault(s => s.ID == id);

            if (secondment == null)
            {
                return HttpNotFound();
            }

            return View(secondment); // يفترض أن لديك View اسمه Renew.cshtml
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRenewal(int EntityID, string EntityType, DateTime RenewalDate, bool IsSameLocation, string NewLocation, string Notes)
        {
            if (string.IsNullOrEmpty(EntityType) || EntityID == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // استخرج معرف المستخدم الحالي من جلسة تسجيل الدخول
            int currentUserId = int.Parse(Session["UserID"].ToString()); // تأكد أن اسم المستخدم هو رقم المستخدم

            var renewal = new Renewal
            {
                EntityID = EntityID,
                EntityType = EntityType,
                RenewalDate = RenewalDate,
                IsSameLocation = IsSameLocation,
                NewLocation = IsSameLocation ? null : NewLocation,
                Notes = Notes,
                CreatedBy = currentUserId,
                CreatedAt = DateTime.Now // هذا ليس ضروريًا إذا كنت تعتمد على default(getdate()) ولكن إضافته لا يضر
            };

            db.Renewals.Add(renewal);
            db.SaveChanges();

            if (EntityType == "Secondment")
                return RedirectToAction("Index", "Secondments");

            return RedirectToAction("Index", "Home");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveSecondmentWithFiles(
     SecondmentsData model,
     HttpPostedFileBase PresidentApprovalFile,
     HttpPostedFileBase AccreditationMemoFile)
        {
            if (model.SecondmentsTypeId == 2)
            {
                model.SecondmentsCountry = "مصر";
            }

            if (!ModelState.IsValid)
            {
                ViewBag.FacultyMembers = new SelectList(db.FacultyMembers, "FacultyMemberID", "FullName", model.SecondmentsMemberId);
                return View("Create", model);
            }

            // حفظ أو تعديل
            var existing = db.SecondmentsData.Find(model.ID);
            if (existing != null)
            {
                db.Entry(existing).CurrentValues.SetValues(model);
            }
            else
            {
                model.InsertedDatetime = DateTime.Now;
                model.UserID = Convert.ToInt32(Session["UserID"]);
                db.SecondmentsData.Add(model);
            }

            db.SaveChanges(); // لحفظ ID قبل إنشاء المجلد

            var baseFolder = Path.Combine(Server.MapPath("~/uploads/secondments"), model.ID.ToString());
            Directory.CreateDirectory(baseFolder);

            // موافقة رئيس الجامعة
            if (PresidentApprovalFile != null && PresidentApprovalFile.ContentLength > 0)
            {
                var fileName = "PresidentApproval.pdf";
                var path = Path.Combine(baseFolder, fileName);
                PresidentApprovalFile.SaveAs(path);
                model.PresidentApprovalFile = $"/uploads/secondments/{model.ID}/{fileName}";
            }

            // مذكرة الاعتماد
            if (AccreditationMemoFile != null && AccreditationMemoFile.ContentLength > 0)
            {
                var fileName = "AccreditationMemo.pdf";
                var path = Path.Combine(baseFolder, fileName);
                AccreditationMemoFile.SaveAs(path);
                model.AccreditationMemo = $"/uploads/secondments/{model.ID}/{fileName}";
            }

            db.Entry(model).State = EntityState.Modified;
            db.SaveChanges();

            TempData["Success"] = "تم حفظ بيانات الإعارة والملفات بنجاح.";
            return RedirectToAction("Details", new { id = model.ID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SecondmentsData secondment)
        {
            if (ModelState.IsValid)
            {
                var original = db.SecondmentsData.FirstOrDefault(s => s.ID == secondment.ID);
                if (original == null)
                    return HttpNotFound();

                // تعيين الدولة تلقائياً لو النوع رقم 2
                if (secondment.SecondmentsTypeId == 2)
                {
                    secondment.SecondmentsCountry = "مصر";
                }

                // تحديث القيم الأساسية
                original.SecondmentsTypeId = secondment.SecondmentsTypeId;
                original.SecondmentsMemberId = secondment.SecondmentsMemberId;
                original.SecondmentsCountry = secondment.SecondmentsCountry;
                original.SecondmentsYear = secondment.SecondmentsYear;
                original.SecondmentsStartDate = secondment.SecondmentsStartDate;
                original.SecondmentsEndDate = secondment.SecondmentsEndDate;
                original.SecurityApprovalNumber = secondment.SecurityApprovalNumber;
                original.SecurityApprovalDate = secondment.SecurityApprovalDate;
                original.ClearanceDate = secondment.ClearanceDate;
                original.UniversityDecisionNumber = secondment.UniversityDecisionNumber;
                original.UniversityDecisionDate = secondment.UniversityDecisionDate;
                original.SecondmentLocation = secondment.SecondmentLocation;
                original.ReturnWorkDate = secondment.ReturnWorkDate;

                // تحديث المستخدم والتاريخ
                original.updatedDatetime = DateTime.Now;
                if (Session["UserID"] != null && int.TryParse(Session["UserID"].ToString(), out int userId))
                {
                    original.UserID = userId;
                }

                // إنشاء المجلد إن لم يكن موجودًا
                string baseFolder = Path.Combine(Server.MapPath("~/uploads/secondments"), secondment.ID.ToString());
                if (!Directory.Exists(baseFolder))
                    Directory.CreateDirectory(baseFolder);

                // حفظ ملف موافقة رئيس الجامعة (في حال تم رفع جديد)
                if (secondment.PresidentApprovalUpload != null && secondment.PresidentApprovalUpload.ContentLength > 0)
                {
                    string fileName = "PresidentApproval.pdf";
                    string path = Path.Combine(baseFolder, fileName);
                    secondment.PresidentApprovalUpload.SaveAs(path);
                    original.PresidentApprovalFile = $"/uploads/secondments/{secondment.ID}/{fileName}";
                }

                // حفظ مذكرة الاعتماد (في حال تم رفع جديد)
                if (secondment.AccreditationMemoUpload != null && secondment.AccreditationMemoUpload.ContentLength > 0)
                {
                    string fileName = "AccreditationMemo.pdf";
                    string path = Path.Combine(baseFolder, fileName);
                    secondment.AccreditationMemoUpload.SaveAs(path);
                    original.AccreditationMemo = $"/uploads/secondments/{secondment.ID}/{fileName}";
                }

                db.SaveChanges();

                TempData["Message"] = "تم حفظ التعديلات بنجاح.";
                return RedirectToAction("Index");
            }

            // لو حصلت أخطاء، أعد تحميل القوائم
            ViewBag.SecondmentsTypeId = new SelectList(
                db.SecondmentsTypes.ToList(), "SecondmentsID", "SecondmentsName", secondment.SecondmentsTypeId);

            ViewBag.Colleges = new SelectList(db.Colleges, "CollegeID", "CollegeName");
            ViewBag.JobTitles = new SelectList(db.JobTitles, "JobTitleID", "JobTitleName");
            ViewBag.Countries = new SelectList(GetArabicCountriesList(), secondment.SecondmentsCountry);
            ViewBag.FacultyMembers = new SelectList(db.FacultyMembers, "FacultyMemberID", "FullName", secondment.SecondmentsMemberId);

            return View(secondment);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            SecondmentsData model,
            HttpPostedFileBase PresidentApprovalUpload,
            HttpPostedFileBase AccreditationMemoUpload)
        {
            if (model.SecondmentsTypeId == 2)
            {
                model.SecondmentsCountry = "مصر";
            }

            if (!ModelState.IsValid)
            {
                ViewBag.FacultyMembers = new SelectList(db.FacultyMembers, "FacultyMemberID", "FullName", model.SecondmentsMemberId);
                ViewBag.SecondmentTypes = new SelectList(db.SecondmentsTypes, "SecondmentsTypeId", "SecondmentsTypeName", model.SecondmentsTypeId);
                return View(model);
            }

            model.InsertedDatetime = DateTime.Now;

            if (Session["UserID"] != null && int.TryParse(Session["UserID"].ToString(), out int uid))
            {
                model.UserID = uid;
            }

            db.SecondmentsData.Add(model);
            db.SaveChanges(); // لحفظ ID

            var baseFolder = Path.Combine(Server.MapPath("~/uploads/secondments"), model.ID.ToString());
            Directory.CreateDirectory(baseFolder);

            bool fileUpdated = false;

            // موافقة رئيس الجامعة
            if (PresidentApprovalUpload != null && PresidentApprovalUpload.ContentLength > 0)
            {
                var fileName = "PresidentApproval.pdf";
                var path = Path.Combine(baseFolder, fileName);
                PresidentApprovalUpload.SaveAs(path);
                model.PresidentApprovalFile = $"/uploads/secondments/{model.ID}/{fileName}";
                fileUpdated = true;
            }

            // مذكرة الاعتماد
            if (AccreditationMemoUpload != null && AccreditationMemoUpload.ContentLength > 0)
            {
                var fileName = "AccreditationMemo.pdf";
                var path = Path.Combine(baseFolder, fileName);
                AccreditationMemoUpload.SaveAs(path);
                model.AccreditationMemo = $"/uploads/secondments/{model.ID}/{fileName}";
                fileUpdated = true;
            }

            if (fileUpdated)
            {
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
            }

            TempData["Success"] = "تم حفظ بيانات الإعارة والملفات بنجاح.";
            return RedirectToAction("Index");
        }



        [HttpGet]
        public ActionResult Edit(int id)
        {
            var secondment = db.SecondmentsData
                               .Include("FacultyMember")
                               .Include("SecondmentsType")
                               .FirstOrDefault(s => s.ID == id);

            if (secondment == null)
                return HttpNotFound();

            var faculty = db.FacultyMembers.FirstOrDefault(f => f.FacultyMemberID == secondment.SecondmentsMemberId);

            int? selectedCollegeId = faculty?.CollegeID;
            int? selectedDeptId = faculty?.College_DepartmentID;
            int? selectedJobTitleId = faculty?.ID_JobTitle;

            ViewBag.Colleges = new SelectList(db.Colleges, "CollegeID", "CollegeName", selectedCollegeId);
            ViewBag.Departments = new SelectList(db.college_Departments.Where(d => d.CollegeID == selectedCollegeId), "college_DepartmentID", "DepartmentName", selectedDeptId);
            ViewBag.JobTitles = new SelectList(db.JobTitles, "JobTitleID", "JobTitleName", selectedJobTitleId);

            ViewBag.FacultyMembers = new SelectList(db.FacultyMembers
                .Where(f => f.CollegeID == selectedCollegeId && f.College_DepartmentID == selectedDeptId && f.ID_JobTitle == selectedJobTitleId),
                "FacultyMemberID", "FullName", secondment.SecondmentsMemberId);

            ViewBag.Countries = new SelectList(GetArabicCountriesList(), secondment.SecondmentsCountry);

            ViewBag.SecondmentsTypeId = new SelectList(
        db.SecondmentsTypes.ToList(),
        "SecondmentsID",         // ← اسم الـ ID في الجدول
        "SecondmentsName",       // ← اسم العمود اللي يعرض للمستخدم
        secondment.SecondmentsTypeId // ← القيمة المختارة
    );





            return View(secondment);
        }


        public ActionResult Details(int id)
        {
            var secondment = db.SecondmentsData
                .Include(s => s.FacultyMember.College)
                .Include(s => s.FacultyMember.CollegeDepartment)
                .Include(s => s.FacultyMember.JobTitle)
                .Include(s => s.SecondmentsType)
                .FirstOrDefault(s => s.ID == id);

            if (secondment == null)
                return HttpNotFound();

            var renewals = db.Renewals
                .Where(r => r.EntityType == "Secondment" && r.EntityID == id)
                .OrderByDescending(r => r.RenewalDate)
                .ToList();

            var vm = new SecondmentWithRenewalVM
            {
                Secondment = secondment,
                Renewals = renewals
            };

            return View(vm);
        }


        [HttpGet]
        public JsonResult GetFacultyDetails(int id)
        {
            var faculty = db.FacultyMembers.Find(id);

            if (faculty == null)
                return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                FullName = faculty.FullName,
                Email = faculty.Email,
                PhoneNumber = faculty.PhoneNumber,
                Gender = faculty.Gender,
                BirthDate = faculty.BirthDate?.ToString("yyyy-MM-dd"),  // ← الصيغة المطلوبة
                HireDate = faculty.HireDate?.ToString("yyyy-MM-dd"),
                NationalID = faculty.NationalID,
                InsuranceNo = faculty.InsuranceNumber,
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: Secondments/Delete/5
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var secondment = db.SecondmentsData
                               .Include(s => s.FacultyMember)
                               .FirstOrDefault(s => s.ID == id);

            if (secondment == null)
            {
                TempData["Error"] = "الإعارة غير موجودة.";
                return RedirectToAction("Index");
            }

            return View(secondment);
        }

        // POST: Secondments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(SecondmentsData model)
        {
            var secondment = db.SecondmentsData.Find(model.ID);

            if (secondment == null)
            {
                TempData["Error"] = "لم يتم العثور على الإعارة.";
                return RedirectToAction("Index");
            }

            db.SecondmentsData.Remove(secondment);
            db.SaveChanges();

            TempData["Success"] = "تم حذف الإعارة بنجاح.";
            return RedirectToAction("Index");
        }

        public ActionResult PrintDetails(int id)
        {
            var secondment = db.SecondmentsData
                .Include("FacultyMember.College")
                .Include("FacultyMember.CollegeDepartment")
                .Include("FacultyMember.JobTitle")
                .Include("SecondmentsType")
                .FirstOrDefault(x => x.ID == id);

            var renewals = db.Renewals
                .Where(r => r.EntityType == "Secondment" && r.EntityID == id)
                .ToList();

            var model = new SecondmentWithRenewalVM
            {
                Secondment = secondment,
                HasRenewal = renewals.Any(),
                Renewals = renewals
            };

            return View("DetailsPrint", model); // أنشئ هذا الـ View
        }
        public ActionResult ExportDetailsPdf(int id)
        {
            var secondment = db.SecondmentsData
                .Include("FacultyMember.College")
                .Include("FacultyMember.CollegeDepartment")
                .Include("FacultyMember.JobTitle")
                .Include("SecondmentsType")
                .FirstOrDefault(x => x.ID == id);

            var renewals = db.Renewals
                .Where(r => r.EntityType == "Secondment" && r.EntityID == id)
                .ToList();

            var model = new SecondmentWithRenewalVM
            {
                Secondment = secondment,
                HasRenewal = renewals.Any(),
                Renewals = renewals
            };

            return new Rotativa.ViewAsPdf("DetailsPrint", model)
            {
                FileName = $"تفاصيل-الإعارة-{secondment.FacultyMember.FullName}.pdf",
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                PageSize = Rotativa.Options.Size.A4
            };
        }

}
}
