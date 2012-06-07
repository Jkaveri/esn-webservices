using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NVelocity;

namespace JK.Core.Utilities
{
    /// <summary>
    /// Mail template 
    /// </summary>
    public class MailTemplate
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public MailTemplate()
        {
            Context = new EmailTemplateToolboxContext();
        }

        /// <summary>
        /// tempalte name property
        /// </summary>
        public string TemplateName { get; set; }
        /// <summary>
        /// subject of mail
        /// </summary>
        private string _subject;
        /// <summary>
        /// Subject mail template
        /// </summary>
        public string Subject
        {
            get { return _subject; }
            set
            {
                Context.Put("subject", value);
                _subject = value;
            }
        }
        /// <summary>
        /// mail to
        /// </summary>
        private string _to;
        /// <summary>
        /// mail to
        /// </summary>
        public string To
        {
            get { return _to; }
            set
            {
                Context.Put("to", value);
                _to = value;
            }
        }
        /// <summary>
        /// mail from
        /// </summary>
        private string _from;
        /// <summary>
        /// mail From
        /// </summary>
        public string From
        {
            get { return _from; }
            set
            {
                Context.Put("from", value);
                _from = value;
            }
        }
        /// <summary>
        /// Message
        /// </summary>
        private string _message;
        /// <summary>
        /// message
        /// </summary>
        public string Message
        {
            get { return _message; }
            set
            {
                Context.Put("message", value);
                _message = value;
            }
        }
        /// <summary>
        /// Mail Template engine inherite by VelocityContext
        /// </summary>
        public EmailTemplateToolboxContext Context { get; set; }
        /// <summary>
        /// is html default is true
        /// </summary>
        private bool _isHtml = true;
        /// <summary>
        /// Is html
        /// </summary>
        public bool IsHTML
        {
            get { return _isHtml; }
            set { _isHtml = value; }
        }


    }
    /// <summary>
    /// Mail template engine inherite by Velocity Context
    /// </summary>
    public class EmailTemplateToolboxContext : VelocityContext
    {
        /// <summary>
        /// initialize
        /// </summary>
        public EmailTemplateToolboxContext()
        {
            Put("date", DateTime.Now);
            Put("settings", SiteSettings.GetInstance());
        }
    }
}
