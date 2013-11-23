using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;

namespace Tweeter.Models
{
    public class PostContext : DbContext
    {
        public PostContext():base("DefaultConnection")
        {

        }
        public DbSet<Post> Posts { get; set; }
    }
    [Table("Posts")]
    public class Post
    {
        public int Id {get; set;}
        [StringLength(200)]
        public string postContent{get; set;}

        [Required]
        public virtual UserProfile user { get; set; }




    }
}