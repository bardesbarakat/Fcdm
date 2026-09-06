using cdm.Database;
using System.Linq;
using System.Web.Mvc;

namespace cdm.Controllers
{
    public class AuthorizedDepartmentsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            int userId = (int)Session["UserId"];

            var allowedIds = db.UserDepartments
                               .Where(ud => ud.UserID == userId)
                               .Select(ud => ud.DepartmentID)
                               .ToList();

            var departments = db.Departments
                                .Where(d => allowedIds.Contains(d.DepartmentID))
                                .ToList();

            return View(departments);
        }
    }
}