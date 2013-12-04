using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tweeter.Models;
using System.Web.Security;
using WebMatrix.WebData;
using System.Text.RegularExpressions;

namespace Tweeter.Controllers
{
    public class PostController : Controller
    {
        private EntityContext db = new EntityContext();

        //
        // GET: /Post/

        
        public ActionResult Index()
        {
                HashSet<Post> myPosts = new HashSet<Post>();
                //get all posts of hashtags you are watching
                User currentUser = db.Users.Where(u => u.UserProfile.UserId == WebSecurity.CurrentUserId).FirstOrDefault();
                
                List<Post> hashtagPosts = new List<Post>();
                hashtagPosts = db.Posts.Where(p => p.hashtags.Intersect(currentUser.watching).Any()).ToList();
                myPosts.Concat(hashtagPosts);
                List<Post> followingPosts = new List<Post>();
                followingPosts = db.Posts.Where(p => currentUser.following.Contains(p.creator)).ToList();
                myPosts.Concat(followingPosts);
                List<Post> postsByMe = new List<Post>();
                postsByMe = db.Posts.Where(p => p.creator.Id == currentUser.Id).ToList();
                myPosts.Concat(postsByMe);
                return View("Index", myPosts);
            }
           
            
        
        //
        // POST: /Post/
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]

        public ActionResult Index(PostSearchModel model)
        {

            ICollection<Post> resultSet = new List<Post>();
            if (ModelState.IsValid)
            {
                //if is hashtag, find all posts with such a hashtag
                if (model.query[0] == '#')
                {
                    string hashtagString = model.query.Substring(1);
                    if (hashtagString == "")
                    {
                        return RedirectToAction("Index", "Hashtag");
                    }
                    Hashtag hashtag = db.Hashtags.Where(h => h.name == hashtagString).FirstOrDefault();
                    if (hashtag != null)
                    {
                        resultSet = hashtag.posts;
                    }
                    if (resultSet.Count == 0)
                    {
                        ModelState.AddModelError("emptyResultSet", "No posts match your search");
                    }
                }
                else
                {
                    //if is not hashtag, find all posts by said user
                    User user = db.Users.Where(u => u.UserProfile.UserName == model.query).FirstOrDefault();
                    resultSet = db.Posts.Where(p => p.creator.UserProfile.UserId == user.UserProfile.UserId).ToList();
                    if (resultSet.Count == 0)
                    {
                        ModelState.AddModelError("emptyResultSet", "No posts match your search");
                    }
                }
            }

           
            return View(resultSet);
        }
       
        //
        // POST: /Post/Like/5
        public ActionResult Like(int id)
        {
            //get the current user
            User user = (from u in db.Users where u.UserProfile.UserId == WebSecurity.CurrentUserId select u).FirstOrDefault();
            Post post = db.Posts.Find(id);
            if (!post.likers.Contains(user))
            {
                post.likers.Add(user);
            }
            if (!user.likes.Contains(post))
            {
                user.likes.Add(post);
            }
            db.SaveChanges();
            //return View();
            return Redirect(Request.UrlReferrer.ToString());

        }


        //
        // GET: /Post/Details/5

        public ActionResult Details(int id = 0)
        {
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        //
        // GET: /Post/Create

        public ActionResult Create()
        {
            if (WebSecurity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login","Account");
            }
            
        }

        //
        // POST: /Post/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Post post)
        {
            //get the current user
            User user = (from u in db.Users where u.UserProfile.UserId == WebSecurity.CurrentUserId select u).FirstOrDefault();
            if (ModelState.IsValid)
            {
                //get the parent post if there is one
                var parentId = Request.Form["parent"];
                if (parentId != null)
                {
                    Post parentPost = db.Posts.Find(parentId);
                    post.parent = parentPost;
                    db.Entry(parentPost).State = EntityState.Unchanged;
                }
                post.creator = user;
                //identify the hashtags in the post
                List<string> hashtags = extractHashtags(post.postContent);
                foreach (string tag in hashtags)
                {
                    Hashtag hashtag;
                    //add the hashtag if it doesn't already exist
                    if (db.Hashtags.Where(h => h.name == tag).Count() == 0)
                    {
                         hashtag = new Hashtag { name = tag};
                         db.Hashtags.Add(hashtag);
                    }
                    else
                    {
                        hashtag = db.Hashtags.Where(h => h.name == tag).FirstOrDefault();
                        db.Entry(hashtag).State = EntityState.Unchanged;
                    }
                    //update the hashtag's list of posts
                    hashtag.posts.Add(post);
                    //update the post's list of hashtags
                    post.hashtags.Add(hashtag);
                    
                    
                }
                post.timestamp = DateTime.Now;
                db.Posts.Add(post);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(post);
        }

        //
        // GET: /Post/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        //
        // POST: /Post/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Post post)
        {
            if (ModelState.IsValid)
            {
                db.Entry(post).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(post);
        }

        //
        // GET: /Post/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        //
        // POST: /Post/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Post post = db.Posts.Find(id);
            db.Posts.Remove(post);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private List<string> extractHashtags(string body)
        {
            List<string> results = new List<string>();
            string pattern = "(?<=\\#)\\w+(?=(\\W|$))";
            foreach (Match m in Regex.Matches(body, pattern))
            {
                results.Add(m.Value);
            }
            return results;
        }
    }
}