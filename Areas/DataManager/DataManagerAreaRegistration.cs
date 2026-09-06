using System.Web.Mvc;

namespace cdm.Areas.DataManager
{
    public class DataManagerAreaRegistration : AreaRegistration
    {
        public override string AreaName => "DataManager";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "DataManager_default",
                "DataManager/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "cdm.Areas.DataManager.Controllers" } // مهم!
            );
        }
    }
}
