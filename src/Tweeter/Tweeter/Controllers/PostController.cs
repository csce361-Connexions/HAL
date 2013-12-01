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

namespace Tweeter.Controllers
{
    public class PostController : Controller
    {
        private EntityContext db = new EntityContext();

        //
        // GET: /Post/

        public ActionResult Index()
        {
            return View(db.Posts.ToList());
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
        // POST: /Post/Follow/5
        public ActionResult Follow(int id)
        {
            int currUid = WebSecurity.CurrentUserId;
            //get the current user
            UserProfilesContext userDb = new UserProfilesContext();
            UserProfile user = new UserProfile();
            user = (from u in userDb.UserProfiles where u.UserId == WebSecurity.CurrentUserId select u).FirstOrDefault();
            Post post = db.Posts.Find(id);

            //if the likers list is null, initialize it
            //if (post.likers == null)
            //{
            //    post.likers = new ICollection<UserProfile>();
            //}

            //if the user has not already liked the post
            List<String> userNames = new List<String>();
            //foreach (UserProfile use in post.followers)
            //{
            //    if (!userNames.Contains(use.UserName))
            //    {
            //        userNames.Add(use.UserName);
            //    }
            //}

            //if (ModelState.IsValid && !post.likers.Contains(user)) //This does not work because the users stored in the database do not have a unique ID
            //if (ModelState.IsValid && !userNames.Contains(user.UserName))
            //{
            //    post.followers.Add(user);
            //    db.Entry(post).State = EntityState.Modified;
            //    db.SaveChanges();
            //    //return RedirectToAction("Index", "Home", null);
            //    return Redirect(Request.UrlReferrer.ToString());
            //}
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
                post.creator = user;
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
    }
}