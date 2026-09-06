
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Reporting.WebForms;

namespace cdm.Reports
{
    public partial class StudyLeavesReport : System.Web.UI.Page
    {
     
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
            ReportViewer1.LocalReport.ReportPath = Server.MapPath("StudyLeavesReport.rdlc");

            // 📌 قراءة Base URL من web.config

            string fullLink = $"{Request.Url.Scheme}://{Request.Url.Authority}/Account/AdminDashboard";

            // 📌 الاتصال بقاعدة البيانات
            string connStr = ConfigurationManager.ConnectionStrings["AppDbContext"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("GetStudyLeavesReportData", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // 📌 إعداد DataSource للتقرير
                        ReportDataSource rds = new ReportDataSource("StudyLeavesDataSet", dt);
                        ReportViewer1.LocalReport.DataSources.Clear();
                        ReportViewer1.LocalReport.DataSources.Add(rds);

                        // 📌 تمرير الرابط كـ Parameter للتقرير
                        ReportParameter reportLinkParam = new ReportParameter("DashboardLink", fullLink);
                        ReportViewer1.LocalReport.SetParameters(reportLinkParam);

                        ReportViewer1.LocalReport.Refresh();
                    }
                }
            }



        }
    }
}







