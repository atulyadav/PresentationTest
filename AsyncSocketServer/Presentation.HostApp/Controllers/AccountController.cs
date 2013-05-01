using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using PresentationApp.DataAccess;
using PresentationApp.DataAccess.Models.Account;
using PresentationApp.Domain;
using PresentationApp.Domain.Account;

namespace Presentation.HostApp.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Index()
        {
            return View("Login");
        }

        public ActionResult Login(string id)
        {
            UserData.createSession();
            if (id != null)
            {
                Guid guid = new Guid(id);
                LoginRequest loginRequest = new LoginRequest();
                loginRequest.PresentationKey = guid;
                return View(loginRequest);
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult Login(LoginRequest loginRequest)
        {
            UserData.createSession();
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);
            try
            {
                if (loginRequest != null)
                {
                    UserData userData = new UserData();

                    if (loginRequest.PresentationKey == null || loginRequest.PresentationKey == Guid.Empty)
                    {
                        Users admin = userData.GetAdmin(loginRequest.UserName, loginRequest.Password);

                        if (admin != null)
                        {
                            FormsAuthentication.SetAuthCookie(admin.Id.ToString(), false);
                            return RedirectToAction("ManageUser");
                        }
                        else
                        {
                            ViewBag.Message = "Please check username/password";
                        }
                    }
                    else
                    {
                        Users user = userData.GetUsers(loginRequest.UserName, loginRequest.Password);
                        if (user != null)
                        {
                            LoginReply loginReply = userData.CheckPresenterOrUser(loginRequest.PresentationKey, user.Id);

                            if (loginReply.Status == 6)
                            {
                                ViewBag.Message = "Invalid Presentation Key. This presentation may be no longer alloted to you. Please check with the admin.";
                            }
                            else if (loginReply.Status == 5)
                            {
                                ViewBag.Message = "This presentation might have been deleted. Please ask your admin to allot you a different URL.";
                            }
                            else if (loginReply.Status == 1 || loginReply.Status == 2 || loginReply.Status == 3 || loginReply.Status == 4)
                            {
                                FormsAuthentication.SetAuthCookie(user.Id.ToString(), false);
                                //return RedirectToAction("presentation", "presentations", new { stat = status, guid = loginRequest.PresentationKey });
                                return View("PresentationDescription", loginReply);
                            }
                        }
                        else
                        {
                            ViewBag.Message = "Please check username/password";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Message = "An Exception occured. Please try again later.";
                LoggerHelper.LogException(MethodBase.GetCurrentMethod().Name, e, JsonConvert.SerializeObject(loginRequest));
            }

            return View();
        }

        [Authorize]
        public ActionResult PresentationDescription(LoginReply loginReply)
        {
            UserData.createSession();
            if (loginReply != null)
            {
                return View(loginReply);
            }
            return RedirectToAction("Login");
        }

        [Authorize]
        [HttpPost]
        public ActionResult PresentationDescription(int Status, long UserId, long PresentationId, string PresentationKey)
        {
            UserData.createSession();
            if (PresentationKey != null)
            {
                Guid presentationKey = new Guid(PresentationKey);
                UserData usrData = new UserData();
                PresentationApp.Domain.Presentation.Presentation presentation = usrData.GetPresentation(PresentationId);
                if (Status == 1 || Status == 3)
                {
                    UserPresentation userPresentation = usrData.GetUsersPresentation(PresentationId, UserId);
                    if (userPresentation.IsUserConnected == true)
                    {
                        ViewBag.Message = "You are already connected as user.";
                    }
                    else
                    {
                        if (presentation.IsHostConnected == true)
                        {
                            userPresentation.IsUserConnected = true;
                            if (usrData.UpdateUserPresentation(userPresentation))
                            {
                                return RedirectToAction("presentation", "presentations", new { stat = Status, guid = PresentationKey });
                            }
                        }
                        else
                        {
                            ViewBag.Message = "Host is not connected yet. Please try in some time.";
                        }
                    }
                }
                else
                {
                    if (presentation.IsHostConnected == true)
                    {
                        ViewBag.Message = "You are already connected as host.";
                    }
                    else
                    {
                        presentation.IsHostConnected = true;
                        if (usrData.UpdatePresentation(presentation))
                        {
                            return RedirectToAction("presentation", "presentations", new { stat = Status, guid = PresentationKey });
                        }
                    }
                }
            }
            return View();
        }

        public ActionResult Logout()
        {
            UserData.createSession();
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);
            try
            {
                FormsAuthentication.SignOut();
            }
            catch (Exception e)
            {
                ViewBag.Message = "An Exception occured. Please try again later.";
                LoggerHelper.LogException(MethodBase.GetCurrentMethod().Name, e);
            }

            return View("Login");
        }

        [Authorize]
        public ActionResult ManageUser()
        {
            UserData.createSession();
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            UserData userData = new UserData();
            try
            {
                IList<Users> userList = userData.GetUsersList();

                return View("ManageUser", userList);
            }
            catch (Exception e)
            {
                TempData["Message"] = "An Exception occured. Please try again later.";
                LoggerHelper.LogException(MethodBase.GetCurrentMethod().Name, e);
                return View();
            }
        }

        [Authorize]
        public ActionResult CreateUser(string id, int flag)
        {
            UserData.createSession();
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);
            CreateOrEditUserRequest userm = new CreateOrEditUserRequest();

            userm.flag = flag;
            if (flag == 2)
            {
                UserData userData = new UserData();
                Users user = userData.GetUsers(Convert.ToInt32(id));
                userm.Id = user.Id;
                userm.Name = user.Name;
                userm.Password = user.Password;
                userm.PhoneNumber = user.PhoneNumber;
                userm.EmailId = user.EmailId;
                userm.IsAdmin = user.IsAdmin;
                userm.UserName = user.UserName;
            }

            return PartialView("CreateUser", userm);
        }

        [Authorize]
        [HttpPost]
        public ActionResult CreateUser(CreateOrEditUserRequest userm)
        {
            UserData.createSession();
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            try
            {
                UserData userData = new UserData();
                Users user = new Users();

                if (userm.flag == 2)
                {
                    user = userData.GetUsers(userm.Id);
                }

                user.Id = userm.Id;
                user.Name = userm.Name;
                user.Password = userm.Password;
                user.PhoneNumber = userm.PhoneNumber;
                user.EmailId = userm.EmailId;
                user.IsAdmin = userm.IsAdmin;
                user.UserName = userm.UserName;
                user.UpdatedDate = DateTime.Now;

                if (userm.flag == 1)
                {
                    user.IsActive = true;
                    user.CreatedDate = DateTime.Now;
                    user.UpdatedDate = null;
                }

                if (userData.UpdateUser(user))
                {
                    TempData["Message"] = "User data was saved successfully.";
                }
                else
                {
                    TempData["Message"] = "Could not save the user data. Please try later.";
                }

                return RedirectToAction("ManageUser");
            }
            catch (Exception e)
            {
                TempData["Message"] = "An Exception occured. Could not save the user data. Please try later.";
                LoggerHelper.LogException(MethodBase.GetCurrentMethod().Name, e, JsonConvert.SerializeObject(userm));
                return RedirectToAction("ManageUser");
            }
        }

        [Authorize]
        public ActionResult DeleteUser(int id)
        {
            UserData.createSession();
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);
            try
            {
                UserData userData = new UserData();

                Users user = userData.GetUsers(id);
                return PartialView("DeleteUser", user);
            }
            catch (Exception e)
            {
                TempData["Message"] = "An Exception occured. Please try again later.";
                LoggerHelper.LogException(MethodBase.GetCurrentMethod().Name, e, String.Format("Id: {0}", id));
                return RedirectToAction("ManageUser");
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteUser(Users user)
        {
            UserData.createSession();
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);
            try
            {
                UserData userData = new UserData();

                Users temp = userData.GetUsers((int)user.Id);
                temp.UpdatedDate = DateTime.Now;
                temp.IsActive = false;

                if (userData.UpdateUser(temp))
                {
                    TempData["Message"] = "User was deleted successfully.";
                }
                else
                {
                    TempData["Message"] = "Could not delete user. Please try later.";
                }

                return RedirectToAction("ManageUser");
            }
            catch (Exception e)
            {
                TempData["Message"] = "An Exception occured. Could not delete user. Please try again later.";
                LoggerHelper.LogException(MethodBase.GetCurrentMethod().Name, e, JsonConvert.SerializeObject(user));
                return RedirectToAction("ManageUser");
            }
        }

        public JsonResult CheckUserIsOccupied(long id)
        {
            UserData.createSession();
            UserData usrData = new UserData();
            if (usrData.checkOccupiedAsPresenter(id))
            {
                return Json(new { redirectTo = Url.Action("ManageUser"), Message = "The user is a host to some presentation. Cannot delete this user." });
            }
            else
            {
                return Json(new { redirectTo = Url.Action("DeleteUser", usrData.GetUsers(id)) });
            }
        }

        public JsonResult CheckUsernameExists(string UserName, int flag)
        {
            UserData.createSession();
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            result.Data = true;

            if (flag == 1)
            {
                UserData userData = new UserData();

                if (userData.GetUsers(UserName) != null)
                {
                    result.Data = false;
                }
            }

            return result;
        }
    }
}