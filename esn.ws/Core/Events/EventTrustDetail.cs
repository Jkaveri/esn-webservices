using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Events
{
    public class EventTrustDetail
    {
        public int EventID { get; set; }
        public int AccID { get; set; }
        public int Like { get; set; }
        public int Dislike { get; set; }
        public bool Deleted { get; set; }
    }
}
