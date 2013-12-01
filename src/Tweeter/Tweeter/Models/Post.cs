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
        public virtual ICollection<User> likers { get; set; }


    }

}