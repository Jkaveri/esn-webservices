using System;
using System.Collections.Generic;
using System.Threading;
using Core.Apps;
using JK.Core;
using JK.Core.Utilities;

namespace Core.Events
{
    public class EventsManager
    {
        private readonly SiteSettings settings;

        public EventsManager()
        {
            settings = SiteSettings.GetInstance();
        }

        public List<Events> GetListEventsAround(double lat1, double lon1, int radius)
        {
            var events = new List<Events>();
            using (var db = new DB())
            {
                var query = String.Format("SELECT  e.*,et.EventTypeName, et.[Time] " +
                                          "FROM [Events] e JOIN EventTypes et " +
                                          "ON e.EventTypeID = et.EventTypeID Where e.Status = {0:D} AND et.Status = {0:D} ", GeneralStatus.Active);
                db.Query(query);
                if (db.HasResult)
                {
                    while (db.Read())
                    {
                        //so like
                        int like = db.GetInt32("Like");
                        //so dislike
                        int dislike = db.GetInt32("Dislike");
                        //ngay tao
                        DateTime dayCreate = db.GetDateTime("DayCreate");
                        //thoi gian mac dinh cua moi event
                        int eventTypeTime = db.GetInt32("Time");
                        //thoi gian ma event do' se ton tai(dd/mm/yyy hh:mm:ss)
                        //= ngayTao + (like-dislike)*LikeDisLikeEffect +eventTypeTime (tinh theo phut)
                        DateTime deathTime =
                            dayCreate.AddMinutes((like - dislike)*settings.LikeDislikeEffect + eventTypeTime);
                        if (deathTime > db.Now)
                        {
                            double lat2 = db.GetDouble("EventLat");
                            double lon2 = db.GetDouble("EventLng");
                            if (Utils.CalcDistance2Point(lat1, lon1, lat2, lon2) < radius)
                            {
                                //neu event con ton tai thi cho vao list
                                var mEvent = new Events();
                                db.SetValues(mEvent);
                                events.Add(mEvent);
                            }
                        }
                    }
                }
            }
            return events;
        }

        public List<Events> GetAvailableEvents(int pageNum, int pageSize)
        {
            var events = new List<Events>();
            using (var db = new DB())
            {
                const string query = "SELECT  e.*,et.EventTypeName, et.[Time] " +
                                     "FROM [Events] e JOIN EventTypes et " +
                                     "ON e.EventTypeID = et.EventTypeID Where e.[Status] = 1";
                string delete = "(";
                db.Query(query);
                if (db.HasResult)
                {
                    int rowNum = 1;
                    while (db.Read())
                    {
                        //so like
                        int like = db.GetInt32("Like");
                        //so dislike
                        int dislike = db.GetInt32("Dislike");
                        //ngay tao
                        DateTime dayCreate = db.GetDateTime("DayCreate");
                        //thoi gian mac dinh cua moi event
                        int eventTypeTime = db.GetInt32("Time");
                        //thoi gian ma event do' se ton tai(dd/mm/yyy hh:mm:ss)
                        //= ngayTao + (like-dislike)*LikeDisLikeEffect +eventTypeTime (tinh theo phut)
                        DateTime deathTime =
                            dayCreate.AddMinutes((like - dislike)*settings.LikeDislikeEffect + eventTypeTime);
                        if (deathTime > db.Now)
                        {
                            if (rowNum++ <= pageNum*pageSize || pageNum == 0)
                            {
                                //neu event con ton tai thi cho vao list
                                var mEvent = new Events();
                                db.SetValues(mEvent);
                                events.Add(mEvent);
                            }
                        }
                        else
                        {
                            if (delete != "(") delete += ",";
                            delete += db.GetInt32("EventID");
                        }
                    }
                    delete += ")";
                    if (!delete.Equals("()"))
                    {
                        new Thread(delegate()
                                       {
                                           using (var db2 = new DB())
                                           {
                                               db2.ExecuteUpdate(
                                                   "UPDATE Events Set [Status] = @Inactive WHERE EventID in " +
                                                   delete, GeneralStatus.Inactive);
                                           }
                                       }).Start();
                    }
                }
            }
            return events;
        }

        public List<Events> GetListFriendEvents(int pageNum, int pageSize)
        {
            var events = new List<Events>();
            using (var db = new DB())
            {
                const string query =
                    "SELECT Events.*, EventTypes.EventTypeName, EventTypes.Time, Profiles.AccID AS FriendID, Profiles.Username, Profiles.Name, Profiles.Avatar " +
                    "FROM Events INNER JOIN EventTypes " +
                    "ON Events.EventTypeID = EventTypes.EventTypeID " +
                    "CROSS JOIN Relation CROSS JOIN Profiles " +
                    "WHERE (Relation.FriendID = 1)";
                string delete = "(";
                db.Query(query);
                if (db.HasResult)
                {
                    int rowNum = 1;
                    while (db.Read())
                    {
                        //so like
                        int like = db.GetInt32("Like");
                        //so dislike
                        int dislike = db.GetInt32("Dislike");
                        //ngay tao
                        DateTime dayCreate = db.GetDateTime("DayCreate");
                        //thoi gian mac dinh cua moi event
                        int eventTypeTime = db.GetInt32("Time");
                        //thoi gian ma event do' se ton tai(dd/mm/yyy hh:mm:ss)
                        //= ngayTao + (like-dislike)*LikeDisLikeEffect +eventTypeTime (tinh theo phut)
                        DateTime deathTime =
                            dayCreate.AddMinutes((like - dislike)*settings.LikeDislikeEffect + eventTypeTime);
                        if (deathTime > db.Now)
                        {
                            if (rowNum++ <= pageNum*pageSize || pageNum == 0)
                            {
                                //neu event con ton tai thi cho vao list
                                var mEvent = new Events();
                                db.SetValues(mEvent);
                                events.Add(mEvent);
                            }
                        }
                        else
                        {
                            if (delete != "(") delete += ",";
                            delete += db.GetInt32("EventID");
                        }
                    }
                    delete += ")";
                    if (!delete.Equals("()"))
                    {
                        new Thread(delegate()
                                       {
                                           using (var db2 = new DB())
                                           {
                                               db2.ExecuteUpdate("UPDATE Events Set [Status] = 1 WHERE EventID in " +
                                                                 delete);
                                           }
                                       }).Start();
                    }
                }
            }
            return events;
        }
    }
}