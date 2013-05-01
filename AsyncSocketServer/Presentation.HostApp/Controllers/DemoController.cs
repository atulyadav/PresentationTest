using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Presentation.HostApp.Controllers
{
    public class DemoController : Controller
    {
        //
        // GET: /Demo/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetAllFileNames(string Params)
        {
            var path = Server.MapPath("~/Demo/swipe/");
            //var di = new DirectoryInfo(path);
            //var files = di.GetFiles();

            var filenames = Directory.GetFiles(path, "*.html").Select(filename => Path.GetFileNameWithoutExtension(filename));
            return Json(filenames);
        }
    }
}