
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FacultyMembers.aspx.cs" Inherits="cdm.Reports.FacultyMembers" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>


<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>تقرير بيانات أعضاء هيئة التدريس</title>
    <style>
    body, html {
        margin: 0;
        padding: 0;
        direction: rtl;
    }

    form {
        margin: 0;
        padding: 0;
    }

    .report-container {
           overflow-x: auto;
    max-width: 100%;
    
        padding: 0 20px;
        box-sizing: border-box;
    }

    .ReportViewer {
        direction: rtl !important;
        text-align: right !important;
    }

    .ReportViewer .Toolbar,
    .ReportViewer .Zoom,
    .ReportViewer .Export,
    .ReportViewer .Find,
    .ReportViewer .DocMap,
    .ReportViewer .SearchBar,
    .ReportViewer .Navigation,
    .ReportViewer .ViewerHeader {
        direction: rtl !important;
        text-align: right !important;
        float: right !important;
        clear: both;
    }

    /* ترتيب أدوات التحكم */
    .ReportViewer .Toolbar div {
        float: right !important;
        margin-left: 5px;
    }

    /* تحسين عرض العنوان */
    .ReportViewer .ReportTitle {
        text-align: right !important;
        font-weight: bold;
        font-size: 18px;
        margin-right: 10px;
    }
</style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <div>
            <h2 class="text-center mb-4">تقرير بيانات أعضاء هيئة التدريس</h2>

            <asp:Button ID="btnBack" runat="server" Text="عودة إلى الرئيسية" CssClass="btn btn-primary mb-3" OnClientClick="window.location.href='../Account/AdminDashboard'; return false;" />

            <rsweb:ReportViewer ID="ReportViewer1" runat="server"
     Font-Names="Cairo"
     Font-Size="12pt"
  width="650px"
     Height="100%"
     AsyncRendering="False"
     SizeToReportContent="True"
     CssClass="ReportViewer" />
    </form>
</body>
</html>
