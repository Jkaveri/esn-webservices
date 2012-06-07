using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Accounts
{
   public enum AccountStatus
    {
        NotConfirmed = 0,
        Confirmed = 1,
        Locked = 2
    }
   public enum AccountRoles
   {
       Admin = 1,
       Moderator = 2,
       User = 3
   }
    public enum RelationStatus
    {
        Pending = 0,
        Confirmed = 1
    }
}
