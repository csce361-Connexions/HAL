using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using Tweeter.Filters;
using Tweeter.Models;
using System.Net.Mail;
using System.Net;
using System.IO;

namespace Tweeter.Controllers
{
     
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        EntityContext db = new EntityContext();
        UserProfilesContext profilesDb = new UserProfilesContext();
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            //first check if the user has verified via email
            User user = db.Users.Where(u => u.UserProfile.UserName == model.UserName).FirstOrDefault();
            if (user.verification == null)
            {


                if (ModelState.IsValid && 
                    !WebSecurity.IsAccountLockedOut(model.UserName, 2, 60) &&
                    WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
                {
                    return RedirectToLocal(returnUrl);
                }
            }
            else
            {
                //The user hasnt yet verfied via email
                ModelState.AddModelError("", "Your account has not yet been verified!");
                return View(model);
            }
            // If we got this far, something failed, redisplay form
            if (WebSecurity.IsAccountLockedOut(model.UserName, 2, 60))
            {
                ModelState.AddModelError("", "Your account has been locked after too many failed login attempts. Try again in 1 minute.");
            }
            else
            {
                ModelState.AddModelError("", "The user name or password provided is incorrect.");
            }
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    HttpPostedFileBase image = Request.Files["profilePic"];
                    if (image.ContentLength > 1000000)
                    {
                        throw new FileSizeException();
                    }
                    if (image.ContentLength > 0)
                    {
                        //Save the profile picture
                        string fileName = model.UserName + ".jpg";
                        string path = Path.Combine(Server.MapPath("~/Images/Account"), fileName);
                        image.SaveAs(path);
                    }
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
                    //Set the first name, last name, and email of the newly created user profile
                    UserProfile newUserProfile = profilesDb.UserProfiles.Where(u => u.UserName == model.UserName).FirstOrDefault();
                    User newUser = new User();
                    string guid = Guid.NewGuid().ToString();
                    newUser.UserProfile = newUserProfile;
                    newUser.EmailAddress = model.emailAddress;
                    newUser.FirstName = model.firstName;
                    newUser.LastName = model.lastName;
                    newUser.verification = guid;
                    newUser.bio = model.bio;
                    
                    sendVerificationEmail(newUser);
                    db.Entry(newUserProfile).State = System.Data.EntityState.Unchanged;
                    db.Users.Add(newUser);
                    db.SaveChanges();
                    TempData["message"] = "Check your email to finish creating your account.";
                    return RedirectToAction("Index", "Home");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
                catch (FileSizeException)
                {
                    ModelState.AddModelError("fileSize", "Your profile picture must be under 1MB in size");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        //GET: /Account/CheckUserName
        [AllowAnonymous]
        public JsonResult CheckUserName(string username, string firstname="", string lastname="")
        {
            //Check if the username exists in the database
            
            UserProfile existing = profilesDb.UserProfiles.Where(u => u.UserName == username).FirstOrDefault();
            //if it does not, return nothing
            List<string> suggestions = new List<string>();
            if (existing != null)
            {         
                //Check first_last
                if(firstname!="" && lastname!=""){
                string firstLast = firstname +"_"+ lastname;
                bool taken = (from u in profilesDb.UserProfiles where u.UserName == firstLast select u).Count() > 0;
                if (!taken)
                {
                    suggestions.Add(firstLast);
                }
                }
                int i = 1;
                while (suggestions.Count < 3)
                {
                    //Count up with the username until there are three suggestions
                    string altName = existing.UserName + i;
                    bool altTaken = (from u in profilesDb.UserProfiles where u.UserName == altName select u).Count() > 0;
                    if (!altTaken)
                    {
                        suggestions.Add(altName);
                    }
                    i++;
                }
                
            }


            return Json(new { suggestions = suggestions }, JsonRequestBehavior.AllowGet); 
        }

        //
        // GET: /Account/Verify
        [AllowAnonymous]
        public ActionResult Verify(string guid)
        {
            //get the user with the given guid
            User user = db.Users.Where(u => u.verification == guid).FirstOrDefault();
            user.verification = null;
            db.Entry(user).State = System.Data.EntityState.Modified;
            db.SaveChanges();
            TempData["message"] = "Your account has successfully been verified!";
            return RedirectToAction("Login");
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            if (WebSecurity.IsAuthenticated)
            {
                TempData["bioText"] = db.Users.Where(u => u.UserProfile.UserId == WebSecurity.CurrentUserId).FirstOrDefault().bio;
            }
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", String.Format("Unable to create local account. An account with the name \"{0}\" may already exist.", User.Identity.Name));
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (profilesDb)
                {
                    UserProfile user = profilesDb.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        profilesDb.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "User name already exists. Please enter a different user name.");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region Helpers
        private void sendVerificationEmail(User recipient)
        {
            
            string url = string.Format("http://{0}/Account/Verify?guid={1}", Request.Url.Authority, recipient.verification);
            string body = string.Format("Welcome to HAL-lo! To verify your new account, follow <a href=\"{0}\">this link</a>.", url);
            //Send email
            MailAddress from = new MailAddress("tmcclenahan0@gmail.com", "HAL-lo");
            MailAddress to = new MailAddress(recipient.EmailAddress);
            MailMessage mailMessage = new MailMessage(from, to);
            mailMessage.Body = body;

            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = "Verify your new HAL-lo account";

            SmtpClient client = new SmtpClient();
            //client.UseDefaultCredentials = false;
            client.Send(mailMessage);
        }
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }
        internal class FileSizeException : Exception
        {

        }
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
