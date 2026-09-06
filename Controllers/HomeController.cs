using cdm.Database;
using cdm.Helpers;
using System.Linq;
using System.Web.Mvc;
namespace cdm.Controllers
{
    [AllowAnonymous]

    public class HomeController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();
      
        //  الصفحة الرئيسية - عرض جميع الأقسام
        public ActionResult Index()
       {
           var departments = db.Departments.ToList(); // مجرد إرسال الأقسام
           return View(departments);
      }

        public ActionResult About()
        {
            ViewBag.Message = "صفحة وصف النظام.";
            return View();
        }
        public ActionResult Team()
        {
          
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "صفحة التواصل معنا.";
            return View();
        }

        public ActionResult Dashboard()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userId = (int)Session["UserID"];
            var user = db.Users.FirstOrDefault(u => u.UserID == userId);

            if (user == null)
                return HttpNotFound("المستخدم غير موجود");

            return View(user);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
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


    }
}
