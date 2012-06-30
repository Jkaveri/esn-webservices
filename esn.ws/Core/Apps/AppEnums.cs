using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Apps
{
    public enum LikeDetailStatus
    {
        Dislike = 0,
        Like = 1,
        Deleted = 3,
    }
    public enum GeneralStatus
    {
        Active = 1,
        Inactive = 2,
        Deleted = 3,
    }
    public enum ShareTypes
    {
        Private = 0,
        Public = 1,
        Custom = 2
    }
   public enum AccountStatus
    {
        NotConfirmed = 0,
        Confirmed = 1,
        Locked = 2,
        Deleted = 3,
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
    public enum RelationType
    {
        Friend = 1,
        Family = 2,
    }
    public enum NotificationStatus
    {
        UnRead = 0,
        Read = 1
    }
    public enum TargetTypes
    {
        Relation = 0,
        Events = 1,
        Comment = 2,
    }
}
