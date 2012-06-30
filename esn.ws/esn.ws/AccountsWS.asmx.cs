using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.EnterpriseServices;
using System.Linq;
using System.Web.Services;
using Core.Accounts;
using JK.Core.Utilities;

namespace esn.ws
{
    /// <summary>
    /// Summary description for Accounts
    /// </summary>
    [WebService(Namespace = "http://esn.com.vn/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class AccountWS : WebService
    {
        private AccountManager manager = new AccountManager();

        /// <summary>
        /// Login with email and password
        /// </summary>
        /// <param name="email">email address</param>
        /// <param name="password">passsword not encrypt</param>
        /// <returns>true for success</returns>
        [WebMethod]
        public bool Login(string email, string password)
        {
            return Accounts.Login(email, password);
        }

        /// <summary>
        /// Retrieve account info by id
        /// </summary>
        /// <param name="id">id of account</param>
        /// <returns>Account object</returns>
        [WebMethod]
        public Accounts Retrieve(int id)
        {
            var account = new Accounts();
            account.Retrieve(id);
            return new Accounts
                       {
                           AccID = account.AccID,
                           RoleID = account.RoleID,
                           Email = account.Email,
                           AccessToken = account.AccessToken,
                           DateCreated = account.DateCreated,
                           IsOnline = account.IsOnline,
                           Profile = account.Profile
                       };
        }

        [WebMethod]
        public object RetrieveJSON(int id)
        {
            var account = new Accounts();
            account.Retrieve(id);
            var profile = account.Profile;
            return new
                       {
                           AccID = account.AccID,
                           RoleID = account.RoleID,
                           Email = account.Email,
                           AccessToken = account.AccessToken,
                           DateCreated = account.DateCreated,
                           IsOnline = account.IsOnline,
                           Username = profile.Username,
                           Name = profile.Name,
                           Address = profile.Address,
                           Street = profile.Street,
                           District = profile.District,
                           City = profile.City,
                           Country = profile.Country,
                           Avatar = profile.Avatar,
                           Phone = profile.Phone,
                           Birthday = profile.Birthday,
                           Gender = profile.Gender,
                           ShareType = profile.ShareType,
                           Favorite = profile.Favorite,
                           Status = account.Status
                       };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="birthday"></param>
        /// <param name="phone"></param>
        /// <param name="gender"></param>
        /// <param name="accessToken"> </param>
        /// <returns></returns>
        [WebMethod]
        public int Register(string name, string email, string password, DateTime birthday,
                            string phone, bool gender, string accessToken)
        {
            return manager.Register(name, email, password, birthday, phone, gender, accessToken);
        }

        [WebMethod]
        public bool AddFriend(int accID, int friendID)
        {
            return manager.AddFriend(accID, friendID);
        }

        [WebMethod]
        public bool ConfirmAddFriendRequest(int relationID)
        {
            return manager.ConfirmRelationRequest(relationID);
        }

        [WebMethod]
        public bool NotConfirmAddFriendRequest(int relationID)
        {
            return manager.NotConfirmRelationRequest(relationID);
        }

        [WebMethod]
        public List<Profiles> GetListFriends(int accountID, int pageNum, int pageSize)
        {
            var result = manager.GetListFriends(accountID, pageNum, pageSize);

            return result;
        }

        [WebMethod]
        public List<object> GetListFriendsJSON(int accountID, int pageNum, int pageSize)
        {
            return manager.GetListFriendsJSON(accountID, pageNum, pageSize);
        }

        [WebMethod]
        public bool ChangePassword(string email, string oldPassword, string newPassword)
        {
            return manager.ChangePassword(email, oldPassword, newPassword);
        }

        [WebMethod]
        public bool CheckEmailExisted(string email)
        {
            return manager.CheckEmailExisted(email);
        }

        [WebMethod]
        public Accounts RetrieveByEmail(string email)
        {
            Accounts account = new Accounts();
            account.Retrieve(email);
            return account;
        }

        [WebMethod]
        public bool UpdateProfile(int accID, string name, bool gender, string birthday, string phone, string avatar,
                                  string address, string street,
                                  string district, string city, string country, string favorite)
        {
            var profile = new Profiles()
                              {
                                  AccID = accID,
                                  Name = name,
                                  Gender = gender,
                                  Birthday = DateTime.Parse(birthday),
                                  Phone = phone,
                                  Avatar = avatar,
                                  Address = address,
                                  Street = street,
                                  District = district,
                                  City = city,
                                  Country = country,
                                  Favorite = favorite
                              };
            return profile.Save(true);
        }

    }
}