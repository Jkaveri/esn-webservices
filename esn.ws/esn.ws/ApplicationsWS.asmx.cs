using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Core.Apps;

namespace esn.ws
{
    /// <summary>
    /// Summary description for ApplicationsWS
    /// </summary>
    [WebService(Namespace = "http://esn.com.vn/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class ApplicationsWS : System.Web.Services.WebService
    {
        private NotificationsManager manager = new NotificationsManager();
        [WebMethod]
        public List<Notifications> GetUnReadNotifications(int accountID)
        {
            try
            {
                return manager.GetUnReadNotifications(accountID);
            }
            catch (Exception)
            {

                return null;
            }
        }
       [WebMethod]
        public List<Notifications> GetListNotifications(int accountID,int pageNum, int pageSize)
       {
           return manager.GetListNotifications(accountID,pageNum,pageSize);
       }
        [WebMethod]
        public void SetNotifyReaded(int notifyID)
        {
            var notify = new Notifications();
            if(notify.Retrieve(notifyID))
            {
                notify.Status = NotificationStatus.Read;
                notify.Save(true);
            }
        } 
    }
}
