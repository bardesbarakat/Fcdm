using cdm.Database;
using cdm.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace cdm.Controllers
{
    [Authorize]
    public class StudyLeaveMembersController : Controller
    {
        private AppDbContext db = new AppDbContext();

       

        public ActionResult SelectFacultyForStudyLeave()
        {
            ViewBag.Colleges = new SelectList(db.Colleges.ToList(), "CollegeID", "CollegeName");
            ViewBag.FacultyMembers = new SelectList(db.FacultyMembers.ToList(), "FacultyMemberID", "FullName");
            return View(); 

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InsertStudyLeave(StudyLeaveMember studyLeaveMember)
        {
            if (ModelState.IsValid)
            {
                // ✅ هات بيانات عضو هيئة التدريس عشان تظهر في الصفحة التالية
                var faculty = db.FacultyMembers
                .Include(f => f.JobTitle)
            .FirstOrDefault(f => f.FacultyMemberID == studyLeaveMember.FacultyMemberID);


                ViewBag.Faculty = faculty;


              
                ViewBag.MilitaryStatusList = new SelectList(new List<string>
        {
            "مؤجل",
            "أدى الخدمة",
            "معفى"
        });

             

                    if (faculty != null)
                {
                    ViewBag.FacultyName = faculty.FullName;
                   //ViewBag.de = faculty.FullName;
                    ViewBag.JobTitle = faculty.JobTitle?.JobTitleName ?? "غير محدد";
                  
                    // 💡 تعبئة الدرجة العلمية الحالية تلقائيًا
                    studyLeaveMember.CurrentAcademicDegree = faculty.JobTitle?.JobTitleName;
                   
                }
                else
                {
                    ViewBag.FacultyName = "غير محدد";
                    ViewBag.JobTitle = "غير محدد";
                }
                ViewBag.Departments = db.Departments.ToList(); // جدول الأقسام الرئيسي
                ViewBag.DestinationCountry = new SelectList(GetArabicCountriesList(), studyLeaveMember.DestinationCountry);

                // ✅ يروح يعرض صفحة إدخال البيانات الفعلية: InsertStudyLeave.cshtml
                return View("InsertStudyLeave", studyLeaveMember);
            }

            // ⚠️ لو ModelState مش صحيح، يرجع لنفس الصفحة عشان يكمل
            ViewBag.Colleges = new SelectList(db.Colleges.ToList(), "CollegeID", "CollegeName");
            ViewBag.JobTitles = new SelectList(db.JobTitles.ToList(), "JobTitleID", "JobTitleName");
            ViewBag.Departments = new SelectList(Enumerable.Empty<SelectListItem>());

            return View("InsertData", studyLeaveMember);
        }


        [HttpGet]
        public ActionResult InsertStudyLeave(int facultyMemberId)
        {
            var faculty = db.FacultyMembers
                            .Include(f => f.JobTitle)
                            .FirstOrDefault(f => f.FacultyMemberID == facultyMemberId);

            ViewBag.MilitaryStatusList = new SelectList(new List<string>
    {
        "مؤجل",
        "أدى الخدمة",
        "معفى"
    });
            var countries = GetArabicCountriesList();

            // أنشئ SelectList بشكل صريح
            ViewBag.DestinationCountry = new SelectList(
                countries.Select(c => new SelectListItem
                {
                    Value = c,
                    Text = c
                }),
                "Value",
                "Text"
            );

          

            if (faculty != null)
            {
                ViewBag.FacultyName = faculty.FullName;
                ViewBag.JobTitle = faculty.JobTitle?.JobTitleName ?? "غير محدد";
               
                var studyLeave = new StudyLeaveMember
                {
                    FacultyMemberID = faculty.FacultyMemberID,
                 DepartmentID =3,
                    CurrentAcademicDegree = faculty.JobTitle?.JobTitleName
                };

                return View(studyLeave);  // هيعرض الصفحة InsertStudyLeave.cshtml
            }

            // لو العضو مش موجود
            TempData["Error"] = "عضو هيئة التدريس غير موجود";
            return RedirectToAction("Index");
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

        public ActionResult DetailsPartial(int id)
        {

            var leave = db.StudyLeaveMembers
       .Include(x => x.FacultyMember.College)
       .Include(x => x.FacultyMember.CollegeDepartment)
       .FirstOrDefault(x => x.LeaveID == id);

            // بعد التأكد من وجود الإجازة
            if (leave == null)
                return HttpNotFound();

            // جلب التجديدات يدويًا وربطها مع ViewModel
            var viewModel = new StudyLeaveWithRenewalVM
            {
                StudyLeave = leave,
                HasRenewal = db.Renewals.Any(r => r.EntityID == leave.LeaveID && r.EntityType == "StudyLeave"),
                Renewals = db.Renewals
                    .Where(r => r.EntityID == leave.LeaveID && r.EntityType == "StudyLeave")
                    .ToList()
            };

            return View("DetailsPartial", viewModel);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveStudyLeaveWithFiles(
         StudyLeaveMember model,
         HttpPostedFileBase UniversityPresidentApprovalFile,
         HttpPostedFileBase ExecutiveDecisionFile,
         IEnumerable<HttpPostedFileBase> AdditionalFiles)
        {
            // التحقق من الحقول النصية المطلوبة
            if (string.IsNullOrWhiteSpace(model.MilitaryStatus))
                ModelState.AddModelError("MilitaryStatus", "الموقف من التجنيد مطلوب");

            if (string.IsNullOrWhiteSpace(model.DestinationCountry))
                ModelState.AddModelError("DestinationCountry", "الدولة مطلوبة");

            if (string.IsNullOrWhiteSpace(model.UniversityName))
                ModelState.AddModelError("UniversityName", "اسم الجامعة مطلوب");

            if (string.IsNullOrWhiteSpace(model.LeaveType))
                ModelState.AddModelError("LeaveType", "نوع الإجازة مطلوب");

            if (string.IsNullOrWhiteSpace(model.LastQualification))
                ModelState.AddModelError("LastQualification", "آخر مؤهل مطلوب");

            if (string.IsNullOrWhiteSpace(model.CurrentYearOfLeave))
                ModelState.AddModelError("CurrentYearOfLeave", "العام الحالي للإجازة مطلوب");

            if (string.IsNullOrWhiteSpace(model.LeavePurpose))
                ModelState.AddModelError("LeavePurpose", "الغرض من الإيفاد مطلوب");

            if (!model.ClearanceDateFromCollege.HasValue)
                ModelState.AddModelError("ClearanceDateFromCollege", "تاريخ إخلاء الطرف مطلوب");

            if (!model.TravelDate.HasValue)
                ModelState.AddModelError("TravelDate", "تاريخ السفر مطلوب");

            if (string.IsNullOrWhiteSpace(model.LeaveDuration))
                ModelState.AddModelError("LeaveDuration", "مدة الإيفاد مطلوبة");

            if (string.IsNullOrWhiteSpace(model.GrantValue))
                ModelState.AddModelError("GrantValue", "قيمة المنحة مطلوبة");

            if (string.IsNullOrWhiteSpace(model.SecurityApprovalNumberAndDate))
                ModelState.AddModelError("SecurityApprovalNumberAndDate", "رقم وتاريخ الموافقة الأمنية مطلوب");

            if (!model.CollegeCouncilApprovalDate.HasValue)
                ModelState.AddModelError("CollegeCouncilApprovalDate", "تاريخ موافقة مجلس الكلية مطلوب");

            if (!model.UniversityPresidentApprovalDate.HasValue)
                ModelState.AddModelError("UniversityPresidentApprovalDate", "تاريخ موافقة رئيس الجامعة مطلوب");

            if (!model.ExecutiveCommitteeApprovalDate.HasValue)
                ModelState.AddModelError("ExecutiveCommitteeApprovalDate", "تاريخ موافقة اللجنة التنفيذية مطلوب");

            if (string.IsNullOrWhiteSpace(model.UniversityExecutiveDecision))
                ModelState.AddModelError("UniversityExecutiveDecision", "رقم وتاريخ القرار التنفيذي مطلوب");

            if (string.IsNullOrWhiteSpace(model.SalaryStatus))
                ModelState.AddModelError("SalaryStatus", "نوع الإجازة (بمرتب / بدون مرتب) مطلوب");

            if (string.IsNullOrWhiteSpace(model.GuarantorNameAndRelation))
                ModelState.AddModelError("GuarantorNameAndRelation", "اسم الضامن وصلة القرابة مطلوبة");

            // التحقق من الملفات المطلوبة
            if (string.IsNullOrEmpty(model.UniversityPresidentApprovalFilePath) && UniversityPresidentApprovalFile == null)
                ModelState.AddModelError("UniversityPresidentApprovalFilePath", "يرجى رفع ملف موافقة رئيس الجامعة (PDF)");

            if (string.IsNullOrEmpty(model.ExecutiveDecisionFilePath) && ExecutiveDecisionFile == null)
                ModelState.AddModelError("ExecutiveDecisionFilePath", "يرجى رفع ملف القرار التنفيذي (PDF)");

            if (!ModelState.IsValid)
            {
                ViewBag.DestinationCountry = new SelectList(GetArabicCountriesList(), "Value", "Text", model.DestinationCountry);
                return View("Edit", model);
            }


            // حفظ أو تعديل
            var existing = db.StudyLeaveMembers.Find(model.LeaveID);
            if (existing != null)
            {
                db.Entry(existing).CurrentValues.SetValues(model);
            }
            else
            {
                model.UserInsertedDate = DateTime.Now;
                model.UserId = Convert.ToInt32(Session["UserID"]);
                model.DepartmentID = 3;
                db.StudyLeaveMembers.Add(model);
            }

            db.SaveChanges();

            var baseFolder = Path.Combine(Server.MapPath("~/uploads/studyLeaves"), model.LeaveID.ToString());
            Directory.CreateDirectory(baseFolder);

            // موافقة رئيس الجامعة
            if (UniversityPresidentApprovalFile != null && UniversityPresidentApprovalFile.ContentLength > 0)
            {
                var fileName = "UniversityPresidentApproval.pdf";
                var path = Path.Combine(baseFolder, fileName);
                UniversityPresidentApprovalFile.SaveAs(path);
                model.UniversityPresidentApprovalFilePath = $"/uploads/studyLeaves/{model.LeaveID}/{fileName}";
            }

            // القرار التنفيذي
            if (ExecutiveDecisionFile != null && ExecutiveDecisionFile.ContentLength > 0)
            {
                var fileName = "ExecutiveDecision.pdf";
                var path = Path.Combine(baseFolder, fileName);
                ExecutiveDecisionFile.SaveAs(path);
                model.ExecutiveDecisionFilePath = $"/uploads/studyLeaves/{model.LeaveID}/{fileName}";
            }

            // الملفات الإضافية (اختيارية)
            if (AdditionalFiles != null && AdditionalFiles.Any())
            {
                var savedFiles = new List<string>();
                foreach (var file in AdditionalFiles)
                {
                    if (file?.ContentLength > 0 && Path.GetExtension(file.FileName).ToLower() == ".pdf")
                    {
                        var uniqueName = Guid.NewGuid().ToString() + ".pdf";
                        var fullPath = Path.Combine(baseFolder, uniqueName);
                        file.SaveAs(fullPath);
                        savedFiles.Add($"/uploads/studyLeaves/{model.LeaveID}/{uniqueName}");
                    }
                }

                model.AdditionalFilesPath = string.Join(";", savedFiles);
            }

            db.Entry(model).State = EntityState.Modified;
            db.SaveChanges();

            TempData["Success"] = "تم حفظ البيانات والملفات بنجاح.";
            return RedirectToAction("DetailsPartial", new { id = model.LeaveID });
        }






        public ActionResult SearchFacultyByCollegeDeptTitle()
        {
            ViewBag.Colleges = new SelectList(db.Colleges.ToList(), "CollegeID", "CollegeName");
            return View();
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

        public JsonResult GetFacultyDetails(int id)
        {
            var member = db.FacultyMembers.FirstOrDefault(f => f.FacultyMemberID == id);

            if (member == null)
                return Json(null, JsonRequestBehavior.AllowGet);

            var result = new
            {
                member.FullName,
                member.Email,
                member.PhoneNumber,
                member.NationalID,
                HireDate = member.HireDate.HasValue ? member.HireDate.Value.ToString("yyyy-MM-dd") : ""
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public FileResult PrintFacultyPdf(int id)
        {
            var member = db.FacultyMembers.FirstOrDefault(f => f.FacultyMemberID == id);
            if (member == null)
                return null;

            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 50, 50, 60, 50);
                PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                BaseFont bf = BaseFont.CreateFont("C:/Windows/Fonts/arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                //bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font titleFont = new Font(bf, 16, Font.BOLD);
                Font labelFont = new Font(bf, 12, Font.BOLD);
                Font valueFont = new Font(bf, 12);

                // عنوان
                Paragraph title = new Paragraph("نموذج بيانات عضو هيئة تدريس", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                doc.Add(title);

                // جدول RTL
                PdfPTable table = new PdfPTable(2);
                table.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2f, 1f });

                void AddRow(string label, string value)
                {
                    PdfPCell cell1 = new PdfPCell(new Phrase(value, valueFont));
                    PdfPCell cell2 = new PdfPCell(new Phrase(label, labelFont));
                    cell1.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    cell2.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    cell1.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell2.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell1.UseAscender = true;
                    cell2.UseAscender = true;
                    table.AddCell(cell1);
                    table.AddCell(cell2);
                }

                AddRow(member.FullName ?? "", "الاسم");
                AddRow(member.Email ?? "", "البريد الإلكتروني");
                AddRow(member.PhoneNumber ?? "", "رقم الهاتف");
                AddRow(member.NationalID ?? "", "الرقم القومي");
                AddRow(member.HireDate?.ToString("yyyy-MM-dd") ?? "", "تاريخ التعيين");

                doc.Add(table);

                // سطر توقيع
                doc.Add(new Paragraph("\n\n"));
                Paragraph sign = new Paragraph("توقيع المسؤول: ..........................................", valueFont);
                sign.Alignment = Element.ALIGN_RIGHT;
                sign.SpacingBefore = 30;
                doc.Add(sign);

                doc.Close();
                return File(ms.ToArray(), "application/pdf", "FacultyMember_" + id + ".pdf");
            }
        }

        public ActionResult AddStudyLeave()
        {
            using (var db = new AppDbContext())
            {
                ViewBag.Colleges = new SelectList(db.Colleges.ToList(), "CollegeID", "CollegeName");
            }

            return View();
        }

        //public ActionResult Index(string countryFilter, string leaveTypeFilter, string durationFilter, string degreeFilter, int? collegeIdFilter)
        //{
        //    // جلب البيانات الأساسية
        //    var studyLeavesQuery = db.StudyLeaveMembers
        //        .Include(s => s.FacultyMember)   // 🔄 هذا هو المهم
        //        .Include(s => s.Department)
        //        .AsQueryable();

        //    // الفلاتر التكميلية
        //    if (!string.IsNullOrEmpty(countryFilter))
        //        studyLeavesQuery = studyLeavesQuery.Where(s => s.DestinationCountry == countryFilter);

        //    if (!string.IsNullOrEmpty(leaveTypeFilter))
        //        studyLeavesQuery = studyLeavesQuery.Where(s => s.LeaveType == leaveTypeFilter);

        //    if (!string.IsNullOrEmpty(durationFilter))
        //        studyLeavesQuery = studyLeavesQuery.Where(s => s.LeaveDuration == durationFilter);

        //    if (!string.IsNullOrEmpty(degreeFilter))
        //        studyLeavesQuery = studyLeavesQuery.Where(s => s.CurrentAcademicDegree == degreeFilter);

        //    if (collegeIdFilter.HasValue)
        //    {
        //        var facultyIds = db.FacultyMembers
        //                            .Where(f => f.CollegeID == collegeIdFilter.Value)
        //                            .Select(f => f.FacultyMemberID)
        //                            .ToList();

        //        studyLeavesQuery = studyLeavesQuery.Where(s => facultyIds.Contains(s.FacultyMemberID));
        //    }

        //    // الفلاتر
        //    ViewBag.Countries = new SelectList(db.StudyLeaveMembers.Select(s => s.DestinationCountry).Distinct());
        //    ViewBag.LeaveTypes = new SelectList(db.StudyLeaveMembers.Select(s => s.LeaveType).Distinct());
        //    ViewBag.LeaveDurations = new SelectList(db.StudyLeaveMembers.Select(s => s.LeaveDuration).Distinct());
        //    ViewBag.Degrees = new SelectList(db.StudyLeaveMembers.Select(s => s.CurrentAcademicDegree).Distinct());
        //    ViewBag.Colleges = new SelectList(db.Colleges.Select(c => new { c.CollegeID, c.CollegeName }).Distinct(), "CollegeID", "CollegeName");

        //    var result = studyLeavesQuery.OrderByDescending(s => s.LeaveID).ToList();

        //    return View(result);
        //}
        public ActionResult Index(string FacultyName)
        {
            var query = db.StudyLeaveMembers
                .Include(l => l.FacultyMember)
                .Include(l => l.Department)
                .Include(l => l.FacultyMember.JobTitle)
                      .OrderByDescending(l => l.LeaveID) // ترتيب تنازلي
                                                 .AsQueryable();



            if (!string.IsNullOrWhiteSpace(FacultyName))
                query = query.Where(l => l.FacultyMember.FullName.Contains(FacultyName));

            // نحضّر الـ ViewModel
            var result = query
     .Select(l => new StudyLeaveWithRenewalVM
     {
         StudyLeave = l,
         HasRenewal = db.Renewals.Any(r => r.EntityType == "StudyLeave" && r.EntityID == l.LeaveID),
         Renewals = db.Renewals
             .Where(r => r.EntityType == "StudyLeave" && r.EntityID == l.LeaveID)
             .ToList()
     })
     .OrderByDescending(vm => vm.StudyLeave.LeaveID) // ✅ هنا الترتيب الحقيقي بعد الـ projection
     .ToList();


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

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var leave = db.StudyLeaveMembers
                .Include(l => l.FacultyMember)
                .Include(l => l.Department)
                .FirstOrDefault(l => l.LeaveID == id);

            if (leave == null)
                return HttpNotFound();

            return View(leave); // تأكد أن لديك View اسمه Delete.cshtml
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var leave = db.StudyLeaveMembers
                .Include(l => l.FacultyMember)
                .Include(l => l.Department)
                .FirstOrDefault(l => l.LeaveID == id);

            if (leave == null)
                return HttpNotFound();

            // تأكيد هوية المستخدم
            int userId = Session["UserID"] != null && int.TryParse(Session["UserID"].ToString(), out var uid) ? uid : 0;

            // سجل الحذف العام
            var log = new DeleteLog
            {
                EntityType = "StudyLeave",
                EntityID = leave.LeaveID,
                EntityName = leave.FacultyMember?.FullName,
                AdditionalInfo = $"البلد: {leave.DestinationCountry}, الدرجة: {leave.CurrentAcademicDegree}, المدة: {leave.LeaveDuration}",
                DepartmentID = leave.DepartmentID,
                DepartmentName = leave.Department?.DepartmentName,
                DeletedByUserId = userId,
                DeletedAt = DateTime.Now
            };

            db.DeleteLogs.Add(log);               // يسجل الحذف
            db.StudyLeaveMembers.Remove(leave);   // يحذف السجل الأصلي
            db.SaveChanges();                     // يحفظ في الجدول
            System.Diagnostics.Debug.WriteLine("✅ تم الحذف والحفظ");
            System.Diagnostics.Debug.WriteLine($"📦 Log ID بعد الحفظ: {log.LogID}");

            TempData["Message"] = "تم الحذف وتسجيل العملية";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ViewDeleteLog(int id)
        {
            var log = db.DeleteLogs.FirstOrDefault(l => l.LogID == id);
            if (log == null)
            { 
           
                var exists = db.DeleteLogs.Any(l => l.LogID == id);
                System.Diagnostics.Debug.WriteLine($"🔍 السجل موجود؟ {exists}");
                return HttpNotFound();
            }
            return View("DeleteLogs", log); // هذا يُحمّل DeleteLog.cshtml بشكل صريح

        }






        //[HttpGet]
        //public ActionResult Edit(int id)
        //{
        //    var leave = db.StudyLeaveMembers
        //        .Include(l => l.FacultyMember)
        //        .Include(l => l.Department)
        //        .FirstOrDefault(l => l.LeaveID == id);

        //    if (leave == null)
        //        return HttpNotFound();

        //    ViewBag.FacultyName = leave.FacultyMember?.FullName ?? "غير معروف";
        //    ViewBag.JobTitle = leave.FacultyMember?.JobTitle?.JobTitleName ?? "غير محددة";
        //    ViewBag.DestinationCountry = new SelectList(GetArabicCountriesList(), leave.DestinationCountry);
        //    ViewBag.Departments = new SelectList(db.Departments, "DepartmentID", "DepartmentName", leave.DepartmentID);
        //    ViewBag.MilitaryStatusList = new SelectList(new List<string> { "مؤجل", "أدى الخدمة", "معفى" }, leave.MilitaryStatus);

        //    return View("Edit", leave); // مهم: تحديد اسم الصفحة هنا لو مختلف
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(
        //    StudyLeaveMember updatedLeave,
        //    HttpPostedFileBase UniversityPresidentApprovalFile,
        //    HttpPostedFileBase ExecutiveDecisionFile,
        //    IEnumerable<HttpPostedFileBase> AdditionalFiles)
      
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        ViewBag.Departments = new SelectList(db.Departments, "DepartmentID", "DepartmentName", updatedLeave.DepartmentID);
        //        ViewBag.MilitaryStatusList = new SelectList(new List<string> { "مؤجل", "أدى الخدمة", "معفى" }, updatedLeave.MilitaryStatus);
        //        return View(updatedLeave);
        //    }

        //    var existingLeave = db.StudyLeaveMembers.Find(updatedLeave.LeaveID);
        //    if (existingLeave == null)
        //        return HttpNotFound();

        //    // تحديث البيانات
        //    db.Entry(existingLeave).CurrentValues.SetValues(updatedLeave);
        //    existingLeave.UserUpdatedDate = DateTime.Now; // لو عندك عمود تعديل
        //    db.SaveChanges();

        //    TempData["Message"] = "تم تعديل البيانات بنجاح";
        //    return RedirectToAction("Index");
        //}

        [HttpGet]
        public ActionResult Renew(int id)
        {
            var leave = db.StudyLeaveMembers
                          .Include("FacultyMember")
                          .FirstOrDefault(s => s.LeaveID == id);

            if (leave == null)
                return HttpNotFound();

            ViewBag.LeaveId = leave.LeaveID;
            ViewBag.LeaveNumber = leave.LeaveID; // رقم الإجازة
            return View("Renew", leave);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRenewal(int EntityID, string EntityType, DateTime RenewalDate, bool IsSameLocation = true, string NewLocation = null)
        {


            try
            {
                if (EntityType != "StudyLeave" && EntityType != "Secondment" && EntityType != "Scholarship")
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "نوع الإيفاد غير صالح");
                }

                var renewal = new Renewal
                {
                    EntityID = EntityID,
                    EntityType = EntityType,
                   
                    RenewalDate = RenewalDate,
                    IsSameLocation = IsSameLocation,
                    NewLocation = IsSameLocation ? null : NewLocation,
                    CreatedBy = Convert.ToInt32(Session["UserID"]),
                    CreatedAt = DateTime.Now
                };

                db.Renewals.Add(renewal);
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("====================================");
                        System.Diagnostics.Debug.WriteLine("Entity: " + validationErrors.Entry.Entity.GetType().Name);
                        System.Diagnostics.Debug.WriteLine("Property: " + validationError.PropertyName);
                        System.Diagnostics.Debug.WriteLine("Error: " + validationError.ErrorMessage);
                        System.Diagnostics.Debug.WriteLine("====================================");
                    }
                }

                throw; // دا بيعيد رمي الاستثناء عشان يظهر في المتصفح لو أنت بتعرضه
            }

            TempData["Message"] = "تم التجديد بنجاح.";
            return RedirectToAction("Index", "StudyLeaveMembers");
        }


        private string SaveFile(HttpPostedFileBase file, string folderName)
        {
            string uploadPath = Server.MapPath($"~/Uploads/{folderName}");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string fullPath = Path.Combine(uploadPath, uniqueFileName);
            file.SaveAs(fullPath);

            return $"/Uploads/{folderName}/{uniqueFileName}";
        }



        [HttpGet]
        public ActionResult Edit(int id)
        {
            var leave = db.StudyLeaveMembers
                .Include(l => l.FacultyMember)
                .Include(l => l.Department)
                .FirstOrDefault(l => l.LeaveID == id);

            if (leave == null)
                return HttpNotFound();

            ViewBag.FacultyName = leave.FacultyMember?.FullName ?? "غير معروف";
            ViewBag.JobTitle = leave.FacultyMember?.JobTitle?.JobTitleName ?? "غير محددة";
            ViewBag.DestinationCountry = new SelectList(GetArabicCountriesList(), leave.DestinationCountry);
            ViewBag.Departments = new SelectList(db.Departments, "DepartmentID", "DepartmentName", leave.DepartmentID);
            ViewBag.MilitaryStatusList = new SelectList(new List<string> { "مؤجل", "أدى الخدمة", "معفى" }, leave.MilitaryStatus);

            return View("Edit", leave); // تأكد أن اسم الـ View هو Edit.cshtml
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            StudyLeaveMember updatedLeave,
            HttpPostedFileBase UniversityPresidentApprovalFile,
            HttpPostedFileBase ExecutiveDecisionFile,
            IEnumerable<HttpPostedFileBase> AdditionalFiles)
        {
            if (!ModelState.IsValid)
            {
                // إعادة تحميل عناصر ViewBag
                ViewBag.Departments = new SelectList(db.Departments, "DepartmentID", "DepartmentName", updatedLeave.DepartmentID);
                ViewBag.MilitaryStatusList = new SelectList(new List<string> { "مؤجل", "أدى الخدمة", "معفى" }, updatedLeave.MilitaryStatus);
                ViewBag.DestinationCountry = new SelectList(GetArabicCountriesList(), updatedLeave.DestinationCountry);
                ViewBag.FacultyName = updatedLeave.FacultyMember?.FullName ?? "غير معروف";
                ViewBag.JobTitle = updatedLeave.FacultyMember?.JobTitle?.JobTitleName ?? "غير محددة";

                return View("Edit", updatedLeave);
            }
         
            // ... باقي الحقول النصية
            var existingLeave = db.StudyLeaveMembers
                .Include(l => l.FacultyMember)
                .FirstOrDefault(x => x.LeaveID == updatedLeave.LeaveID);

            if (existingLeave == null)
                return HttpNotFound();

            // تحديث البيانات
            db.Entry(existingLeave).CurrentValues.SetValues(updatedLeave);
            existingLeave.UserUpdatedDate = DateTime.Now;

            // حفظ ملف موافقة رئيس الجامعة
            if (UniversityPresidentApprovalFile != null && UniversityPresidentApprovalFile.ContentLength > 0)
            {
                string path = SaveFile(UniversityPresidentApprovalFile, "PresidentApprovals");
                existingLeave.UniversityPresidentApprovalFilePath = path;
            }

            // حفظ ملف القرار التنفيذي
            if (ExecutiveDecisionFile != null && ExecutiveDecisionFile.ContentLength > 0)
            {
                string path = SaveFile(ExecutiveDecisionFile, "ExecutiveDecisions");
                existingLeave.ExecutiveDecisionFilePath = path;
            }

            // حفظ الملفات الإضافية
            if (AdditionalFiles != null)
            {
                foreach (var file in AdditionalFiles)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        string path = SaveFile(file, "Additional");
                        // يمكنك حفظ هذا المسار في جدول فرعي مثلاً StudyLeaveFiles
                    }
                }
            }

            //existingLeave.ReturnMissionFormDate = updatedLeave.ReturnMissionFormDate;
            //existingLeave.WorkResumptionDate = updatedLeave.WorkResumptionDate;
            //existingLeave.DestinationCountry = updatedLeave.DestinationCountry;
            //existingLeave.MilitaryStatus = updatedLeave.MilitaryStatus;
            db.SaveChanges();
            TempData["Message"] = "تم تعديل البيانات بنجاح";
            return RedirectToAction("Index");
        }

    }
}
