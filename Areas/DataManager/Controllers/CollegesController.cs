using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using cdm.Database;   // AppDbContext
using cdm.Models;     // College

namespace cdm.Areas.DataManager.Controllers
{
    public class CollegesController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // GET: DataManager/Colleges
        public ActionResult Index()
        {
            var list = db.Colleges.OrderBy(c => c.CollegeName).ToList();
            return View(list);
        }

        // GET: DataManager/Colleges/Create
        public ActionResult Create() => View(new College());

        // POST: DataManager/Colleges/Create
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(College model)
        {
            if (!ModelState.IsValid) return View(model);

            db.Colleges.Add(model);
            db.SaveChanges();
            TempData["ok"] = "تم إضافة الكلية.";
            return RedirectToAction("Index");
        }

        // GET: DataManager/Colleges/Edit/5
        public ActionResult Edit(int id)
        {
            var college = db.Colleges.Find(id);
            if (college == null) return HttpNotFound();
            return View(college);
        }

        // POST: DataManager/Colleges/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(College model)
        {
            if (!ModelState.IsValid) return View(model);

            db.Entry(model).State = EntityState.Modified;
            db.SaveChanges();
            TempData["ok"] = "تم تحديث بيانات الكلية.";
            return RedirectToAction("Index");
        }

        // POST: DataManager/Colleges/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var college = db.Colleges.Find(id);
            if (college == null)
            {
                TempData["err"] = "الكلية غير موجودة.";
                return RedirectToAction("Index");
            }

            // لو عليها أقسام/بيانات مرتبطة امنعي الحذف برسالة لطيفة
            var hasDeps = db.college_Departments.Any(d => d.CollegeID == id);
            if (hasDeps)
            {
                TempData["err"] = "لا يمكن الحذف لوجود أقسام مرتبطة بهذه الكلية.";
                return RedirectToAction("Index");
            }

            db.Colleges.Remove(college);
            db.SaveChanges();
            TempData["ok"] = "تم حذف الكلية.";
            return RedirectToAction("Index");
        }
    }
}
