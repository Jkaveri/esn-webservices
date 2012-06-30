using System;
using System.Collections.Generic;
using System.Text;
using Core.Apps;
using JK.Core;
using JK.Core.Utilities;

namespace Core.Accounts
{
    public class AccountManager
    {
        public int Register(string name, string email, string password, DateTime birthday,
                                 string phone, bool gender, string access_token)
        {
            var account = new Accounts
                              {
                                  Email = email,
                                  Password = password.ToMD5(),
                                  RoleID = AccountRoles.User,
                                  AccessToken = access_token,
                                  VerificationCode = Guid.NewGuid().ToString().Replace("-", "").ToLower(),
                                  DateCreated = DateTime.Now,
                                  IsOnline = true,
                                  Status = AccountStatus.NotConfirmed
                              };
            if (account.Save())
            {
                var profile = new Profiles
                                  {
                                      AccID = account.AccID,
                                      Name = name,
                                      Birthday = birthday,
                                      Phone = phone,
                                      Gender = gender
                                  };
                if (profile.Save())
                {
                    return account.AccID;
                }
                return 0;
            }
            return 0;
        }

        public List<Profiles> GetListFriends(int accountID, int pageNum, int pageSize)
        {
            var list = new List<Profiles>();
            using (var db = new DB())
            {
                var query = new StringBuilder();
                query.Append("SELECT * FROM ( ")
                    .AppendFormat(
                        "SELECT TOP ({0:D} * {1:D}) ResultNum = ROW_NUMBER() over ( order by AP.Name ASC ), AP.*  ",
                        pageNum, pageSize)
                    .Append("FROM Profiles AP JOIN Relation R  ")
                    .Append("ON AP.AccID = R.AccID JOIN Profiles FP ")
                    .Append("ON R.FriendID = FP.AccID  ")
                    .Append("WHERE R.FriendID = @ID) as PAGINATED ")
                    .AppendFormat("WHERE ResultNum > (( {0:D} -1)*{1:D}) ", pageNum, pageSize);
                db.Query(query.ToString(), accountID);
                if (db.HasResult)
                {
                    while (db.Read())
                    {
                        var profiles = new Profiles();
                        db.SetValues(profiles);
                        list.Add(profiles);
                    }
                }
            }
            return list;
        }
        public List<object> GetListFriendsJSON(int accountID, int pageNum, int pageSize)
        {
            var list = new List<object>();
            using (var db = new DB())
            {
                var query = new StringBuilder();
                query.Append("SELECT * FROM ( ")
                    .AppendFormat(
                        "SELECT TOP ({0:D} * {1:D}) ResultNum = ROW_NUMBER() over ( order by AP.Name ASC ), AP.*  ",
                        pageNum, pageSize)
                    .Append("FROM Profiles AP JOIN Relation R  ")
                    .Append("ON AP.AccID = R.AccID JOIN Profiles FP ")
                    .Append("ON R.FriendID = FP.AccID  ")
                    .Append("WHERE R.FriendID = @ID) as PAGINATED ")
                    .AppendFormat("WHERE ResultNum > (( {0:D} -1)*{1:D}) ", pageNum, pageSize);
                db.Query(query.ToString(), accountID);
                if (db.HasResult)
                {
                    while (db.Read())
                    {
                        var profiles = new
                                           {
                                                   AccID = db.GetInt32("AccID"),
                                                   Username = db.GetString("Username"),
                                                   Name = db.GetString("Name"),
                                                   Avatar = db.GetString("Avatar"),
                                                   Phone = db.GetString("Phone")
                                           };
                        list.Add(profiles);
                    }
                }
            }
            return list;
        }

        public bool ChangePassword(string email, string oldPassword, string newPassword)
        {
            //set email, password for account
            var account = new Accounts {Email = email, Password = oldPassword};
            //invoke login method to get full account info (by email, password)
            account = account.Login();
            //new login thanh cong
            if (account != null)
            {
                //set password moi
                account.Password = newPassword.ToMD5();
                //luu vao db
                if (account.Save(true))
                {
                    //gui mail thong bao cho user do biet
                    var mailer = new Mailer();
                    var template = new MailTemplate();
                    SiteSettings settings = SiteSettings.GetInstance();
                    //get profile
                    Profiles profile = account.Profile;
                    //add profile to template context for $profile variable
                    template.Context.Put("profile", profile);
                    //add Now to template context for $date variable
                    template.Context.Put("date", DateTime.Now);
                    //set from
                    template.From = settings.SiteMail;
                    //set to
                    template.To = email;
                    //set subject
                    template.Subject = "Your email was changed!";
                    //set template file name
                    template.TemplateName = settings.TemplateFileDirectory + settings.ChangePasswordTemplate;
                    //send mail
                    if (mailer.Send(template))
                    {
                        //neu send thanh cong thi return tru
                        return true;
                    }
                    //khong thanh cong thi set lai password cu 
                    account.Password = oldPassword.ToMD5();
                    account.Save(true);
                    //thong bao that bai
                    return false;
                }
            }
            return false;
        }

        public bool AddFriend(int accID, int friendID)
        {
            //kiem tra xem account co ton tai hay ko 
            var account = new Profiles();
            var friend = new Profiles();
            //neu ton tai
            if (account.Retrieve(accID) && friend.Retrieve(friendID))
            {
                //tao 1 relation 
                var relation = new Relation
                                   {
                                       AccID = accID,
                                       FriendID = friendID,
                                       RelationType = RelationType.Friend,
                                       DayCreate = DateTime.Now,
                                       Status = RelationStatus.Pending
                                   };
                //save relation do
                if (relation.Save(false))
                {
                    //tao 1 notification cho nguoi duoc add ban
                    var notification = new Notifications
                                           {
                                               AccID = accID,
                                               ReceiveID = friendID,
                                               TargetID = relation.RelationID,
                                               TargetType = TargetTypes.Relation,
                                               Description =
                                                   String.Format("{0:S} Muon ket ban voi ban!", account.Name),
                                               DateCreate = DateTime.Now,
                                               Status = NotificationStatus.UnRead
                                           };
                    //save notification do
                    if (notification.Save(false))
                    {
                        //thanh cong
                        return true;
                    }
                    //ko thanh cong thi xoa relation
                    relation.Delete(true);
                    //thong bao that bai
                    return false;
                }
                return false;
            }
            return false;
        }

        public bool ConfirmRelationRequest(int relationID)
        {
            var relation = new Relation();
            //get relation to confirm
            relation.Retrieve(relationID);
            //init new relation for confirm
            var confirmRelation = new Relation
                                      {
                                          FriendID = relation.AccID,
                                          AccID = relation.FriendID,
                                          RelationType = relation.RelationType,
                                          DayCreate = DateTime.Now,
                                          Status = RelationStatus.Confirmed
                                      };
            //save new relation of this request
            //if save success then update realtion status for request relation
            if (confirmRelation.Save(false))
            {
                relation.Status = RelationStatus.Confirmed;
                if (!relation.Save(true))
                {
                    confirmRelation.Delete(true);
                    return false;
                }
                return true;
            }
            return false;
        }

        public bool NotConfirmRelationRequest(int relationID)
        {
            var relation = new Relation();
            //get relation to confirm
            if (relation.Retrieve(relationID))
            {
                if (relation.Delete(true))
                {
                    try
                    {
                        using (var db = new DB())
                        {
                            db.ExecuteUpdate("DELETE Notifications WHERE TargetID = @relationID AND TargetType = @targetType",
                                     relationID, TargetTypes.Relation);

                        }
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public List<Events.Events> GetFriendEvents(int accountID, int pageNum, int pageSize)
        {
            if (accountID < 0) return null;
            using (var db = new DB())
            {
                var events = new List<Events.Events>();
                var builder = new StringBuilder();
                builder.Append("SELECT * FROM ( ")
                    .AppendFormat(
                        "SELECT top ({0:D} * {1:D}) ResultNum = ROW_NUMBER() over ( order by E.DayCreate DESC ), A.Email, E.* ",
                        pageNum, pageSize)
                    .Append("FROM Accounts A INNER JOIN Events E ")
                    .Append("ON A.AccID = E.AccID INNER JOIN Relation R")
                    .Append("ON A.AccID = R.AccID  ")
                    .Append("AND A.AccID = R.FriendID   ")
                    .Append("WHERE R.FriendID = @ID) as PAGINATED ")
                    .AppendFormat("WHERE ResultNum > (( {0:D} -1)*{1:D}) ", pageNum, pageSize);
                db.Query(builder.ToString(), accountID);
                while (db.Read())
                {
                    var _event = new Events.Events();
                    db.SetValues(_event);
                    events.Add(_event);
                }
                return events;
            }
        }
        public bool CheckEmailExisted(string email)
        {
            using (var db = new DB())
            {
                db.Query("SELECT * FROM Accounts WHERE Email = @email And [Status] <> @status",email,GeneralStatus.Deleted);
                if(db.Read())
                {
                    var id = db.GetInt32("AccID");
                    return id > 0;
                }
                return false;
            }
        }
    }
}