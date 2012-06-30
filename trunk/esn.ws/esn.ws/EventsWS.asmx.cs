using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Services;
using Core.Apps;
using Core.Events;

namespace esn.ws
{
    /// <summary>
    /// Summary description for EventsWS
    /// </summary>
    [WebService(Namespace = "http://esn.com.vn/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class EventsWS : WebService
    {
        private readonly EventsManager _eventManager = new EventsManager();
        private readonly EventTypeManager _eventTypeManager = new EventTypeManager();

        [WebMethod]
        public List<EventTypes> GetListEventTypes()
        {
            return _eventTypeManager.GetAll();
        }

        [WebMethod]
        public int CreateEvent(int accountID, int eventTypeID, string title, string description, string picture,
                               double latitude, double longitude, ShareTypes shareType)
        {
            var _event = new Events
                             {
                                 AccID = accountID,
                                 EventTypeID = eventTypeID,
                                 Title = title,
                                 Description = description,
                                 Picture = picture,
                                 DayCreate = DateTime.Now,
                                 EventLat = latitude,
                                 EventLng = longitude,
                                 ShareType = shareType,
                                 Status = GeneralStatus.Active
                             };
            if (_event.Save())
            {
                return _event.EventID;
            }
            return 0;
        }

        [WebMethod]
        public List<Events> GetAvailableEvents(int pageNum, int pageSize)
        {
            try
            {
                return _eventManager.GetAvailableEvents(pageNum, pageSize);
            }
            catch (Exception)
            {
                return null;
            }
        }

        [WebMethod]
        public List<Events> GetListEventsAround(double lat, double lon, int radius)
        {
            try
            {
                return _eventManager.GetListEventsAround(lat, lon, radius);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [WebMethod]
        public List<Events> GetListFriendEvents(int pageNum, int pageSize)
        {
            return null;
        }

        [WebMethod]
        public int Like(int eventId, int accountId)
        {
            var detail = new LikeDetails();
            detail.AccID = accountId;
            detail.EventID = eventId;
            detail.Status = LikeDetailStatus.Like;
            if (detail.IsUpdate) return 0;
           
            var _event = new Events();
            _event.Retrieve(eventId);
            if(_event.EventID>0)
            {
                _event.Like++;
                if (_event.Save(true))
                {
                    if(detail.Save(false))
                    {
                        return _event.Like;
                    }
                    _event.Like--;
                    _event.Save(true);
                }
            }
            return 0;
        }
        [WebMethod]
        public int Dislike(int eventId, int accountId)
        {
            var detail = new LikeDetails {AccID = accountId, EventID = eventId, Status = LikeDetailStatus.Dislike};
            if (detail.IsUpdate) return 0;

            var _event = new Events();
            _event.Retrieve(eventId);
            if (_event.EventID > 0)
            {
                _event.Dislike++;
                if (_event.Save(true))
                {
                    if (detail.Save(false))
                    {
                        return _event.Dislike;
                    }
                    _event.Dislike--;
                    _event.Save(true);
                }
            }
            return 0;
        }

        [WebMethod]
        public object RetrieveJSON(int id)
        {
            Events _event = new Events();
            _event.Retrieve(id);
            return new
                       {
                           EventID = _event.EventID,
                           AccID = _event.AccID,
                           EventTypeID = _event.EventTypeID,
                           Title = _event.Title,
                           Description = _event.Description,
                           Picture = _event.Picture,
                           DayCreate = _event.DayCreate,
                           EventLat = _event.EventLat,
                           EventLng = _event.EventLng,
                           ShareType = _event.ShareType,
                           Like = _event.Like,
                           Dislike = _event.Dislike,
                           Status = _event.Status
                       };
        }
        [WebMethod]
        public Events Retrieve(int id)
        {
            Events _event = new Events();
            _event.Retrieve(id);
            return _event;
        }
        [WebMethod]
        public bool IsLiked(int eventId, int accountId)
        {
            var detail = new LikeDetails { AccID = accountId, EventID = eventId, Status = LikeDetailStatus.Dislike };
            return detail.IsLiked;
        }
        [WebMethod]
        public bool IsDislike(int eventId, int accountId)
        {
            var detail = new LikeDetails { AccID = accountId, EventID = eventId, Status = LikeDetailStatus.Dislike };
            return detail.IsDisliked;
        }
    }
}