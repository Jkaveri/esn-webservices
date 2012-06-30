using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JK.Core;

namespace Core.Apps
{
    public class LikeDetails : JKBean
    {
        public int EventID { get; set; }
        public int AccID { get; set; }
        public LikeDetailStatus Status { get; set; }

        public LikeDetails()
        {
            SetModule(this);
        }

        public override bool IsUpdate
        {
            get
            {
                if (EventID > 0 && AccID > 0)
                {
                    using (DB db = new DB())
                    {
                        db.Query("SELECT ID FROM LikeDetails WHERE EventID = @eventid AND AccID = @accID", EventID,
                                 AccID);
                        return db.HasResult;
                    }
                }
                return false;
            }
        }

        public bool IsLiked { 
            get
            {
                if (EventID > 0 && AccID > 0)
                {
                    using (var db = new DB())
                    {
                        db.Query("SELECT ID FROM LikeDetails WHERE EventID = @eventid AND AccID = @accID AND Status = @status",EventID,AccID,LikeDetailStatus.Like);
                        return db.HasResult;
                    }
                }
                throw new Exception("EventID and AccID must be setted");
            } 
        }
        public bool IsDisliked
        {
            get
            {
                if (EventID > 0 && AccID > 0)
                {
                    using (var db = new DB())
                    {
                        db.Query("SELECT ID FROM LikeDetails WHERE EventID = @eventid AND AccID = @accID AND Status = @status", EventID, AccID, LikeDetailStatus.Dislike);
                        return db.HasResult;
                    }
                }
                throw new Exception("EventID and AccID must be setted");
            }
        }
    }
}