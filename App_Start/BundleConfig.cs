using System.Web.Optimization;

namespace cdm
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // jQuery & Validation
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Modernizr (for development only)
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // Bootstrap Scripts
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                        "~/Scripts/bootstrap.js"));

            // Custom JS bundle including all required vendor scripts and main.js
            bundles.Add(new ScriptBundle("~/bundles/js").Include(
      "~/Content/vendor/bootstrap/js/bootstrap.bundle.min.js",
      "~/Content/vendor/php-email-form/validate.js",
      "~/Content/vendor/aos/aos.js",
      "~/Content/vendor/swiper/swiper-bundle.min.js",
      "~/Content/vendor/glightbox/js/glightbox.min.js",
      "~/Content/vendor/imagesloaded/imagesloaded.pkgd.min.js",
      "~/Content/vendor/isotope-layout/isotope.pkgd.min.js",
      "~/Content/js/main.js"
  ));


            // Main CSS bundle including all required styles
            bundles.Add(new StyleBundle("~/bundles/css").Include(
                "~/Content/vendor/bootstrap/css/bootstrap.min.css",
                "~/Content/vendor/bootstrap-icons/bootstrap-icons.css",
                "~/Content/vendor/aos/aos.css",
                "~/Content/vendor/swiper/swiper-bundle.min.css",
                "~/Content/vendor/glightbox/css/glightbox.min.css",
                "~/Content/css/main.css"
            ));
#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
BundleTable.EnableOptimizations = true;
#endif

        }
    }
}
