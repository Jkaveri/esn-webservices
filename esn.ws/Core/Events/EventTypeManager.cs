using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JK.Core;
using Core.Apps;

namespace Core.Events
{
    public class EventTypeManager
    {
        public List<EventTypes> GetAll()
        {
            using (var db = new DB())
            {
                var eventTypes = new List<EventTypes>();
                db.Query("SELECT * FROM EventTypes WHERE STATUS = @status",GeneralStatus.Active);
                if(db.HasResult)
                {
                    while (db.Read())
                    {
                        var type = new EventTypes();
                        db.SetValues(type);
                       eventTypes.Add(type);
                    }
                }
                return eventTypes;
            }
        } 
    }
}
