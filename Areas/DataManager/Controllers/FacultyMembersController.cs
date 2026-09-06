using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using ClosedXML.Excel;                   // NuGet: ClosedXML
using cdm.Database;                      // AppDbContext
using cdm.Models;                        // الكيانات: FacultyMember, College, college_Department, JobTitle
using cdm.Areas.DataManager.Models;      // VM + DTOs

namespace cdm.Areas.DataManager.Controllers
{
    public class FacultyMembersController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();
       
   //     شاشة العرض: ترتيب الأعمدة + جداول مساعدة + رفع الملف
            public ActionResult Index()
        {
            var vm = new FacultyExcelUploadVM
            {
                Colleges = db.Colleges
                    .OrderBy(x => x.CollegeName)
                    .Select(x => new CollegeLite
                    {
                        CollegeID = x.CollegeID,
                        CollegeName = x.CollegeName
                    }).ToList(),

                // عدّلي اسم الـDbSet هنا لو مختلف عندك
                Departments = db.college_Departments
    .OrderBy(x => x.CollegeID).ThenBy(x => x.DepartmentName)
    .Select(x => new DepartmentLite
    {
        College_DepartmentID = x.college_DepartmentID,  // ← عدّلي التعيين
        DepartmentName = x.DepartmentName,
        CollegeID = x.CollegeID
    }).ToList(),


                JobTitles = db.JobTitles
                    .OrderBy(x => x.JobTitleName)
                    .Select(x => new JobTitleLite
                    {
                        JobTitleID = x.JobTitleID,
                        JobTitleName = x.JobTitleName

                    }).ToList()
            };

            return View(vm);
        }

        // تنزيل تمپليت Excel بنفس ترتيب الأعمدة
        public FileResult DownloadTemplate()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("FacultyMembersTemplate");
                string[] headers =
                {
                    "FacultyMemberID","FullName","CollegeID","college_DepartmentID","HireDate",
                    "Email","PhoneNumber","InsuranceNumber","NationalID","Gender",
                    "BirthDate","ID_JobTitle","UserId","UserInsertedDate","UserUpdatedDate","Specialization"
                };
                for (int i = 0; i < headers.Length; i++)
                    ws.Cell(1, i + 1).Value = headers[i];

                ws.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;
                ws.Columns().AdjustToContents();

                using (var ms = new System.IO.MemoryStream())
                {
                    wb.SaveAs(ms);
                    return File(
                        ms.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "FacultyMembersTemplate.xlsx"
                    );
                }
            }
        }

        // استيراد من Excel — إضافة فقط (بدون حذف القديم)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportExcel(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                TempData["err"] = "من فضلك اختاري ملف Excel.";
                return RedirectToAction("Index");
            }

            int added = 0, skipped = 0;
            var sbErrors = new System.Text.StringBuilder();

            // دالة لعرض الـInnerExceptions كاملة
            Func<Exception, string> Deep = (Exception ex) =>
            {
                var msg = ex.Message;
                while (ex.InnerException != null) { ex = ex.InnerException; msg += " | " + ex.Message; }
                return msg;
            };

            try
            {
                using (var wb = new XLWorkbook(file.InputStream))
                {
                    var ws = wb.Worksheets.FirstOrDefault();
                    var used = ws?.RangeUsed();
                    if (ws == null || used == null)
                    {
                        TempData["err"] = "الملف فارغ أو ورقة العمل غير صحيحة.";
                        return RedirectToAction("Index");
                    }

                    var rows = used.RowsUsed().Skip(1).ToList();
                    int rowNo = 1;

                    foreach (var r in rows)
                    {
                        rowNo++;

                        string fullName = r.Cell(2).GetString().Trim();

                        // CollegeID: نص/Double/Int
                        int? collegeId = null;
                        var colTxt = r.Cell(3).GetString().Trim();
                        if (int.TryParse(colTxt, NumberStyles.Any, CultureInfo.InvariantCulture, out var c1))
                            collegeId = c1;
                        else if (r.Cell(3).TryGetValue<double>(out var c2))
                            collegeId = (int)c2;
                        else if (r.Cell(3).TryGetValue<int>(out var c3))
                            collegeId = c3;

                        // DepartmentID: نص/Double/Int
                        int? deptId = null;
                        var deptTxt = r.Cell(4).GetString().Trim();
                        if (int.TryParse(deptTxt, NumberStyles.Any, CultureInfo.InvariantCulture, out var d1))
                            deptId = d1;
                        else if (r.Cell(4).TryGetValue<double>(out var d2))
                            deptId = (int)d2;
                        else if (r.Cell(4).TryGetValue<int>(out var d3))
                            deptId = d3;

                        DateTime? hireDate = r.Cell(5).TryGetValue<DateTime>(out var hd) ? hd : (DateTime?)null;
                        string email = r.Cell(6).GetString().Trim();
                        string phone = r.Cell(7).GetString().Trim();
                        string insurance = r.Cell(8).GetString().Trim();
                        string nationalId = r.Cell(9).GetString().Trim();
                        string gender = r.Cell(10).GetString().Trim();
                        DateTime? birthDate = r.Cell(11).TryGetValue<DateTime>(out var bd) ? bd : (DateTime?)null;
                        int? jobTitleId = r.Cell(12).TryGetValue<int>(out var jt) ? jt : (int?)null;
                        string userIdCell = r.Cell(13).GetString().Trim();
                        string specialization = r.Cell(16).GetString().Trim();

                        // تحقق أساسي
                        if (string.IsNullOrWhiteSpace(fullName))
                        { skipped++; sbErrors.AppendLine($"صف {rowNo}: FullName فارغ."); continue; }
                        if (collegeId == null)
                        { skipped++; sbErrors.AppendLine($"صف {rowNo}: CollegeID فارغ/غير رقمي."); continue; }
                        if (deptId == null)
                        { skipped++; sbErrors.AppendLine($"صف {rowNo}: college_DepartmentID فارغ/غير رقمي."); continue; }

                        // تأكيد وجود الكلية
                        bool collegeOk = db.Colleges.Any(x => x.CollegeID == collegeId);
                        if (!collegeOk) { skipped++; sbErrors.AppendLine($"صف {rowNo}: CollegeID {collegeId} غير موجود."); continue; }

                        //// ✅ فحص القسم على dbo.Departments مباشرة (لتفادي أي Mapping)
                        //var deptOk = db.Database.SqlQuery<int>(
                        //    "SELECT COUNT(1) FROM dbo.Departments WHERE college_DepartmentID = @p0",
                        //    deptId
                        //).FirstOrDefault() > 0;

                        //if (!deptOk)
                        //{
                        //    skipped++;
                        //    sbErrors.AppendLine($"صف {rowNo}: DepartmentID {deptId} غير موجود.");
                        //    continue;
                        //}

                        // (اختياري) منع التكرار حسب الرقم القومي
                        if (!string.IsNullOrWhiteSpace(nationalId))
                        {
                            bool exists = db.FacultyMembers.Any(x => x.NationalID == nationalId);
                            if (exists)
                            {
                                skipped++;
                                sbErrors.AppendLine($"صف {rowNo}: مكرر — NationalID '{nationalId}' موجود بالفعل، تمّ تخطيه.");
                                continue;
                            }
                        }

                        // قصّ آمن
                        email = SafeTrim(email, 150);
                        phone = SafeTrim(phone, 50);
                        insurance = SafeTrim(insurance, 50);
                        nationalId = SafeTrim(nationalId, 50);
                        gender = SafeTrim(gender, 10);
                        specialization = SafeTrim(specialization, 200);

                        // حاول تحويل UserId من الخلية لو رقم، وإلا خليها 3 (أو حسب المطلوب)
                        int? parsedUser = null;
                        if (int.TryParse(userIdCell, out var u))
                            parsedUser = u;

                        var fm = new FacultyMember
                        {
                            FullName = fullName,
                            CollegeID = collegeId.Value,
                            College_DepartmentID = deptId.Value,   // ← المهم
                            HireDate = hireDate,                    // الموديل عندك Required + Nullable => تأكدي الملف بيحوي تاريخ
                            Email = email,
                            PhoneNumber = phone,
                            InsuranceNumber = insurance,
                            NationalID = nationalId,
                            Gender = gender,
                            BirthDate = birthDate,                  // الموديل Required + Nullable => تأكدي الملف بيحوي تاريخ
                            ID_JobTitle = jobTitleId ?? 0,          // لو الموديل int? بدّليها = jobTitleId
                            UserId = parsedUser ?? 3,               // غيّري الافتراضي حسب سياستك
                            UserInsertedDate = DateTime.UtcNow,
                            Specialization = specialization
                        };

                        db.FacultyMembers.Add(fm);
                        try
                        {
                            db.SaveChanges();
                            added++;
                        }
                        catch (DbEntityValidationException vex)
                        {
                            skipped++;
                            foreach (var eve in vex.EntityValidationErrors)
                            {
                                var entity = eve.Entry.Entity;
                                foreach (var ve in eve.ValidationErrors)
                                {
                                    object attempted = null;
                                    try
                                    {
                                        var prop = entity.GetType().GetProperty(ve.PropertyName);
                                        attempted = prop != null ? prop.GetValue(entity, null) : null;
                                    }
                                    catch { }
                                    sbErrors.AppendLine($"صف {rowNo}: '{ve.PropertyName}' → {ve.ErrorMessage}. القيمة: '{(attempted ?? "null")}'.");
                                }
                            }
                            try { db.Entry(fm).State = EntityState.Detached; } catch { }
                        }
                        catch (DbUpdateException duex)
                        {
                            skipped++;
                            try { db.Entry(fm).State = EntityState.Detached; } catch { }
                            sbErrors.AppendLine($"صف {rowNo}: خطأ تحديث (قيود/أنواع/فهرس). التفاصيل: {Deep(duex)}");
                        }
                        catch (Exception exRow)
                        {
                            skipped++;
                            try { db.Entry(fm).State = EntityState.Detached; } catch { }
                            sbErrors.AppendLine($"صف {rowNo}: خطأ غير متوقع: {Deep(exRow)}");
                        }
                    }
                }

                // ملخص
                if (sbErrors.Length > 0)
                    TempData["err"] = $"تم الاستيراد: {added} مضاف / {skipped} متخطّى.{Environment.NewLine}{sbErrors}";
                else
                    TempData["ok"] = $"تم الاستيراد: {added} مضاف / {skipped} متخطّى.";
            }
            catch (Exception ex)
            {
                TempData["err"] = "خطأ أثناء قراءة/حفظ البيانات: " + Deep(ex);
            }

            return RedirectToAction("Index");
        }

        // أداة قص آمنة
        private static string SafeTrim(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return s;
            s = s.Trim();
            return s.Length <= max ? s : s.Substring(0, max);
        }

        // ========== Colleges CRUD (نفس الصفحة) ==========
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateCollege(string CollegeName, string CollegeNameEN)
        {
            if (string.IsNullOrWhiteSpace(CollegeName))
            {
                TempData["err"] = "اسم الكلية مطلوب.";
                return RedirectToAction("Index");
            }

            db.Colleges.Add(new College
            {
                CollegeName = CollegeName.Trim(),
            
            });
            db.SaveChanges();
            TempData["ok"] = "تم إضافة الكلية.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult UpdateCollege(int CollegeID, string CollegeName, string CollegeNameEN)
        {
            var c = db.Colleges.Find(CollegeID);
            if (c == null)
            {
                TempData["err"] = "الكلية غير موجودة.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(CollegeName))
            {
                TempData["err"] = "اسم الكلية مطلوب.";
                return RedirectToAction("Index");
            }

            c.CollegeName = CollegeName.Trim();
         

            db.SaveChanges();
            TempData["ok"] = "تم تحديث بيانات الكلية.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DeleteCollege(int CollegeID)
        {
            var c = db.Colleges.Find(CollegeID);
            if (c == null)
            {
                TempData["err"] = "الكلية غير موجودة.";
                return RedirectToAction("Index");
            }

            // منع الحذف لو عليها أقسام
            bool hasDeps = db.college_Departments.Any(d => d.CollegeID == CollegeID);
            if (hasDeps)
            {
                TempData["err"] = "لا يمكن حذف الكلية لوجود أقسام مرتبطة بها.";
                return RedirectToAction("Index");
            }

            db.Colleges.Remove(c);
            db.SaveChanges();
            TempData["ok"] = "تم حذف الكلية.";
            return RedirectToAction("Index");
        }

        // ======== Departments CRUD (من نفس صفحة FacultyMembers) ========

        // إضافة قسم جديد
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateDepartment(string DepartmentName, int CollegeID)
        {
            if (string.IsNullOrWhiteSpace(DepartmentName))
            {
                TempData["err"] = "اسم القسم مطلوب.";
                return RedirectToAction("Index");
            }
            // تأكيد الكلية
            if (!db.Colleges.Any(x => x.CollegeID == CollegeID))
            {
                TempData["err"] = "الكلية غير موجودة.";
                return RedirectToAction("Index");
            }

            db.college_Departments.Add(new college_Department
            {
                DepartmentName = DepartmentName.Trim(),
                CollegeID = CollegeID
            });
            db.SaveChanges();
            TempData["ok"] = "تم إضافة القسم.";
            return RedirectToAction("Index");
        }

        // تعديل قسم
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult UpdateDepartment(int college_DepartmentID, string DepartmentName, int CollegeID)
        {
            var dep = db.college_Departments.Find(college_DepartmentID);
            if (dep == null)
            {
                TempData["err"] = "القسم غير موجود.";
                return RedirectToAction("Index");
            }
            if (string.IsNullOrWhiteSpace(DepartmentName))
            {
                TempData["err"] = "اسم القسم مطلوب.";
                return RedirectToAction("Index");
            }
            if (!db.Colleges.Any(x => x.CollegeID == CollegeID))
            {
                TempData["err"] = "الكلية غير موجودة.";
                return RedirectToAction("Index");
            }

            dep.DepartmentName = DepartmentName.Trim();
            dep.CollegeID = CollegeID;

            db.SaveChanges();
            TempData["ok"] = "تم تحديث بيانات القسم.";
            return RedirectToAction("Index");
        }

        // حذف قسم
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DeleteDepartment(int college_DepartmentID)
        {
            var dep = db.college_Departments.Find(college_DepartmentID);
            if (dep == null)
            {
                TempData["err"] = "القسم غير موجود.";
                return RedirectToAction("Index");
            }

            // امنعي الحذف لو عليه أعضاء هيئة تدريس
            bool inUse = db.FacultyMembers.Any(f => f.College_DepartmentID == college_DepartmentID);
            if (inUse)
            {
                TempData["err"] = "لا يمكن حذف القسم لوجود أعضاء مرتبطين به.";
                return RedirectToAction("Index");
            }

            db.college_Departments.Remove(dep);
            db.SaveChanges();
            TempData["ok"] = "تم حذف القسم.";
            return RedirectToAction("Index");
        }
        // ======== JobTitles CRUD ========

        // إضافة وظيفة جديدة
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateJobTitle(string JobTitleName, string JobTitleNameEN)
        {
            if (string.IsNullOrWhiteSpace(JobTitleName))
            {
                TempData["err"] = "اسم الوظيفة مطلوب.";
                return Redirect(Url.Action("Index", "FacultyMembers", new { area = "DataManager" }) + "#jobs");
            }

            db.JobTitles.Add(new JobTitle
            {
                JobTitleName = JobTitleName.Trim(),
              
            });
            db.SaveChanges();
            TempData["ok"] = "تم إضافة الوظيفة.";
            return Redirect(Url.Action("Index", "FacultyMembers", new { area = "DataManager" }) + "#jobs");
        }

        // تعديل وظيفة
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult UpdateJobTitle(int JobTitleID, string JobTitleName, string JobTitleNameEN)
        {
            var jt = db.JobTitles.Find(JobTitleID);
            if (jt == null)
            {
                TempData["err"] = "الوظيفة غير موجودة.";
                return Redirect(Url.Action("Index", "FacultyMembers", new { area = "DataManager" }) + "#jobs");
            }
            if (string.IsNullOrWhiteSpace(JobTitleName))
            {
                TempData["err"] = "اسم الوظيفة مطلوب.";
                return Redirect(Url.Action("Index", "FacultyMembers", new { area = "DataManager" }) + "#jobs");
            }

            jt.JobTitleName = JobTitleName.Trim();
           

            db.SaveChanges();
            TempData["ok"] = "تم تعديل الوظيفة.";
            return Redirect(Url.Action("Index", "FacultyMembers", new { area = "DataManager" }) + "#jobs");
        }

        // حذف وظيفة
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DeleteJobTitle(int JobTitleID)
        {
            var jt = db.JobTitles.Find(JobTitleID);
            if (jt == null)
            {
                TempData["err"] = "الوظيفة غير موجودة.";
                return Redirect(Url.Action("Index", "FacultyMembers", new { area = "DataManager" }) + "#jobs");
            }

            bool inUse = db.FacultyMembers.Any(f => f.ID_JobTitle == JobTitleID);
            if (inUse)
            {
                TempData["err"] = "لا يمكن حذف الوظيفة لوجود أعضاء مرتبطين بها.";
                return Redirect(Url.Action("Index", "FacultyMembers", new { area = "DataManager" }) + "#jobs");
            }

            db.JobTitles.Remove(jt);
            db.SaveChanges();
            TempData["ok"] = "تم حذف الوظيفة.";
            return Redirect(Url.Action("Index", "FacultyMembers", new { area = "DataManager" }) + "#jobs");
        }

    }
}
