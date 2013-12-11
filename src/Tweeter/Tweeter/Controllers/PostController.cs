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
                if (WebSecurity.IsAuthenticated)
                {
                    User currentUser = db.Users.Where(u => u.UserProfile.UserId == WebSecurity.CurrentUserId).FirstOrDefault();
                    //Get a list of all hashtags ids you're watching
                    List<int> hids = currentUser.watching.Select(h => h.Id).ToList();
                    ViewData["myHashtags"] = hids.ToArray<int>();
                    //Get all hashtags with one of those ids
                    List<Hashtag> mytags = db.Hashtags.Where(h => hids.Contains(h.Id)).ToList();
                    //Get all posts with one of those hashtags
                    foreach (Hashtag tag in mytags)
                    {
                        List<Post> myHashtagPosts = db.Posts.Where(p => p.hashtags.Select(h => h.Id).Contains(tag.Id)).ToList();
                        myPosts.UnionWith(myHashtagPosts);
                    }
                    List<Post> followingPosts = new List<Post>();
                    //Get all user ids that you're following
                    List<int> userIds = currentUser.following.Select(u => u.Id).ToList();
                    //get all posts by one of those users
                    foreach (int userid in userIds)
                    {
                        List<Post> thisUsersPosts = db.Posts.Where(p => p.creator.Id == userid).ToList();
                        myPosts.UnionWith(thisUsersPosts);
                    }
                    List<Post> postsByMe = new List<Post>();
                    postsByMe = db.Posts.Where(p => p.creator.Id == currentUser.Id).ToList();
                    myPosts.UnionWith(postsByMe);
                }
                return PartialView("Index", myPosts.OrderByDescending(p => p.timestamp));
            }
           
            
        
        

        public ActionResult Search(string query)
        {

            ICollection<Post> resultSet = new List<Post>();
            
                //if is hashtag, find all posts with such a hashtag
                if (query[0] == '#')
                {
                    string hashtagString = query.Substring(1);
                    if (hashtagString == "")
                    {
                        //display a list of all hashtags
                        return PartialView("_HashtagList");
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
                    User user = db.Users.Where(u => u.UserProfile.UserName == query).FirstOrDefault();
                    if (user != null)
                    {
                        resultSet = db.Posts.Where(p => p.creator.UserProfile.UserId == user.UserProfile.UserId).ToList();
                    }
                }
                ViewBag.viewIrreplaceable = true;
            return PartialView("Index", resultSet.OrderByDescending(p=>p.timestamp));
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
                return RedirectToAction("Index","Home");
            }

            return RedirectToAction("Index","Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Comment(PostCommentModel model)
        {
            //get the current user
            User user = (from u in db.Users where u.UserProfile.UserId == WebSecurity.CurrentUserId select u).FirstOrDefault();
            if (ModelState.IsValid)
            {
                //Create the new post
                Post post = new Post();
                post.creator = user;
                post.parent = db.Posts.Find(model.parentId);
                post.postContent = model.postContent;
                //identify the hashtags in the post
                List<string> hashtags = extractHashtags(post.postContent);
                foreach (string tag in hashtags)
                {
                    Hashtag hashtag;
                    //add the hashtag if it doesn't already exist
                    if (db.Hashtags.Where(h => h.name == tag).Count() == 0)
                    {
                        hashtag = new Hashtag { name = tag };
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
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
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