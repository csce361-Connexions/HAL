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
        private PostContext db = new PostContext();

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
            int currUid = WebSecurity.CurrentUserId;
            //get the current user
            UserProfilesContext userDb = new UserProfilesContext();
            UserProfile user = new UserProfile();
            user = (from u in userDb.UserProfiles where u.UserId == WebSecurity.CurrentUserId select u).FirstOrDefault();
            Post post = db.Posts.Find(id);
            
            //if the likers list is null, initialize it
            if (post.likerIDs == null)
            {
                post.likerIDs = new List<int>();
            }

            //if the user has not already liked the post
            if (ModelState.IsValid && !post.likerIDs.Contains(currUid))
            {
                post.numLikes++;
                post.likerIDs.Add(currUid);
                db.Entry(post).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Home", null);
            }
            return View();
        }

        //
        // POST: /Post/Follow/5
        public ActionResult Follow(Post post)
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
            int asfd = WebSecurity.CurrentUserId;
            //get the current user
            UserProfilesContext userDb = new UserProfilesContext();
            UserProfile user = (from u in userDb.UserProfiles where u.UserId == WebSecurity.CurrentUserId select u).FirstOrDefault();
            if (ModelState.IsValid)
            {
                post.user = user;
                post.likerIDs = new List<int>();
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