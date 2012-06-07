using System;
using System.Web;
namespace JK.Core.Utilities
{
    public class Cookier
    {
        /// <summary>
        /// Construstor Cookier
        /// </summary>
        public Cookier()
        {
            var cookie = new HttpCookie("name");
        }

        /// <summary>
        /// Set Name and Value of Cookier
        /// </summary>
        /// <param name="name">string; Name of Cookier</param>
        /// <param name="value">string; value of Cookier</param>
        public void Set(string name, string value)
        {
            var setting = SiteSettings.GetInstance();
            var ctx = HttpContext.Current;

            var cookie = new HttpCookie(name);
            cookie.Expires = DateTime.Now.AddDays(1);
            cookie.Value = value;

            ctx.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Set Name, Value and Expires of Cookier
        /// </summary>
        /// <param name="name">string; Name of Cookier</param>
        /// <param name="value">string; value Of cookier</param>
        /// <param name="expires">datetime; Expires day of cookier</param>
        public void Set(string name, string value, int expires)
        {
            var ctx = HttpContext.Current;
            var setting = SiteSettings.GetInstance();
            var cookie = new HttpCookie(name);
            cookie.Expires = DateTime.Now.AddDays(expires);
            cookie.Value = value;
            cookie.Domain = setting.SiteUrl;
            ctx.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Get cookier
        /// </summary>
        /// <param name="name">string; Name of cookier</param>
        /// <returns>value Cookier</returns>
        public HttpCookie Get(string name)
        {
            if(IsExist(name))
            {
                var ctx = HttpContext.Current;
                return ctx.Request.Cookies[name];
            }
            return null;
        }

        /// <summary>
        /// Get value of cookie by name
        /// </summary>
        /// <param name="name">string; name of Cookie</param>
        /// <returns>string; value</returns>
        public string GetValue(string name)
        {
            return Get(name).Value;
        }

        /// <summary>
        /// Remove cookie by name
        /// </summary>
        /// <param name="name">string; Name of cookie</param>
        public void Remove(string name)
        {
            var httpCookie = HttpContext.Current.Response.Cookies[name];
            if (httpCookie != null)
                httpCookie.Expires = DateTime.Now.AddDays(-1);
        }

        /// <summary>
        /// Check a cookie is exist by name
        /// </summary>
        /// <param name="name">string; name of cookie want to check </param>
        /// <returns>bool; true if cookie is exist</returns>
        public static bool IsExist(string name)
        {
            return (HttpContext.Current.Request.Cookies[name] != null);
        }
    }
}
