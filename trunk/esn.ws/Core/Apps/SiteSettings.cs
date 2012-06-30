using System;

namespace JK.Core
{
    public partial class SiteSettings
    {
        #region Site

        /// <summary>
        /// Url of website ex:http://www.domain.com
        /// </summary>
        public string SiteUrl
        {
            get { return GetValue("ESN::Site::SiteUrl"); }
            set { SetValue("ESN::Site::SiteUrl", value); }
        }

        #endregion

        #region Read

        /// <summary>
        /// Setting Entity page Site
        /// </summary>
        public int EntitiesPerSite
        {
            get
            {
                string str = GetValue("ESN::Read::EntitiesPerPage");
                return Int32.Parse(str);
            }
            set { SetValue("ESN::Read::EntitiesPerPage", value.ToString()); }
        }

        /// <summary>
        /// Setting Default Picture
        /// </summary>
        public string DefaultPicture
        {
            get { return GetValue("ESN::Site::DefaultPicture"); }
            set { SetValue("ESN::Site::DefaultPicture", value); }
        }

        #endregion

        #region Mail

        /// <summary>
        /// Setting  Site Mail
        /// </summary>
        public string SiteMail
        {
            get { return GetValue("ESN::Mail::SiteMail"); }
            set { SetValue("ESN::Mail::SiteMail", value); }
        }

        /// <summary>
        /// Setting SMTP host
        /// </summary>
        public string SmtpHost
        {
            get { return GetValue("ESN::Mail::SmtpHost"); }
            set { SetValue("ESN::Mail::SmtpHost", value); }
        }

        /// <summary>
        /// Setting SMTP port
        /// </summary>
        public int SmtpPort
        {
            get { return Int32.Parse(GetValue("ESN::Mail::SmtpPort")); }
            set { SetValue("ESN::Mail::SmtpPort", value.ToString()); }
        }

        /// <summary>
        /// Setiing Enable SSL
        /// </summary>
        public bool SmtpEnableSsl
        {
            get { return Boolean.Parse(GetValue("ESN::Mail::SmtpEnableSsl")); }
            set { SetValue("ESN::Mail::SmtpEnableSsl", value.ToString()); }
        }


        /// <summary>
        /// Setting Require Authentication
        /// </summary>
        public bool SmtpRequireAuthentication
        {
            get { return Boolean.Parse(GetValue("ESN::Mail::SmtRequireAuthentication")); }
            set { SetValue("ESN::Mail::SmtRequireAuthentication", value.ToString()); }
        }

        /// <summary>
        /// setting Username of SMTP
        /// </summary>
        public string SmtpUsername
        {
            get { return GetValue("ESN::Mail::SmtpUsername"); }
            set { SetValue("ESN::Mail::SmtpUsername", value); }
        }

        /// <summary>
        /// setting password of username
        /// </summary>
        public string SmtpPassword
        {
            get { return GetValue("ESN::Mail::SmtpPassword"); }
            set { SetValue("ESN::Mail::SmtpPassword", value); }
        }

        public string TemplateFileDirectory
        {
            get { return GetValue("ESN::Mail::TemplateFileDirectory"); }
            set { SetValue("ESN::Mail::TemplateFileDirectory", value); }
        }

        public string ChangePasswordTemplate
        {
            get { return GetValue("ESN::Mail::ChangePasswordTemplate"); }
            set { SetValue("ESN::Mail::ChangePasswordTemplate", value); }
        }

        #endregion

        #region events

        public int EventMaxAge
        {
            get { return int.Parse(GetValue("ESN::Event::MaxAge")); }
            set { SetValue("ESN::Event::MaxAge", value.ToString()); }
        }
        public int LikeDislikeEffect
        {
            get { return int.Parse(GetValue("ESN::Event::LikeDislikeEffect")); }
            set { SetValue("ESN::Event::LikeDislikeEffect", value.ToString()); }
        }

        #endregion

    }
}