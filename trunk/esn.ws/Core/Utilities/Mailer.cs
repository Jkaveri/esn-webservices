using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Mail;
using System.Web;
namespace JK.Core.Utilities
{
    /// <summary>
    /// Mailer
    /// </summary>
    public class Mailer
    {
        /// <summary>
        /// use html in body
        /// </summary>
        private bool _useHtml = true;
        /// <summary>
        /// site settings
        /// </summary>
        private readonly SiteSettings _setting;
        /// <summary>
        /// event mail sent
        /// </summary>
        public event EventHandler MailSent;

        /// <summary>
        /// get information from SiteSetting  
        /// </summary>
        public Mailer()
        {
            _setting = SiteSettings.GetInstance();
        }

        /// <summary>
        /// Send Mail
        /// </summary>
        /// <param name="subsject">string; Subject of mail</param>
        /// <param name="from">string; email from</param>
        /// <param name="message">string; message of mail</param>
        /// <returns>if true, process is Success</returns>
        public bool Send(string subsject, string from, string message)
        {
            using (var mm = new MailMessage(from, _setting.SiteMail))
            {
                mm.Subject = subsject;
                mm.IsBodyHtml = UseHtml;
                mm.Body = message;
                var replyto = new MailAddress(from ?? SiteSettings.GetInstance().SiteMail);
                mm.ReplyTo = replyto;
                SendMailMessage(mm);
            }

            return true;
        }

        /// <summary>
        /// Send mail by mail template
        /// </summary>
        /// <param name="template">template mail</param>
        /// <returns>true if success</returns>
        public bool Send(MailTemplate template)
        {

            string fileText = Utils.GetFileText(HttpContext.Current.Server.MapPath(template.TemplateName));
            fileText = TemplateEngine.Evaluate(fileText, template.Context);

            using (var mm = new MailMessage(template.From ?? SiteSettings.GetInstance().SiteMail, template.To))
            {
                mm.Subject = template.Subject;
                mm.IsBodyHtml = template.IsHTML;
                mm.Body = fileText;
                var replyto = new MailAddress(template.From??SiteSettings.GetInstance().SiteMail);
                mm.ReplyTo = replyto;
                SendMailMessage(mm);
            }
            return true;

        }
        /// <summary>
        /// Send Mail Messager with mail message
        /// use smpt
        /// </summary>
        /// <param name="mm">MailMessage; Mail details</param>
        /// <returns></returns>
        private bool SendMailMessage(MailMessage mm)
        {
            try
            {
                using (mm)
                {
                    var client = new SmtpClient();

                    client.Host = _setting.SmtpHost;
                    if (_setting.SmtpRequireAuthentication)
                        client.Credentials = new NetworkCredential(_setting.SmtpUsername, _setting.SmtpPassword);

                    if (_setting.SmtpEnableSsl)
                        client.EnableSsl = true;

                    if (_setting.SmtpPort > 0)
                        client.Port = _setting.SmtpPort;

                    client.Send(mm);

                    if (MailSent != null)
                    {
                        MailSent(this, new EventArgs());
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// Use HTML of Mailer 
        /// </summary>
        public bool UseHtml
        {
            get { return _useHtml; } 
            set { _useHtml = value; }
        }
    }
}
