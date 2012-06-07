using System;
using JK.Core;
using JK.Core.Utilities;

namespace Core.Accounts
{
    public class Accounts : JKBean
    {

        public Accounts()
        {
            SetModule(this);
            PrimaryKeyName = "AccID";
        }

        public int AccID
        {
            get { return ID; }
            set { ID = value; }
        }

        public AccountRoles RoleID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string AccessToken { get; set; }
        public string VerificationCode { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsOnline { get; set; }
        public AccountStatus Status { get; set; }

        public Accounts Login()
        {
            if (!Email.EmailIsValid()) return null;
            if (!Password.IsNullOrEmpty()) return null;
            Password = Password.ToMD5();
            using (var db = new DB())
            {
                db.Query("SELECT * FROM Accounts Where Email = @email and [Deleted]=0", Email);
                while (db.Read())
                {
                    string p = db.GetString("Password");
                    if (p.Equals(Password))
                    {
                        var account = new Accounts();
                        db.SetValues(account);
                        return account;
                    }
                }
            }
            return null;
            
        }
       
        public static bool Login(string email, string password)
        {
            if (!email.EmailIsValid()) return false;
            if (!password.IsNullOrEmpty()) return false;
            //encode password
            password = password.ToMD5();
            using (var db = new DB())
            {
                db.Query("SELECT [Password] FROM Accounts Where Email = @email and [Deleted]=0", email);
                while (db.Read())
                {
                    string p = db.GetString(0);
                    if (p.Equals(password))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public Profiles GetProfile()
        {
            if (AccID < 0) throw new Exception("AccID isn't setted");
            var info = new Profiles();
            info.Retrieve(AccID);
            return info;
        }

    }
}