using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace JK.Core
{
    public partial class SiteSettings
    {
        private static SiteSettings _instance;

        /// <summary>
        /// configuration
        /// </summary>
        private Configuration _config;

        /// <summary>
        /// Default Constructor
        /// </summary>
        private SiteSettings()
        {
            Open();
        }

        /// <summary>
        /// Get setting from SiteSetting
        /// </summary>
        /// <returns></returns>
        public static SiteSettings GetInstance()
        {
            return _instance ?? (_instance = new SiteSettings());
        }

        /// <summary>
        /// Open Web.Config
        /// </summary>
        private void Open()
        {
            _config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            if (_config == null) throw new Exception("Could not open config file");
        }

        /// <summary>
        /// Get key from Web.config
        /// </summary>
        /// <param name="key">string; Key in Web.config</param>
        /// <returns>value of this Key</returns>
        public string GetValue(string key)
        {
            if(_config.AppSettings.Settings[key]!=null)
            {
                return _config.AppSettings.Settings[key].Value;
            }
            return "";
        }

        /// <summary>
        /// Set value of Key input
        /// </summary>
        /// <param name="key">string; key in web.config</param>
        /// <param name="value">string; value this key</param>
        public void SetValue(string key, string value)
        {
            _config.AppSettings.Settings[key].Value = value;
        }

        /// <summary>
        /// Save configuation
        /// </summary>
        public void Save()
        {
            if (_config == null) return;
            _config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}