using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JK.Core;

namespace Core.Accounts
{
    public class Relation:JKBean
    {
        public int RelationID { get; set; }
        public int AccID { get; set; }
        public int FriendID { get; set; }
        public int RelationTypeID { get; set; }
        public DateTime DayCreate { get; set; }
        public RelationStatus Status { get; set; }
        public override bool Delete(bool forever = false)
        {
            return base.Delete(true);
        }
    }
}
