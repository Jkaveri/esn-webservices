using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JK.Core;

namespace Core.Events
{
   public class EventTypes:JKBean
    {
       public int EventTypeID { get; set; }
       public string EventTypeName { get; set; }
       public string LabelImage { get; set; }
       public int Time { get; set; }
       public string Slug { get; set; }
       public int Status { get; set; }
       public EventTypes()
       {
           SetModule(this);
           PrimaryKeyName = "EventTypeID";
       }
    }
}
