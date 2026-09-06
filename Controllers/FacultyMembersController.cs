using cdm.Database;
using cdm.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using X.PagedList;

namespace cdm.Controllers
{
    [Authorize]
    public class FacultyMembersController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: FacultyMembers

        public ActionResult Index(int? collegeId, int? departmentId, int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var facultyMembers = db.FacultyMembers
                .Include(f => f.College)
                .Include(f => f.CollegeDepartment)
                .Include(f => f.JobTitle)
                .AsQueryable();

            if (collegeId.HasValue)
            {
                facultyMembers = facultyMembers.Where(f => f.CollegeID == collegeId.Value);
            }

            if (departmentId.HasValue)
            {
                facultyMembers = facultyMembers.Where(f => f.College_DepartmentID == departmentId.Value);
            }

            ViewBag.Colleges = new SelectList(db.Colleges.ToList(), "CollegeID", "CollegeName", collegeId);
            ViewBag.Departments = collegeId.HasValue
                ? new SelectList(db.college_Departments.Where(d => d.CollegeID == collegeId.Value).ToList(), "college_DepartmentID", "DepartmentName", departmentId)
                : new SelectList(Enumerable.Empty<SelectListItem>());

            ViewBag.SelectedCollegeId = collegeId;
            ViewBag.SelectedDepartmentId = departmentId;

            return View(facultyMembers.OrderByDescending(f => f.FacultyMemberID).ToPagedList(pageNumber, pageSize));

        }




        // GET: FacultyMembers/Create
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.JobTitles = new SelectList(db.JobTitles.ToList(), "JobTitleID", "JobTitleName");
            ViewBag.Colleges = new SelectList(db.Colleges.ToList(), "CollegeID", "CollegeName");
            ViewBag.Departments = new SelectList(Enumerable.Empty<SelectListItem>()); // تبدأ فاضية
            return View();
        }

        // POST: FacultyMembers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FacultyMember facultyMember, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (Session["UserID"] != null && int.TryParse(Session["UserID"].ToString(), out int userId))
                {
                    facultyMember.UserId = userId;
                    facultyMember.UserInsertedDate = DateTime.Now;
                }

                db.FacultyMembers.Add(facultyMember);
                db.SaveChanges();

                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "FacultyMembers", new { facultyMemberId = facultyMember.FacultyMemberID });
            }

            ViewBag.JobTitles = new SelectList(db.JobTitles, "JobTitleID", "JobTitleName", facultyMember.ID_JobTitle);
            ViewBag.Colleges = new SelectList(db.Colleges, "CollegeID", "CollegeName", facultyMember.CollegeID);
            ViewBag.Departments = new SelectList(db.college_Departments, "college_DepartmentID", "DepartmentName", facultyMember.College_DepartmentID);
            ViewBag.ReturnUrl = returnUrl;

            return View(facultyMember);
        }

        // GET: FacultyMembers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            FacultyMember facultyMember = db.FacultyMembers.Find(id);
            if (facultyMember == null)
                return HttpNotFound();

            ViewBag.CollegeID = new SelectList(db.Colleges, "CollegeID", "CollegeName", facultyMember.CollegeID);
            ViewBag.College_DepartmentID = new SelectList(db.college_Departments, "college_DepartmentID", "DepartmentName", facultyMember.College_DepartmentID);
            ViewBag.JobTitleID = new SelectList(db.JobTitles, "JobTitleID", "JobTitleName", facultyMember.ID_JobTitle);
            return View(facultyMember);
        }

        // POST: FacultyMembers/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FacultyMember facultyMember)
        {
            if (ModelState.IsValid)
            {
                var original = db.FacultyMembers.AsNoTracking().FirstOrDefault(f => f.FacultyMemberID == facultyMember.FacultyMemberID);
                if (original == null)
                    return HttpNotFound();

                facultyMember.UserInsertedDate = original.UserInsertedDate;

                if (Session["UserID"] != null && int.TryParse(Session["UserID"].ToString(), out int userId))
                {
                    facultyMember.UserId = userId;
                    facultyMember.UserUpdatedDate = DateTime.Now;
                }

                db.Entry(facultyMember).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CollegeID = new SelectList(db.Colleges, "CollegeID", "CollegeName", facultyMember.CollegeID);
            ViewBag.College_DepartmentID = new SelectList(db.college_Departments, "college_DepartmentID", "DepartmentName", facultyMember.College_DepartmentID);
            ViewBag.JobTitleID = new SelectList(db.JobTitles, "JobTitleID", "JobTitleName", facultyMember.ID_JobTitle);

            return View(facultyMember);
        }

        // GET: FacultyMembers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            FacultyMember facultyMember = db.FacultyMembers.Find(id);
            if (facultyMember == null)
                return HttpNotFound();

            return View(facultyMember);
        }

       
        [HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Delete(int id)
{
    var faculty = db.FacultyMembers
        .Include(f => f.College)
        .Include(f => f.CollegeDepartment)
        .Include(f => f.JobTitle)
        .FirstOrDefault(f => f.FacultyMemberID == id);

    if (faculty == null)
        return HttpNotFound();

    // الحصول على المستخدم الحالي (إن وُجد)
    int userId = Session["UserID"] != null && int.TryParse(Session["UserID"].ToString(), out var uid) ? uid : 0;

    // إضافة سجل الحذف إلى جدول DeleteLogs
    var log = new DeleteLog
    {
        EntityType = "FacultyMember",
        EntityID = faculty.FacultyMemberID,
        EntityName = faculty.FullName,
        AdditionalInfo = $"الدرجة: {faculty.JobTitle?.JobTitleName}, البريد: {faculty.Email}, الهاتف: {faculty.PhoneNumber}",
        DepartmentID = faculty.College_DepartmentID,
        DepartmentName = faculty.CollegeDepartment?.DepartmentName,
        DeletedByUserId = userId,
        DeletedAt = DateTime.Now
    };

    db.DeleteLogs.Add(log);           // 🟢 حفظ في سجل الحذف
    db.FacultyMembers.Remove(faculty); // 🗑 حذف من الجدول الرئيسي
    db.SaveChanges();                 // 💾 تأكيد الحذف والحفظ

    TempData["Message"] = "تم حذف عضو هيئة التدريس وتسجيل الحذف في السجل.";
    return RedirectToAction("Index");
}






        [HttpGet]
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


        public JsonResult GetJobTitlesByDepartment(int departmentId)
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
        public JsonResult GetFacultyMembers(int departmentId, int jobTitleId)
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
        
        public ActionResult SelectFacultyForStudyLeave()
        {
            ViewBag.Colleges = new SelectList(db.Colleges.ToList(), "CollegeID", "CollegeName");
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
       
        public ActionResult Report()
        {
            var facultyMembers = db.FacultyMembers
                .Include("College")
                .Include("CollegeDepartment")
                .Include("JobTitle")
                .Include("User") // هنا مهم جداً
                .ToList();

            return View(facultyMembers);
        }


        public JsonResult GetFacultyDetails(int id)
        {
            var faculty = db.FacultyMembers.FirstOrDefault(f => f.FacultyMemberID == id);
            if (faculty == null)
                return Json(null, JsonRequestBehavior.AllowGet);

            var viewModel = new FacultyMemberDetailsVM
            {
                FullName = faculty.FullName,
                Email = faculty.Email,
                PhoneNumber = faculty.PhoneNumber,
                Gender = faculty.Gender,
                BirthDate = faculty.BirthDate ?? DateTime.MinValue,
                HireDate = faculty.HireDate ?? DateTime.MinValue,
                NationalID = faculty.NationalID,
                InsuranceNo = faculty.InsuranceNumber
            };

            return Json(viewModel, JsonRequestBehavior.AllowGet);
        }


    }
}
