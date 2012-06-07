using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JK.Core;

namespace Core.Accounts
{
    public class RelationType:JKBean
    {
        public int RelationTypeID { get; set; }
        public string RelationTypeName { get; set; }
        public int ShareID { get; set; }
        public int Status { get; set; }
    }

}
