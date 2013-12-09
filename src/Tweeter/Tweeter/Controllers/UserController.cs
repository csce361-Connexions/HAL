using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
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
        // POST: /User/
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Index(UserSearchModel model)
        {
            if (model.firstName != null && model.lastName != null)
            {
                return View(db.Users.Where(u => u.LastName.Equals(model.lastName, StringComparison.CurrentCultureIgnoreCase) && u.FirstName.Equals(model.firstName, StringComparison.CurrentCultureIgnoreCase)));
            }
            else if (model.userName != null)
            {
                return View(db.Users.Where(u => u.UserProfile.UserName.Equals(model.userName, StringComparison.CurrentCultureIgnoreCase)));
            }
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
        //
        // GET: /User/Image/Thal
        public ActionResult Image(string name, string classname)
        {
            string path = Server.MapPath("~/Images/Account");
            string file = Directory.GetFiles(path, name+".*", SearchOption.TopDirectoryOnly)
            .Where(s => s.EndsWith(".jpg") || s.EndsWith(".jpeg") || s.EndsWith(".png") || s.EndsWith(".bmp")).FirstOrDefault();
            string filePath = Url.Content(string.Format("~/Images/Account/Default/default.jpg"));
            if (file != null)
            {
                filePath = Url.Content(string.Format("~/Images/Account/{0}.jpg", name));
               
            }
            return Content(string.Format(@"<img src=""{0}"" class=""{1}""/>",filePath,classname));
        }
         //
        // POST: /User/Edit/1
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Update(UserUpdateModel model)
        {
            try
            {
                //Update profile pic if one is given
                HttpPostedFileBase image = Request.Files["profilePic"];
                if (image.ContentLength > 1000000)
                {
                    throw new FileSizeException();
                }
                if (image.ContentLength > 0)
                {
                    //Save the profile picture
                    string fileName = WebSecurity.CurrentUserName + ".jpg";
                    string path = Path.Combine(Server.MapPath("~/Images/Account"), fileName);
                    image.SaveAs(path);
                }
                User updateUser = db.Users.Where(u=>u.UserProfile.UserId == WebSecurity.CurrentUserId).FirstOrDefault();
                updateUser.bio = model.bio;
                db.SaveChanges();

            }
            catch (FileSizeException)
            {
                ModelState.AddModelError("fileSize", "Your profile picture must be under 1MB in size");
                return View(model);
            }
            TempData["message"] = "Your account has been updated!";
            return RedirectToAction("Index", "Home");
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        internal class FileSizeException:Exception {

        }
    }
}