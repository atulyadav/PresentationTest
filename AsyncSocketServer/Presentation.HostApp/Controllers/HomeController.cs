using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using PresentationApp.DataAccess;

namespace Presentation.HostApp.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return RedirectToAction("Login", "Account");
        }

        public ActionResult Link()
        {
            return View();
        }

        public ActionResult ViewPage1()
        {
            return View();
        }

        public ActionResult ViewPage2()
        {
            return View();
        }

        public ActionResult ViewPage3()
        {
            return View();
        }

        public ActionResult PopUp()
        {
            return View();
        }

        public ActionResult Presentation()
        {
            return View();
        }

        public ActionResult ConnectHost()
        {
            UserData userDate = new UserData();
            ViewBag.ClientUrls = userDate.GetClientUrls();
            ViewBag.HostUrls = userDate.GetHostUrls();
            ViewBag.ClientsName = userDate.GetClientsName();
            return View(ViewBag);
        }

        public ActionResult Connected(string guid)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            try
            {
                if (this.Response.Cookies["UserInfo"] != null)
                {
                    HttpCookie UserInfoCookie = new HttpCookie("UserInfo");
                    UserInfoCookie["Guid"] = guid;
                    UserInfoCookie["MainPage"] = "false";
                    Response.Cookies.Add(UserInfoCookie);
                }
                else
                {
                }
                GetImageInfo();
            }
            catch (Exception e)
            {
                LoggerHelper.LogException(MethodBase.GetCurrentMethod().Name, e, "GUID : " + guid);
            }
            return View();
        }

        public void GetImageInfo()
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            try
            {
                if (this.Response.Cookies["ImageInfo"] != null)
                {
                    var path = Server.MapPath("~/Content/Images/honda/");
                    var di = new DirectoryInfo(path);
                    var files = di.GetFiles();
                    HttpCookie UserInfoCookie = new HttpCookie("ImageInfo");
                    UserInfoCookie["NumOfImages"] = files.Length.ToString();
                    Response.Cookies.Add(UserInfoCookie);
                }
            }
            catch (Exception e)
            {
                LoggerHelper.LogException(MethodBase.GetCurrentMethod().Name, e);
            }
        }

        public ActionResult SlideShow()
        {
            // String RelativePath = Server.MapPath("../Content/Images/honda/").Replace(Request.ServerVariables["~/Content/Images/honda/"], String.Empty);
            return View();
        }

        public ActionResult LoadImage(string slideNum)
        {
            ViewBag.slideNum = slideNum;
            return PartialView("_ShowImage", ViewBag);
        }

        public ActionResult GeneratePopUp(string Message)
        {
            ViewBag.Message = Message;
            return PartialView("_PopUpPage");
        }

        public ActionResult Temp()
        {
            return View();
        }
    }
}