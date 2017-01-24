using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank
{
    public static class Settings
    {
        public static string ConnectionString { get; set; } = ConfigurationManager.ConnectionStrings["bank"].ConnectionString;
        public static bool FriendlyErrors { get; set; } = ConfigurationManager.AppSettings.AllKeys.Contains("FriendlyErrors") ? bool.Parse(ConfigurationManager.AppSettings["FriendlyErrors"]) : true;
        public static string GoogleAnalyticsTrackingId { get; set; }
            = ConfigurationManager.AppSettings.AllKeys.Contains("GoogleAnalyticsTrackingId") ? ConfigurationManager.AppSettings["GoogleAnalyticsTrackingId"] : null;
        public static string FacebookImportAppId { get; set; }
            = ConfigurationManager.AppSettings.AllKeys.Contains("FacebookImportAppId") ? ConfigurationManager.AppSettings["FacebookImportAppId"] : null;
        public static string FacebookImportAppSecret { get; set; }
            = ConfigurationManager.AppSettings.AllKeys.Contains("FacebookImportAppSecret") ? ConfigurationManager.AppSettings["FacebookImportAppSecret"] : null;

        public static string ReportTemplatePath { get; set; }
            = ConfigurationManager.AppSettings.AllKeys.Contains("ReportTemplatePath") ? ConfigurationManager.AppSettings["ReportTemplatePath"] : null;

        public static string ElasticsearchHost { get; set; }
            = ConfigurationManager.AppSettings.AllKeys.Contains("ElasticsearchHost") ? ConfigurationManager.AppSettings["ElasticsearchHost"] : null;

        public static string ElasticsearchPassword { get; set; }
            = ConfigurationManager.AppSettings.AllKeys.Contains("ElasticsearchPassword") ? ConfigurationManager.AppSettings["ElasticsearchPassword"] : null;

        public static string TwitterKey { get; set; }
            = ConfigurationManager.AppSettings.AllKeys.Contains("TwitterKey") ? ConfigurationManager.AppSettings["TwitterKey"] : null;

        public static string TwitterSecret { get; set; }
            = ConfigurationManager.AppSettings.AllKeys.Contains("TwitterSecret") ? ConfigurationManager.AppSettings["TwitterSecret"] : null;


        private static string _phantomJs = null;
        public static string PhantomJs
        {
            get
            {
                return _phantomJs = AppDomain.CurrentDomain.GetData("DataDirectory").ToString() + @"\phantomjs\phantomjs.exe";
            }
        }

        
    }
}
