using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JK.Core.Utilities
{
    /// <summary>
    /// Mail SMTP
    /// </summary>
    public class MailSMTP
    {
        /// <summary>
        /// host SMTP
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Port SMTP
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Username SMTP
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password of Username SMTP
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Enable SSL
        /// </summary>
        public bool EnableSSL { get; set; }
    }
}
