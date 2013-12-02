﻿using System;
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