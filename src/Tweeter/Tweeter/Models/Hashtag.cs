using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Tweeter.Models
{
    
    public class Hashtag
    {
        public int Id { get; set; }

        public string name { get; set; }
        public virtual ICollection<Post> posts { get; set; }
        public DateTime creationDate { get; set; }
        public virtual ICollection<User> watchers { get; set; }
        public Hashtag()
        {
            posts = new HashSet<Post>();
            creationDate = DateTime.Now;
        }
    }

}