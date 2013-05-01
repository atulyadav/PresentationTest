using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using PresentationApp.DataAccess;

namespace Presentation.HostApp.Controllers
{
    public class HumiraController : Controller
    {
        //
        // GET: /Humira/

        public ActionResult Index()
        {
            PresentationApp.DataAccess.LoggerHelper.Info("Testing");
            UserData userDate = new UserData();
            ViewBag.ClientUrls = userDate.GetClientUrls();
            ViewBag.HostUrls = userDate.GetHostUrls();
            ViewBag.ClientsName = userDate.GetClientsName();
            return View(ViewBag);
        }

        public void GetImageInfo()
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

        public ActionResult PopupWithHeader(string Params)
        {
            ViewBag.Params = Params;
            String str = Uri.UnescapeDataString(Params);
            string[] str2 = str.Split(',');
            Response.Redirect("../Humira/" + str2[0] + ".html");
            return null;
        }

        private static string ConvertStringArrayToString(string[] array)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string value in array)
            {
                builder.Append(value);
            }
            return builder.ToString();
        }

        public ActionResult Presentation(string guid)
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
            return View();
        }

        //To Get FileNames(html)
        public ActionResult GetHtmlfileNames(string Params)
        {
            string[] fileNames = { "../Humira/as-01.html", "../Humira/as-02.html", "../Humira/as-03.html", "../Humira/as-04.html", "../Humira/as-05.html" };

            //var path = Server.MapPath("~/Demo/swipe/");
            //var filenames = Directory.GetFiles(path, "*.html").Select(filename => Path.GetFileNameWithoutExtension(filename));
            return Json(fileNames);
        }
    }
}