using cdm.Database;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace cdm.Controllers
{
    public class CollegeController : Controller
    {
        private AppDbContext db = new AppDbContext();

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

    }
}