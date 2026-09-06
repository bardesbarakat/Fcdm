using cdm.Database;
using cdm.Helpers;
using cdm.Models;
using cdm.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.SessionState;
using X.PagedList;

namespace cdm.Controllers
{
    [SessionState(SessionStateBehavior.Required)]

    public class AccountController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // GET: /Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "يرجى إدخال اسم المستخدم وكلمة المرور.";
                return View();
            }

            username = username.Trim();
            password = password.Trim();

            var user = db.Users
                         .Include(u => u.Role)
                         .FirstOrDefault(u =>
                            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                            u.PasswordHash == password &&
                            u.IsActive == true);

            if (user == null)
            {
                ViewBag.Error = "اسم المستخدم أو كلمة المرور غير صحيحة.";
                return View();
            }


            // تسجيل الدخول
            FormsAuthentication.SetAuthCookie(username, false);
            // خُد مرجع Session بأمان
            var sess = System.Web.HttpContext.Current?.Session;
            if (sess == null)
            {
                // نادرًا ما تحصل لو الموديول مش جاهز؛ رجّع الرسالة أو أعد المحاولة
                ViewBag.Error = "تعذّر إنشاء الجلسة. رجاءً جرّب مرة أخرى.";
                return View();
            }
            sess["UserID"] = user.UserID.ToString();
            sess["FullName"] = user.FullName;
            sess["UserRole"] = user.Role?.RoleName ?? "";
            sess["RoleID"] = user.RoleID;

            // ✅ جلب أول قسم للمستخدم (كما هو عندك)
            var firstDepartment = db.UserDepartments
                                    .Where(ud => ud.UserID == user.UserID)
                                    .Select(ud => ud.DepartmentID)
                                    .FirstOrDefault();
            if (firstDepartment != 0)
            {
                sess["DepartmentID"] = firstDepartment;
            }
           

            // توجيه بحسب الصلاحية
            if (Session["SelectedDepartmentID"] != null &&
                int.TryParse(Session["SelectedDepartmentID"].ToString(), out int departmentId))
            {
                Session.Remove("SelectedDepartmentID");

                bool hasAccess = db.UserDepartments.Any(ud =>
                    ud.UserID == user.UserID &&
                    ud.DepartmentID == departmentId);

                if (hasAccess)
                {
                    string encryptedId = EncryptionHelper.Encrypt(departmentId.ToString());
                    return RedirectToAction("InsertData", "Department", new { encryptedId });
                }

                ViewBag.Message = "تم تسجيل الدخول، ولكن لا تملك صلاحية الدخول لهذا القسم.";
                return View("NoAccess");
            }

          
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Users()
        {
            if (Session["UserRole"] == null ||
                (Session["UserRole"].ToString() != "Admin" && Session["UserRole"].ToString() != "SuperAdmin"))
            {
                return RedirectToAction("NoAccess", "Home");
            }

            var reportTitles = new List<string>
            {
                "تقرير الحضور الشهري",
                "إحصائيات المستخدمين",
                "تقرير الإجازات",
                "تقرير الأقسام الأكاديمية"
            };

            var viewModel = new AdminDashboardViewModel
            {
                Users = db.Users.Include(u => u.Role).ToList(),
                Reports = reportTitles
            };

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult EditProfile()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login");

            int userId = (int)Session["UserID"];
            var user = db.Users.Find(userId);

            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(User model, HttpPostedFileBase ImageFile)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login");

            int userId = (int)Session["UserID"];
            var user = db.Users.Find(userId);

            if (user == null)
                return HttpNotFound();

            user.Username = model.Username;
            user.PasswordHash = model.PasswordHash;
            user.Email = model.Email;
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.IsActive = model.IsActive;

            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                string fileName = Guid.NewGuid() + System.IO.Path.GetExtension(ImageFile.FileName);
                string path = Server.MapPath("~/UploadedImages/" + fileName);
                ImageFile.SaveAs(path);

                user.ImageUrl = Url.Content("~/Content/UserImages/" + fileName);
            }

            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();

            ViewBag.Message = "تم حفظ التعديلات بنجاح";
            return View(user);
        }

        [HttpGet]
        public ActionResult AdminDashboard()
        {
            // تحقق من وجود جلسة مستخدم
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // تحقق من صلاحية الدور (Admin أو SuperAdmin)
            if (Session["RoleID"] == null || ((int)Session["RoleID"] != 2 && (int)Session["RoleID"] != 3))
            {
                return RedirectToAction("NoAccess", "Account");
            }

            // تقارير ثابتة
            var reportTitles = new List<string>
    {
        "تقرير الحضور الشهري",
        "إحصائيات المستخدمين",
        "تقرير الإجازات",
        "تقرير الأقسام الأكاديمية"
    };

            // تحميل البيانات المطلوبة للوحة التحكم
            var viewModel = new AdminDashboardViewModel
            {
                Users = db.Users.Include(u => u.Role).ToList(),
                Reports = reportTitles,
                Departments = db.Departments.ToList(), // جدول الأقسام
                FacultyMembers = db.FacultyMembers
                          .Include(f => f.College)
                          .Include(f => f.CollegeDepartment) // ✅ هذا هو الصحيح
                          .ToList()
            };

            var users = db.Users.Where(u => u.IsActive == true).ToList();
            ViewBag.Users = users;

            
            return View(viewModel);
        }

        public ActionResult UsersPartial()
        {
            var users = db.Users.Include(u => u.Role).ToList();
            var roles = db.Roles.ToList(); // ✅ جلب كل الأدوار

            ViewBag.Roles = roles; // ✅ مررهم للـ View
            return PartialView("UsersPartial", users); // ✅ الصحيح

        }

        public ActionResult ReportsPartial()
        {
            return View("_ReportsPartial");
        }

        public ActionResult DepartmentsPartial()
        {
            var departments = db.Departments.ToList();

            // جلب المستخدمين
            var users = db.Users.Where(u => u.IsActive == true).ToList();
            ViewBag.Users = users;

            return View("_DepartmentsPartial", departments);
        }

        public ActionResult NoAccess()
        {
            return View();
        }




        public ActionResult FacultyMembers(int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var facultyMembers = db.FacultyMembers
                .Include("College")
                .Include("CollegeDepartment")
                .Include("JobTitle")
                .Include("User")
                .OrderBy(f => f.FacultyMemberID)
                .ToPagedList(pageNumber, pageSize);

            return View("FacultyMembers", facultyMembers); // ده View عادي (مش Partial)
        }

        //جزء users
      
        [HttpGet]
        public ActionResult GetUser(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
                return HttpNotFound();

            // هنا بنبني كائن خفيف DTO بالخصائص المطلوبة فقط
            var userDto = new
            {
                user.UserID,
                user.Username,
                user.Email,
                user.FullName,
                user.PhoneNumber,
                user.RoleID,
                user.IsActive
            };

            return Json(userDto, JsonRequestBehavior.AllowGet);
        }

        // إضافة / تعديل مستخدم
        [HttpPost]
        public ActionResult SaveUser(User user, string IsActive)
        {
            user.IsActive = (IsActive == "on");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, errors });
            }

            bool isUsernameDuplicate = db.Users.Any(u => u.Username == user.Username && u.UserID != user.UserID);
            if (isUsernameDuplicate)
                return Json(new { success = false, message = "اسم المستخدم موجود بالفعل!" });

            bool isEmailDuplicate = db.Users.Any(u => u.Email == user.Email && u.UserID != user.UserID);
            if (isEmailDuplicate)
                return Json(new { success = false, message = "البريد الإلكتروني مستخدم بالفعل!" });

            if (user.UserID == 0)
            {
                db.Users.Add(user);
            }
            else
            {
                var existingUser = db.Users.Find(user.UserID);
                if (existingUser == null)
                    return HttpNotFound();

                existingUser.Username = user.Username;
                existingUser.Email = user.Email;
                existingUser.FullName = user.FullName;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.RoleID = user.RoleID;
                existingUser.IsActive = user.IsActive;

                // ✅ تعديل كلمة المرور لو فقط كانت مدخلة
                if (!string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    existingUser.PasswordHash = user.PasswordHash;
                }

                db.Entry(existingUser).State = EntityState.Modified;
            }

            db.SaveChanges();
            return Json(new { success = true });
        }



        // حذف مستخدم
        [HttpPost]
        public ActionResult DeleteUserConfirmed(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
                return HttpNotFound();

            db.Users.Remove(user);
            db.SaveChanges();

            return Json(new { success = true });
        }


        // جزء الاقسام

        // جلب بيانات قسم واحد (تُستخدم في التعديل)
        public ActionResult GetDepartment(int id)
        {
            var department = db.Departments.Find(id);
            if (department == null)
                return HttpNotFound();

            return Json(department, JsonRequestBehavior.AllowGet);
        }

        // إضافة / تعديل قسم

        [HttpPost]
        [Route("Account/SaveDepartment")]
        public ActionResult SaveDepartment(Department department, int? AssignedUserID)
        {
            System.Diagnostics.Debug.WriteLine("بيانات مستلمة:");
            foreach (var key in Request.Form.AllKeys)
            {
                System.Diagnostics.Debug.WriteLine($"{key}: {Request.Form[key]}");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                System.Diagnostics.Debug.WriteLine("ModelState Errors:");
                foreach (var err in errors)
                    System.Diagnostics.Debug.WriteLine(err);

                return Json(new { success = false, errors });
            }

            if (department.DepartmentID == 0)
            {
                // إضافة قسم جديد
                department.CreatedAt = DateTime.Now;
                db.Departments.Add(department);
                db.SaveChanges();

                // ربط القسم بمستخدم (لو تم اختياره)
                if (AssignedUserID.HasValue)
                {
                    var userDepartment = new UserDepartment
                    {
                        UserID = AssignedUserID.Value,
                        DepartmentID = department.DepartmentID,
                        AssignedAt = DateTime.Now
                    };
                    db.UserDepartments.Add(userDepartment);
                    db.SaveChanges();
                }
            }
            else
            {
                // تعديل قسم موجود
                var existingDept = db.Departments.Find(department.DepartmentID);
                if (existingDept == null)
                    return HttpNotFound();

                existingDept.DepartmentName = department.DepartmentName;
                existingDept.DepartmentNameEN = department.DepartmentNameEN;
                existingDept.Description = department.Description;

                db.Entry(existingDept).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                // لو حابة تحدث رابط المستخدم المسؤول هنا كمان:
                if (AssignedUserID.HasValue)
                {
                    // نحذف الربط القديم (لو حابب)
                    var oldUserDept = db.UserDepartments.FirstOrDefault(ud => ud.DepartmentID == department.DepartmentID);
                    if (oldUserDept != null)
                    {
                        db.UserDepartments.Remove(oldUserDept);
                        db.SaveChanges();
                    }

                    // نضيف الربط الجديد
                    var userDepartment = new UserDepartment
                    {
                        UserID = AssignedUserID.Value,
                        DepartmentID = department.DepartmentID,
                        AssignedAt = DateTime.Now
                    };
                    db.UserDepartments.Add(userDepartment);
                    db.SaveChanges();
                }
            }

            return Json(new { success = true });
        }



        // حذف قسم
        [HttpPost]
        public ActionResult DeleteDepartment(int id)
        {
            var department = db.Departments.Find(id);
            if (department == null)
                return HttpNotFound();

            db.Departments.Remove(department);
            db.SaveChanges();

            return Json(new { success = true });
        }


        public ActionResult AddDepartment()
        {
            var users = db.Users.Where(u => u.IsActive == true).ToList();
            ViewBag.Users = users;

            return View();
        }



        //// جزء الاجازات الدراسية///////

        public ActionResult StudyLeavesPartial()
        {
            // لو اليوزر مش مسجل دخول
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            string userRole = Session["UserRole"]?.ToString() ?? "";
            List<StudyLeaveMember> leaves;

            if (userRole == "Admin" || userRole == "SuperAdmin")
            {
                // ✅ لو Admin أو SuperAdmin، يرجع كل الإجازات (مباشرة من موديلك)
                leaves = db.StudyLeaveMembers
                    .Include(l => l.FacultyMember)
                    .Include(l => l.Department)
                    .ToList();
            }
            else
            {
                // ✅ لو مستخدم عادي، يرجع بس الإجازات الخاصة بأقسامه
                int userId = (int)Session["UserID"];
                var userDepartments = db.UserDepartments
                    .Where(ud => ud.UserID == userId)
                    .Select(ud => ud.DepartmentID)
                    .ToList();

                leaves = db.StudyLeaveMembers
                    .Include(l => l.FacultyMember)
                    .Include(l => l.Department)
                    .Where(l => userDepartments.Contains(l.DepartmentID))
                    .ToList();
            }

            return View("StudyLeaves", leaves);
        }
        /// <summary>
        /// نعرض قايمة بالاعضاء اللي ليهم اجازات
        /// </summary>
        /// <returns></returns>
        public ActionResult StudyLeavesMembersList()
        {
            var facultyWithLeaves = db.StudyLeaveMembers
                .Include(l => l.FacultyMember)
                .GroupBy(l => l.FacultyMemberID)
                .Select(g => g.FirstOrDefault().FacultyMember)
                .Where(f => f != null)
                .ToList();

            return View(facultyWithLeaves);
        }

        public ActionResult StudyLeavesByMember(int id)
        {
            var leaves = db.StudyLeaveMembers
                .Include(l => l.FacultyMember)
                .Where(l => l.FacultyMemberID == id)
                .ToList();

            if (!leaves.Any())
            {
                ViewBag.Message = "لا يوجد إجازات لهذا العضو.";
            }

            return View(leaves);
        }
        public ActionResult StudyLeavesReport()
        {
            // جلب البيانات من الـ Stored Procedure (باستخدام DTO)
            var reportData = db.Database.SqlQuery<StudyLeaveReportDTO>("GetStudyLeavesReportData").ToList();

            // خزنها في Session (أو ViewBag) عشان نمررها للـ Report
            Session["StudyLeavesReportData"] = reportData;

            return View();
        }

    }


}


