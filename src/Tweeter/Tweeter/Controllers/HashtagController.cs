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
    public class HashtagController : Controller
    {
        private EntityContext db = new EntityContext();

        //
        // GET: /Hashtag/

        public ActionResult Index(string type="full")
        {
            if (type == "partial")
            {
                return PartialView(db.Hashtags.OrderByDescending(h => h.creationDate).ToList());
            }
            else
            {
                return View(db.Hashtags.OrderByDescending(h => h.creationDate).ToList());
            }
        }

        //
        // GET: /Hashtag/Details/5

        public ActionResult Details(int id = 0)
        {
            if (WebSecurity.IsAuthenticated)
            {
                Hashtag hashtag = db.Hashtags.Find(id);
                if (hashtag == null)
                {
                    return HttpNotFound();
                }
                User currentUser = (from u in db.Users where u.UserProfile.UserId == WebSecurity.CurrentUserId select u).FirstOrDefault();
                if (currentUser.watching.Contains(hashtag))
                {
                    ViewBag.watchText = "Unwatch";
                }
                else
                {
                    ViewBag.watchText = "Watch";
                }
                return View(hashtag);
            }
            else
            {
                return (RedirectToAction("Login", "Account"));
            }
        }
        public ActionResult Watch(int id)
        {
            if (WebSecurity.IsAuthenticated)
            {
                //get the current user
                User currentUser = (from u in db.Users where u.UserProfile.UserId == WebSecurity.CurrentUserId select u).FirstOrDefault();
                if (db.Users.Where(u => u.Id == id).Count() != 0)
                {
                    Hashtag tag = db.Hashtags.Find(id);

                    //are we watching or unwatching?
                    if (currentUser.watching.Contains(tag))
                    {
                        //ununwatch
                        currentUser.watching.Remove(tag);
                        tag.watchers.Remove(currentUser);
                    }
                    else
                    {
                        //watch
                        //associate the current user and the watched tag with each other
                        currentUser.watching.Add(tag);
                        tag.watchers.Add(currentUser);
                    }
                    db.SaveChanges();


                }
                return Redirect(Request.UrlReferrer.ToString()); ;
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