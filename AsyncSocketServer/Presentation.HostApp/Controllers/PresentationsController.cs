using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PresentationApp.DataAccess;

namespace Presentation.HostApp.Controllers
{
    public class PresentationsController : Controller
    {
        //
        // GET: /Humira/

        public ActionResult Index()
        {
            UserData userData = new UserData();
            ViewBag.ClientUrls = userData.GetClientUrls();
            ViewBag.HostUrls = userData.GetHostUrls();
            ViewBag.ClientsName = userData.GetClientsName();
            return View("~/Views/presentations/Humira/Index.cshtml", ViewBag);
        }

        public ActionResult PopupWithHeader(string Params)
        {
            ViewBag.Params = Params;
            string presenatationName = null;
            String str = Uri.UnescapeDataString(Params);
            string[] popUpName = str.Split(',');
            if (this.Request.Cookies["PresenatationName"] != null)
            {
                var _presenatationName = Request.Cookies["PresenatationName"].Values;
                presenatationName = _presenatationName.ToString();
            }
            Response.Redirect("../Presentations/" + presenatationName + "/" + popUpName[0] + ".html");
            return null;
        }

        [Authorize]
        public ActionResult Presentation(string stat, string guid)
        {
            UserData usrData = new UserData();
            string pname = null;
            HttpCookie _guid = Request.Cookies["Guid"];
            HttpCookie _isHost = Request.Cookies["IsHost"];
            HttpCookie _presentationName = Request.Cookies["PresenatationName"];
            HttpCookie _userState = Request.Cookies["UserState"];
            if (_guid != null)
                _guid.Expires = DateTime.Now.AddHours(-20);
            if (_isHost != null)
                _isHost.Expires = DateTime.Now.AddHours(-20);
            if (_presentationName != null)
                _presentationName.Expires = DateTime.Now.AddHours(-20);
            if (_userState != null)
                _userState.Expires = DateTime.Now.AddHours(-20);

            HttpCookie n_guid = new HttpCookie("Guid");
            HttpCookie n_isHost = new HttpCookie("IsHost");
            HttpCookie n_presentationName = new HttpCookie("PresenatationName");
            HttpCookie n_userState = new HttpCookie("UserState");

            n_guid.Value = guid;
            n_userState.Value = stat;
            if (stat.Equals("2") || stat.Equals("4"))
            {
                n_isHost.Value = "True";
                pname = usrData.GetPresentation(new Guid(guid)).Name;
            }
            else if (stat.Equals("1") || stat.Equals("3"))
            {
                n_isHost.Value = "False";
                pname = usrData.GetUsersPresentation(new Guid(guid)).Presentation.Name;
            }
            //n_guid.Expires = DateTime.Today.AddMonths(12);
            // n_isHost.Expires = DateTime.Today.AddMonths(12);

            n_presentationName.Value = pname;

            Response.Cookies.Add(n_guid);
            Response.Cookies.Add(n_isHost);
            Response.Cookies.Add(n_presentationName);
            Response.Cookies.Add(n_userState);
            return View("~/Views/presentations/" + pname + "/Presentation.cshtml");
        }

        //To Get FileNames(html)
        public ActionResult GetHtmlfileNames(string Params)
        {
            //string[] fileNames = { "../Presentations/Humira/as-01.html", "../Presentations/Humira/as-02.html", "../Presentations/Humira/as-03.html", "../Presentations/Humira/as-04.html", "../Presentations/Humira/as-05.html", "../Presentations/Humira/as-isi-01.html" };

            if (this.Request.Cookies["Guid"] != null && this.Request.Cookies["IsHost"] != null)
            {
                UserData usrdata = new UserData();
                var sguid = Request.Cookies["Guid"].Values;
                var _isHost = Request.Cookies["IsHost"].Values;
                bool isHost = Boolean.Parse(_isHost.ToString());
                Guid guid = Guid.Parse(sguid.ToString());

                long id = usrdata.GetPresentationId(guid, isHost);
                var presentation = usrdata.GetPresentation(id);
                string[] fileNames = presentation.PresentationFileSequence.Split(',');
                return Json(fileNames);
            }
            return null;
        }

        // [Authorize]
        public ActionResult Logout()
        {
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

            //if (this.Request.Cookies["Guid"] != null && this.Request.Cookies["IsHost"] != null)
            //{
            //    UserData userData = new UserData();
            //    var sguid = Request.Cookies["Guid"].Values;
            //    var _isHost = Request.Cookies["IsHost"].Values;
            //    bool isHost = Boolean.Parse(_isHost.ToString());
            //    Guid guid = Guid.Parse(sguid.ToString());
            //    userData.UpdateUserConnectionStatus(guid, isHost);
            //}

            //return View("Login");
            return RedirectToAction("Login", "Account");
        }

        public ActionResult PlainPopup(string Params)
        {
            ViewBag.Params = Params;
            string presenatationName = null;
            String str = Uri.UnescapeDataString(Params);
            string[] popUpName = str.Split(',');
            if (this.Request.Cookies["PresenatationName"] != null)
            {
                var _presenatationName = Request.Cookies["PresenatationName"].Values;
                presenatationName = _presenatationName.ToString();
            }
            Response.Redirect("../Presentations/" + presenatationName + "/" + popUpName[0] + ".html");
            return null;
        }
    }
}