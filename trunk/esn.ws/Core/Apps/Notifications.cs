using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JK.Core;

namespace Core.Apps
{
    public class Notifications:JKBean
    {
        public int AccID { get; set; }
        public int ReceiveID { get; set; }
        public int TargetID { get; set; }
        public string Description { get; set; }
        public DateTime DateCreate { get; set; }
        public TargetTypes TargetType { get; set; }
        public NotificationStatus Status { get; set; }
        public Notifications()
        {
            SetModule(this);
        }
    }
}
