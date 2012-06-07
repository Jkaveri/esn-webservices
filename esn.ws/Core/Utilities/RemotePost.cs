using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JK.Core.Utilities
{
    /// <summary>
    /// Remote Port
    /// </summary>
    public class RemotePost
    {
        private System.Collections.Specialized.NameValueCollection Inputs 
            = new System.Collections.Specialized.NameValueCollection();

        public string Url = "";
        public string Method = "post";
        public string FormName = "form1";
        public bool UseRsa { get; set; }
        /// <summary>
        /// Add Port
        /// </summary>
        /// <param name="name">string; Name port</param>
        /// <param name="value">string; value of port</param>
        public void Add(string name, string value)
        {
            Inputs.Add(name, value);
        }

        /// <summary>
        /// Setting Port
        /// </summary>
        public void Post()
        {
            HttpContext.Current.Response.Clear();

            HttpContext.Current.Response.Write("<html><head>");

            HttpContext.Current.Response.Write(string.Format("</head><body onload=\"document.{0}.submit()\">", FormName));

            HttpContext.Current.Response.Write(string.Format("<form name=\"{0}\" method=\"{1}\" action=\"{2}\" >",

           FormName, Method, Url));
            for (var i = 0; i < Inputs.Keys.Count; i++)
            {
                var value = (UseRsa) ? CryptoRsa.Encrypt(Inputs[Inputs.Keys[i]]) : Inputs[Inputs.Keys[i]];
                HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", Inputs.Keys[i], value));
            }
            HttpContext.Current.Response.Write("</form>");
            HttpContext.Current.Response.Write("</body></html>");
            HttpContext.Current.Response.End();
        }
    }
}