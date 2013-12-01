using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using Tweeter.Models;

namespace Tweeter.Models
{
   
    [Table("Posts")]
    public class Post
    {
        public int Id {get; set;}
        [StringLength(200)]
        public string postContent{get; set;}

        public virtual User creator { get; set; }

        public virtual ICollection<Hashtag> hashtags { get; set; }
        //public virtual ICollection<UserProfile> likers { get; set; }


        //public Post()
        //{
        //    likers = new HashSet<UserProfile>();
        //}

    }

    //public class LikesContext : DbContext
    //{
    //    public DbSet<UserProfile> users { get; set; }
    //    public DbSet<Post> posts { get; set; }
    //    public LikesContext()
    //        : base("DefaultConnection")
    //    {

    //    }
    //    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    //    {
    //        modelBuilder.Entity<Post>().HasMany(p => p.likers).WithMany(u => u.likes).Map(m =>
    //        {
    //            m.MapLeftKey("likes_PostId");
    //            m.MapRightKey("likes_UserId");
    //            m.ToTable("Likes");
    //        });
    //    }
    //}
}