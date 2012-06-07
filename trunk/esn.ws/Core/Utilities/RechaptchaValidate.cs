using System;
using System.IO;
using System.Net;
using System.Web;

namespace JK.Core.Utilities
{
    /// <summary>
    /// For validate recaptcha by webservice ReCaptchaValidation
    /// </summary>
    public class ReCaptchaValidation
    {
        private  string challenge;
        private  string ip;
        private  string privateKey;
        private  IWebProxy proxy;
        private  string response;
        private bool _errored;
        private Exception _ex;
        private string _vr;

        
        public ReCaptchaValidation(string clientIP, string privateKey,
                                   string challenge, string response)
            : this(null, clientIP, privateKey,
                   challenge, response)
        {
        }

        public ReCaptchaValidation(IWebProxy proxy, string clientIP,
                                   string privateKey, string challenge, string response)
        {
            this.proxy = proxy;
            ip = clientIP;
            this.privateKey = privateKey;
            this.challenge = challenge;
            this.response = response;
        }

        public bool IsErrored
        {
            get { return _errored; }
        }

        public Exception Exception
        {
            get { return _ex; }
        }

        public string ValidationResult
        {
            get { return _vr; }
        }

        public bool Validate()
        {
            try
            {
                string post = "privatekey=" + HttpUtility.UrlEncode(privateKey) +
                              "&remoteip=" + HttpUtility.UrlEncode(ip) + "&challenge=" +
                              HttpUtility.UrlEncode(challenge) + "&response=" +
                              HttpUtility.UrlEncode(response);

                WebRequest wr = WebRequest.Create
                    ("http://www.google.com/recaptcha/api/verify");
                wr.Method = "POST";

                if (proxy != null)
                    wr.Proxy = proxy;

                wr.ContentLength = post.Length;
                wr.ContentType = "application/x-www-form-urlencoded";
                using (var sw = new StreamWriter(wr.GetRequestStream()))
                {
                    sw.Write(post);
                    sw.Close();
                }

                var resp = (HttpWebResponse) wr.GetResponse();
                using (var sr = new StreamReader(resp.GetResponseStream()))
                {
                    string valid = sr.ReadLine();
                    if (valid != null)
                    {
                        if (valid.ToLower().Trim() == "false")
                        {
                            string errorcode = sr.ReadLine();

                            if (errorcode != null)
                            {
                                if (errorcode.ToLower().Trim() != "incorrect-captcha-sol")
                                {
                                    _vr = errorcode;
                                    _errored = true;
                                    return false;
                                }
                            }
                        }

                        return (valid.ToLower().Trim() == "true");
                    }
                    else _vr = "empty web service response";

                    sr.Close();
                    return false;
                }
            }
            catch (Exception caught)
            {
                _errored = true;
                _ex = caught;
            }
            return false;
        }
    }
}