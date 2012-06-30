using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Apps;
using JK.Core;

namespace Core.Events
{
   public class Events:JKBean
    {
       public int EventID { get; set; }
       public int AccID { get; set; }
       public int EventTypeID { get; set; }
       public string Title { get; set; }
       public string Description { get; set; }
       public string Picture { get; set; }
       public DateTime DayCreate { get; set; }
       public double EventLat { get; set; }
       public double EventLng { get; set; }
       public ShareTypes ShareType { get; set; }
       public int Like { get; set; }
       public int Dislike { get; set; }
       public GeneralStatus Status { get; set; }
       public Events()
       {
           SetModule(this);
           PrimaryKeyName = "EventID";
       }
    }
}
