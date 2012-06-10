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
        private readonly EventsManager manager = new EventsManager();

        [WebMethod]
        public int CreateEvent(int accountID, int eventTypeID, string title, string description, string picture, double latitude, double longitude, ShareTypes shareType)
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
                                 Status = false
                             };
            if (_event.Save())
            {
                return _event.ID;
            }
            return 0;
        }
        [WebMethod]
        public List<Events> GetAvailableEvents(int pageNum, int pageSize)
        {
            try
            {
                return manager.GetAvailableEvents(pageNum,pageSize);
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
                return manager.GetListEventsAround(lat, lon, radius);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}