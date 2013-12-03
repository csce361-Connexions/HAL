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
    }
   
}