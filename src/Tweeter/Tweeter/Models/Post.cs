﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using Tweeter.Models;

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

        public virtual UserProfile user { get; set; }

        public virtual ICollection<UserProfile> likers { get; set; }

        public int numLikes { get; set; }

        public virtual ICollection<UserProfile> followers { get; set; }


    }
}