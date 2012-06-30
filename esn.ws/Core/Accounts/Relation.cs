using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Apps;
using JK.Core;

namespace Core.Accounts
{
    public class Relation:JKBean
    {
        public int RelationID { get; set; }
        public int AccID { get; set; }
        public int FriendID { get; set; }
        public RelationType RelationType { get; set; }
        public DateTime DayCreate { get; set; }
        public RelationStatus Status { get; set; }
        public Relation()
        {
            SetModule(this);
            PrimaryKeyName = "RelationID";
        }
    }
}
