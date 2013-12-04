using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Tweeter.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string verification { get; set; }
        public virtual UserProfile UserProfile { get; set; }

        public virtual ICollection<Post> likes { get; set; }
        public virtual ICollection<User> followers { get; set; }
        public virtual ICollection<User> following { get; set; }
        public virtual ICollection<Hashtag> watching { get; set; }
        public User()
        {
            watching = new HashSet<Hashtag>();
        }
    }
    public class UserSearchModel
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string userName { get; set; }
    }
   
}