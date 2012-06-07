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
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
        // [System.Web.Script.Services.ScriptService]
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
        public bool login(string email, string password)
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
            return account;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="birthday"></param>
        /// <param name="phone"></param>
        /// <param name="gender"></param>
        /// <returns></returns>
        [WebMethod]
        public Accounts Register(string firstName, string lastName, string email, string password, DateTime birthday,
                                 string phone, bool gender)
        {
            return manager.Register(firstName,lastName,email,password,birthday,phone,gender);
        }
        [WebMethod]
        public Profiles GetProfiles(int accountID)
        {
            var account = new Accounts();
            account.AccID = accountID;
            return account.GetProfile();
        }

        [WebMethod]
        public List<Accounts> GetListEntities(int page, int pagesize)
        {
            return manager.GetListEntities(page, pagesize).ToList();
        }
        [WebMethod]
        public List<Accounts> GetListFriends(int accountID, int pageNum, int pageSize)
        {
            return manager.GetListFriends(accountID, pageNum, pageSize);
        } 
        [WebMethod]
        public List<Profiles> GetListFriendProfiles (int accountID, int pageNum, int pageSize)
        {
            return manager.GetListFriendProfiles(accountID, pageNum, pageSize);
        }
        [WebMethod]
        public bool ChangePassword(string email, string oldPassword, string newPassword)
        {
            return manager.ChangePassword(email, oldPassword, newPassword);
        } 
    }
}