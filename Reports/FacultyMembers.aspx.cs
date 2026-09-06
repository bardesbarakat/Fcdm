using Microsoft.Reporting.WebForms;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace cdm.Reports
{
    public partial class FacultyMembers : System.Web.UI.Page
    {
        protected global::Microsoft.Reporting.WebForms.ReportViewer ReportViewer1; // ✅ تأكد إنه متعرف
        protected global::System.Web.UI.ScriptManager ScriptManager1;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ReportViewer1.LocalReport.EnableHyperlinks = true;

                LoadReport();
            }
        }

        private void LoadReport()
        {
            // 📌 تفعيل الروابط داخل التقرير
            ReportViewer1.LocalReport.EnableHyperlinks = true;

            // 📌 تعيين مسار التقرير
            ReportViewer1.LocalReport.ReportPath = Server.MapPath("FacultyMembers.rdlc");

            // 📌 قراءة Base URL من web.config
          
            string fullLink = $"{Request.Url.Scheme}://{Request.Url.Authority}/Account/AdminDashboard";

            // 📌 الاتصال بقاعدة البيانات
            string connStr = ConfigurationManager.ConnectionStrings["AppDbContext"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("GetFacultyMembersReport", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // 📌 إعداد DataSource للتقرير
                        ReportDataSource rds = new ReportDataSource("FacultyMembersDS", dt);
                        ReportViewer1.LocalReport.DataSources.Clear();
                        ReportViewer1.LocalReport.DataSources.Add(rds);

                        // 📌 تمرير الرابط كـ Parameter للتقرير
                        ReportParameter reportLinkParam = new ReportParameter("DashboardLink", fullLink);
                        ReportViewer1.LocalReport.SetParameters(reportLinkParam);
                        ReportViewer1.ZoomMode = ZoomMode.PageWidth;

                        ReportViewer1.LocalReport.Refresh();
                    }
                }
            }



        }
    }
}