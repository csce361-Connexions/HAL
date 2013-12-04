using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tweeter.Models;

namespace Tweeter.Controllers
{
    public class HashtagController : Controller
    {
        private EntityContext db = new EntityContext();

        //
        // GET: /Hashtag/

        public ActionResult Index()
        {
            return View(db.Hashtags.OrderByDescending(h => h.creationDate).ToList());
        }

        //
        // GET: /Hashtag/Details/5

        public ActionResult Details(int id = 0)
        {
            Hashtag hashtag = db.Hashtags.Find(id);
            if (hashtag == null)
            {
                return HttpNotFound();
            }
            return View(hashtag);
        }

       
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}