using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JK.Core;

namespace Core.Apps
{
    public class NotificationsManager
    {
        public List<Notifications> GetUnReadNotifications(int accountID)
        {
            using (var db = new DB())
            {
                var notifications = new List<Notifications>();
                db.Query("SELECT * FROM Notifications WHERE ReceiveID = @Receive AND [Status] = @status", accountID,
                         NotificationStatus.UnRead);
                while (db.Read())
                {
                    var notify = new Notifications();
                    db.SetValues(notify);
                    notifications.Add(notify);
                }
                return notifications;
            }
        }

        public List<Notifications> GetListNotifications(int accountID, int pageNum, int pageSize)
        {
            using (var db = new DB())
            {
                var notifications = new List<Notifications>();
                db.Query("SELECT * FROM Notifications WHERE ReceiveID = @Receive ORDER BY DateCreate DESC", accountID);
                var rowNum = 1;
                while (db.Read())
                {
                    if (rowNum++ > pageNum*pageSize) continue;
                    var notify = new Notifications();
                    db.SetValues(notify);
                    notifications.Add(notify);
                }
                return notifications;
            }
        }
    }
}