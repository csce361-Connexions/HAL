using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tweeter.Models;

namespace Tweeter.Controllers
{
    public class HomeController : Controller
    {
        private PostContext db = new PostContext();

        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to HAL Tweeter!";
            //ViewBag.PostList = db.Posts.ToList();
            return View(db.Posts.ToList());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
