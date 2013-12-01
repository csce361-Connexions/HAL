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
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Post>().
                HasMany(p => p.likers).WithMany(u => u.likes).Map(
                m =>
                {
                    m.MapLeftKey("likes_PostId");
                    m.MapRightKey("likes_UserId");
                    m.ToTable("Likes");
                });
            
        }
    }
}