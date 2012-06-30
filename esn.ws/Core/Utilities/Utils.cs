using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace JK.Core.Utilities
{
    /// <summary>
    /// Utilities
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// check Email is valid
        /// </summary>
        /// <param name="email">string; Email to confirm</param>
        /// <returns></returns>
        public static bool EmailIsValid(this string email)
        {
            if (String.IsNullOrEmpty(email)) return false;
            var reg = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+){1,3}$");
            return reg.IsMatch(email);
        }

        /// <summary>
        /// Trim input String 
        /// </summary>
        /// <param name="input">string; Input to Trim</param>
        /// <returns></returns>
        public static string TrimSlash(this string input)
        {
            return input.Trim('/');
        }

        /// <summary>
        /// Check File upload is Image
        /// </summary>
        /// <param name="filename">string; file name</param>
        /// <returns>If is Image, return True</returns>
        public static bool IsImage(this string filename)
        {
            if (String.IsNullOrEmpty(filename)) return false;
            var reg = new Regex(".[jpg|png|gif]$");
            return reg.IsMatch(filename.ToLower());
        }

        /// <summary>
        /// check file upload is video
        /// </summary>
        /// <param name="filename">string; file name</param>
        /// <returns>If is Video, return True</returns>
        public static bool IsVideo(this string filename)
        {
            if (String.IsNullOrEmpty(filename)) return false;
            var reg = new Regex(".[flv|mp4|avi|mp3]$");
            return reg.IsMatch(filename.ToLower());
        }

        /// <summary>
        /// Get Date With Format
        /// </summary>
        /// <param name="dateInput">string; Date input from other site</param>
        /// <param name="culture">string; culture format</param>
        /// <returns>string; date out</returns>
        public static DateTime GetDateWithFormat(string dateInput, string culture)
        {
            if (!String.IsNullOrEmpty(dateInput))
            {
                DateTime dateOut;
                var myCulture = CultureInfo.CreateSpecificCulture(culture);
                DateTimeStyles style = DateTimeStyles.None;
                DateTime.TryParse(dateInput, myCulture, style, out dateOut);
                return dateOut;
            }
            throw new Exception("Parameter is null");
        }

        /// <summary>
        /// show messager
        /// </summary>
        /// <param name="message">string; message details </param>
        /// <param name="type">string; type of message
        ///     <example>
        ///         <c>error</c>
        ///     </example>
        /// </param>
        /// <returns></returns>
        public static string ShowMessager(string message, string type)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("<div class='notification {0}'>", type)
                .AppendFormat("<div>{0}</div>", message)
                .AppendLine("</div>");
            return builder.ToString();
        }

        /// <summary>
        /// check Null or Empty of string input
        /// </summary>
        /// <param name="input">string; string input</param>
        /// <returns>If null or empty, return true</returns>
        public static bool IsNotNullOrEmpty(this string input)
        {
            return (input != null && input.Length > 0);
        }
        public static bool IsNullOrEmpty(this string input)
        {
            return !input.IsNotNullOrEmpty();
        }

        /// <summary>
        /// Encryption to MD5
        /// </summary>
        /// <param name="input">string; input string</param>
        /// <returns>string output is encode</returns>
        public static string ToMD5(this string input)
        {
            var md5 = new MD5CryptoServiceProvider();
            var originalBytes = ASCIIEncoding.Default.GetBytes(input);
            var hashedBytes = md5.ComputeHash(originalBytes);

            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Encode Base 64
        /// </summary>
        /// <param name="input">string; text input</param>
        /// <returns></returns>
        public static string EncodeBase64(this string input)
        {
            try
            {
                var originalBytes = ASCIIEncoding.Default.GetBytes(input);
                return Convert.ToBase64String(originalBytes);
            }
            catch
            {

                return "";
            }
        }

        /// <summary>
        /// Decode Base 64
        /// </summary>
        /// <param name="base64String">string; text input</param>
        /// <returns></returns>
        public static string DecodeBase64(this string base64String)
        {
            try
            {
                var decodeBytes = Convert.FromBase64String(base64String);
            
            return ASCIIEncoding.ASCII.GetString(decodeBytes);
            }
            catch
            {
                return "";
            }
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <param name="lenght"></param>
        /// <param name="more"></param>
        /// <returns></returns>
        public  static string DescriptionEllipse(string description,int lenght = 255, string more ="...")
        {
           description = HttpContext.Current.Server.HtmlDecode(description);
          description =  HtmlRemoval.StripTagsCharArray(description);
            if(description.Length > lenght)
            {
                return description.Replace("\n"," ").Replace("&nbsp;"," ").Substring(0, lenght) + more;
            }
            return description;
        }

        /// <summary>
        /// Split int input
        /// </summary>
        /// <param name="seperator"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int[] SplitInt32(char seperator,string input)
        {
            var arr = input.Split(seperator);
            var outArr = new int[arr.Length];
            if (arr.Where((t, i) => !int.TryParse(t.Trim(),out outArr[i])).Any())
            {
                throw  new Exception("input string is invalid");
            }
            return outArr;
        }

        /// <summary>
        /// setting Redirect to any site
        /// </summary>
        /// <param name="url">string; url a site</param>
        /// <param name="ms">int; Timing</param>
        /// <param name="page">System.Web.UI.Page; page</param>
        public static void Redirect(string url, int ms, System.Web.UI.Page page)
        {
            string script = "<script type=\"text/javascript\">" +
                           "Redirect('" + url + "', " + ms + ");" +
                           "</script>";
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Redirect", script);
        }

        /// <summary>
        /// get text file 
        /// </summary>
        /// <param name="path">path of file</param>
        /// <returns>string; text content</returns>
        public static string GetFileText(string path)
        {
            using (var sr = new StreamReader(path))
            {
                string text = sr.ReadToEnd();
                sr.Close();
                return text;
            }
        }
        private static double ToRad(this double a)
        {
            return a*Math.PI/180;
        }
        public static double CalcDistance2Point(double lat1,double lon1, double lat2, double lon2)
        {
            const int r = 6371;//km -- ban kinh trai dat
            var dLat = Math.Abs(lat2 - lat1).ToRad();
            var dLon = Math.Abs(lon2 - lon1).ToRad();
            lat1 = lat1.ToRad();
            lat2 = lat2.ToRad();
            
            var a = Math.Sin(dLat/2)*Math.Sin(dLat/2)+
                    Math.Cos(lat1) * Math.Cos(lat2)*Math.Sin(dLon / 2) * Math.Sin(dLon / 2) ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return r * c;
        }
    }
}