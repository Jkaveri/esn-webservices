using System;
using JK.Core;

namespace Core.Accounts
{
    public class Profiles : JKBean
    {
        public Profiles()
        {
            SetModule(this);
            PrimaryKeyName = "AccID";
        }

        public int AccID { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Street { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Avatar { get; set; }
        public string Phone { get; set; }
        public DateTime Birthday { get; set; }
        public bool Gender { get; set; }
        public int SharedID { get; set; }
        public string Favorite { get; set; }
    }
}