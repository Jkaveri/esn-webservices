using System;
using System.Collections.Generic;
using System.Text;
using JK.Core;
using JK.Core.Utilities;

namespace Core.Accounts
{
    public class AccountManager : JKManager<Accounts>
    {
        public Accounts Register(string firstName, string lastName, string email, string password, DateTime birthday,
                                 string phone, bool gender)
        {
            var account = new Accounts
                              {
                                  Email = email,
                                  Password = password.ToMD5(),
                                  RoleID = AccountRoles.User,
                                  AccessToken = "",
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
                                      FirstName = firstName,
                                      LastName = lastName,
                                      Birthday = birthday,
                                      Phone = phone,
                                      Gender = gender
                                  };
                if (profile.Save())
                {
                    return account;
                }
                return null;
            }
            return null;
        }

        public List<Accounts> GetListFriends(int accountID, int pageNum, int pageSize)
        {
            var list = new List<Accounts>();
            using (var db = new DB())
            {
                var query = new StringBuilder();
                query.Append("SELECT * FROM ( ")
                    .AppendFormat(
                        "SELECT top ({0:D} * {1:D}) ResultNum = ROW_NUMBER() over ( order by A.AccID ASC ), A.*  ",
                        pageNum, pageSize)
                    .Append("FROM Accounts A JOIN Relation R  ")
                    .Append("ON A.AccID = R.AccID JOIN Accounts F  ")
                    .Append("ON R.FriendID = F.AccID  ")
                    .Append("WHERE R.FriendID = @ID) as PAGINATED ")
                    .AppendFormat("WHERE ResultNum > (( {0:D} -1)*{1:D}) ", pageNum, pageSize);
                db.Query(query.ToString(), accountID);
                if (db.HasResult)
                {
                    while (db.Read())
                    {
                        var friend = new Accounts();
                        db.SetValues(friend);
                        list.Add(friend);
                    }
                }
            }
            return list;
        }

        public List<Profiles> GetListFriendProfiles(int accountID, int pageNum, int pageSize)
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
                    var settings = SiteSettings.GetInstance();
                    //get profile
                    var profile = account.GetProfile();
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
        public bool AddFriend(int accID,int friendID)
        {
            var relation = new Relation
                               {
                                   AccID = accID,
                                   FriendID = friendID,
                                   RelationTypeID = 1,
                                   DayCreate = DateTime.Now,
                                   Status = RelationStatus.Pending
                               };
            return relation.Save(false);
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
                                          RelationTypeID = relation.RelationTypeID,
                                          DayCreate = DateTime.Now,
                                          Status = RelationStatus.Confirmed
                                      };
            //save new relation of this request
            //if save success then update realtion status for request relation
            if(confirmRelation.Save(false))
            {
                relation.Status = RelationStatus.Confirmed;
                if(!relation.Save(true))
                {
                    confirmRelation.Delete(true);
                }
                return true;
            }
            return false;
        }
    }
}