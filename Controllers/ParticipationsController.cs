using cdm.Database;
using cdm.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace cdm.Controllers
{
    public class ParticipationsController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        [HttpGet]
        public ActionResult Details(int id)
        {
            var participation = db.Participations
                .Include(p => p.FacultyMember)
                .FirstOrDefault(p => p.Id == id);

            if (participation == null)
                return HttpNotFound();

            return View(participation); // تأكد إن فيه View اسمه DetailsPart.cshtml
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var participation = db.Participations.Find(id);
            if (participation == null)
            {
                return HttpNotFound();
            }

            // 🗑 حذف السجل
            db.Participations.Remove(participation);

            // 📝 إضافة لوج الحذف
            var log = new DeleteLog
            {
                EntityName = "Participation",
                //RecordId = id,
            //    DeletedBy = Session["UserID"]?.ToString(),
                DeletedAt = DateTime.Now
            };
            db.DeleteLogs.Add(log);

            db.SaveChanges();

            TempData["Message"] = "تم حذف المشاركة بنجاح.";
            return RedirectToAction("Index");
        }
        // GET: Participations/Delete/5
        public ActionResult Delete(int id)
        {
            var participation = db.Participations
                .Include(p => p.FacultyMember)
                .FirstOrDefault(p => p.Id == id);

            if (participation == null)
            {
                return HttpNotFound();
            }

            return View(participation);
        }
    }
}
