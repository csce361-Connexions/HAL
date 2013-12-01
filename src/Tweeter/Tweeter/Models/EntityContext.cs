using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Tweeter.Models
{
    public class EntityContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Hashtag> Hashtags { get; set; }
        public EntityContext()
            : base("DefaultConnection")
        {

        }
    }
}