using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tweeter.Models;
using WebMatrix.WebData;

namespace Tweeter.Controllers
{
    public class UserController : Controller
    {
        private EntityContext db = new EntityContext();

        //
        // GET: /User/

        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        //
        // GET: /User/Details/5

        public ActionResult Details(int id = 0)
        {
            ViewBag.isSelf = false;
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            //is the currently logged in user already following this user?
            User currentUser = (from u in db.Users where u.UserProfile.UserId == WebSecurity.CurrentUserId select u).FirstOrDefault();
            if (currentUser.following.Contains(user))
            {
                ViewBag.followText = "Unfollow";
            }
            else
            {
                if (currentUser.Equals(user))
                {
                    ViewBag.isSelf = true;
                }
                else 
                { 
                    ViewBag.followText = "Follow";
                }
            }
            return View(user);
        }

        //
        // GET: /User/Follow/5
        public ActionResult Follow(int id)
        {
            if (WebSecurity.IsAuthenticated)
            {
                //get the current user
                User currentUser = (from u in db.Users where u.UserProfile.UserId == WebSecurity.CurrentUserId select u).FirstOrDefault();
                if (db.Users.Where(u => u.Id == id).Count() != 0)
                {
                    User followUser = (from u in db.Users where u.Id == id select u).FirstOrDefault();
                    if (currentUser.Id != followUser.Id)
                    {
                        //are we following or unfollowing?
                        if (currentUser.following.Contains(followUser))
                        {
                            //unfollow
                            currentUser.following.Remove(followUser);
                            followUser.followers.Remove(currentUser);
                        }
                        else
                        {
                            //follow
                            //associate the current user and the followed user with each other
                            currentUser.following.Add(followUser);
                            followUser.followers.Add(currentUser);
                        }
                        db.SaveChanges();

                    }
                }
                return  Redirect(Request.UrlReferrer.ToString());;
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
           
        }

        

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}