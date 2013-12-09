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

        [Required]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        public string postContent{get; set;}

        public virtual User creator { get; set; }
        
        public DateTime timestamp { get; set; }

        public virtual ICollection<Hashtag> hashtags { get; set; }
        public virtual ICollection<User> likers { get; set; }

        public virtual Post parent { get; set; }

        public Post()
        {
            hashtags = new HashSet<Hashtag>();
        }

    }
    public class PostSearchModel
    {
       [Required]
        public string query { get; set; }
    }
    public class PostCommentModel
    {
        public string postContent { get; set; }
        [Required]
        public int parentId { get; set; }
    }

}