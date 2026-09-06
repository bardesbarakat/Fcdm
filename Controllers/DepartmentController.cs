using cdm.Database;
using cdm.Helpers;
using cdm.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using X.PagedList;

namespace cdm.Controllers
{
  //  [Authorize]
    public class DepartmentController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult InsertData(string encryptedId)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            int userId = (int)Session["UserID"];
            int departmentId = int.Parse(EncryptionHelper.Decrypt(encryptedId));

            // التحقق: هل المستخدم له صلاحية على هذا القسم؟
            bool hasAccess = db.UserDepartments.Any(ud => ud.UserID == userId && ud.DepartmentID == departmentId);

            if (!hasAccess)
                return RedirectToAction("NoAccess", "Account"); // لو ملوش صلاحية يروح NoAccess

            // تحميل البيانات لو له صلاحية
            ViewBag.Colleges = new SelectList(db.Colleges.ToList(), "CollegeID", "CollegeName");
            ViewBag.JobTitles = new SelectList(db.JobTitles.ToList(), "JobTitleID", "JobTitleName");
            ViewBag.EncryptedId = encryptedId;

            return View();
        }


     
        public ActionResult SelectDepartment()
        {
            ViewBag.Departments = new SelectList(db.Departments, "DepartmentID", "DepartmentName");
            return View();
        }

        [HttpPost]
        public ActionResult SelectDepartment(int departmentId)
        {
            string encryptedId = EncryptionHelper.Encrypt(departmentId.ToString());
            return RedirectToAction("ViewData", new { encryptedId });
        }


        // عرض الأقسام
        public ActionResult Index()
        {
            var departments = db.Departments.OrderBy(d => d.DepartmentID).ToList();
            return PartialView("_DepartmentsPartial", departments);
        }

        // إضافة قسم
        [HttpPost]
        public ActionResult Add(string departmentName, string departmentNameEN, string description)
        {
            var dept = new Department
            {
                DepartmentName = departmentName,
                DepartmentNameEN = departmentNameEN,
                Description = description,
                CreatedAt = DateTime.Now
            };
            db.Departments.Add(dept);
            db.SaveChanges();
            return Json(new { success = true });
        }

        // تعديل قسم
        [HttpPost]
        public ActionResult Edit(int id, string departmentName, string departmentNameEN, string description)
        {
            var dept = db.Departments.Find(id);
            if (dept == null)
                return Json(new { success = false });

            dept.DepartmentName = departmentName;
            dept.DepartmentNameEN = departmentNameEN;
            dept.Description = description;
            db.SaveChanges();
            return Json(new { success = true });
        }

        // حذف قسم
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var dept = db.Departments.Find(id);
            if (dept == null)
                return Json(new { success = false });

            db.Departments.Remove(dept);
            db.SaveChanges();
            return Json(new { success = true });
        }


        public JsonResult GetAllJobTitles()
        {
            // جلب الدرجات الوظيفية من قاعدة البيانات
            var jobTitles = db.JobTitles
                .Select(j => new { Value = j.JobTitleID, Text = j.JobTitleName })
                .ToList();

            return Json(jobTitles, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetDepartmentsByCollege(int collegeId)
        {
            // جلب الأقسام بناءً على الكلية
            var departments = db.college_Departments
                .Where(d => d.CollegeID == collegeId)
                .Select(d => new { Value = d.college_DepartmentID, Text = d.DepartmentName })
                .ToList();

            return Json(departments, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Index(string encryptedId)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            int userId = (int)Session["UserID"];
            int departmentId = int.Parse(EncryptionHelper.Decrypt(encryptedId));

            bool hasAccess = db.UserDepartments.Any(ud => ud.UserID == userId && ud.DepartmentID == departmentId);

            if (!hasAccess)
                return RedirectToAction("NoAccess", "Account");

            // تحميل البيانات الخاصة بالقسم
            var data = db.StudyLeaveMembers.Where(s => s.DepartmentID == departmentId).ToList();
            return View(data);
        }


    }
}
 
