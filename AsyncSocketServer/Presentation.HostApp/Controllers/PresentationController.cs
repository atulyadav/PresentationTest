using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Ionic.Zip;
using Newtonsoft.Json;
using PresentationApp.DataAccess;
using PresentationApp.DataAccess.Models.Presentation;
using PresentationApp.Domain.Account;

namespace Presentation.HostApp.Controllers
{
    public class PresentationController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            UserData.createSession();

            UserData userData = new UserData();

            IList<PresentationApp.Domain.Presentation.Presentation> presentations = userData.GetPresentationList();

            return View(presentations);
        }

        [Authorize, ValidateInput(false)]
        public ActionResult CreatePresentation(string id, int flag)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            UserData.createSession();

            CreatePresentationRequest presentationReq = new CreatePresentationRequest();
            presentationReq.StartTime = DateTime.Now;
            presentationReq.EndTime = DateTime.Now;
            presentationReq.flag = flag;
            ViewBag.Flag = flag;
            ViewBag.Layout = "~/Views/Shared/_LinkLayout.cshtml";
            if (flag == 2)
            {
                UserData userData = new UserData();
                PresentationApp.Domain.Presentation.Presentation presentation = userData.GetPresentation(Convert.ToInt32(id));
                presentationReq.Id = presentation.Id;
                presentationReq.Name = presentation.Name;
                presentationReq.StartTime = presentation.StartTime;
                presentationReq.EndTime = presentation.EndTime;
                presentationReq.Timezone = presentation.Timezone;
                presentationReq.Description = HttpUtility.HtmlDecode(presentation.Description);

                ViewBag.Layout = "~/Views/Shared/_EditPresentationNav.cshtml";
                ViewBag.Flag = flag;
                ViewBag.id = id;
                ViewBag.PresentationName = presentation.Name;
            }

            return View(presentationReq);
        }

        [Authorize]
        [HttpPost, ValidateInput(false)]
        public ActionResult CreatePresentation(CreatePresentationRequest presentationReq)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            UserData.createSession();

            if (presentationReq != null)
            {
                bool chkCreate = (presentationReq.flag == 1);

                try
                {
                    UserData userData = new UserData();
                    PresentationApp.Domain.Presentation.Presentation presentation;

                    if (chkCreate)
                    {
                        presentation = new PresentationApp.Domain.Presentation.Presentation();
                    }
                    else
                    {
                        presentation = userData.GetPresentation(presentationReq.Id);
                        if (presentation.Presenter != null)
                        {
                            if (userData.checkOccupiedAsPresenter(presentation.Presenter.Id, presentation.Id, presentationReq.StartTime, presentationReq.EndTime))
                            {
                                TempData["Message"] = "Cannot update the presentation data as the host is occupied with another presentation at the selected time. Please change the time or choose another host.";
                                return RedirectToAction("CreatePresentation", new { id = presentation.Id, flag = 2 });
                            }
                        }
                    }

                    string presentationName = "";

                    if (presentation != null)
                    {
                        presentationName = presentation.Name;
                        presentation.Name = presentationReq.Name;
                        presentation.StartTime = presentationReq.StartTime;
                        presentation.EndTime = new DateTime(presentation.StartTime.Year, presentation.StartTime.Month, presentation.StartTime.Day, presentationReq.EndTime.Hour, presentationReq.EndTime.Minute, presentationReq.EndTime.Second);
                        presentation.Description = presentationReq.Description;
                        presentation.Timezone = presentationReq.Timezone;

                        if (chkCreate)
                        {
                            presentation.PresentationFileSequence = null;
                            presentation.PresentationStates = userData.GetPresentationState(2);
                            presentation.Presenter = null;
                            presentation.PresenterKey = Guid.NewGuid();
                            presentation.Status = true;
                        }

                        if (userData.UpdatePresentation(presentation))
                        {
                            if (chkCreate)
                            {
                                CreatePresentationViewDirectory(presentationReq.Name);

                                CreatePresentationDirectory(presentationReq.Name);

                                CreatePresentationViewFile(presentationReq.Name);

                                TempData["Message"] = "Presentation data was saved successfully.";

                                return RedirectToAction("UploadPresentationFolder", presentation);
                            }
                            else
                            {
                                if (presentationName != presentation.Name)
                                {
                                    RenamePresentationFolder(presentationName, presentation.Name);
                                    RenamePresentationViewFolder(presentationName, presentation.Name);
                                }

                                ViewBag.Layout = "~/Views/Shared/_EditPresentationNav.cshtml";
                                ViewBag.Flag = presentationReq.flag;
                                ViewBag.id = presentation.Id;
                                ViewBag.PresentationName = presentation.Name;

                                TempData["Message"] = "Presentation data was updated successfully.";

                                return RedirectToAction("CreatePresentation", new { id = presentationReq.Id, flag = presentationReq.flag });
                            }
                        }
                        else
                        {
                            TempData["Message"] = "Presentation was not updated. Please try again later.";
                            return RedirectToAction("CreatePresentation", new { id = presentationReq.Id, flag = presentationReq.flag });
                        }
                    }
                }
                catch (Exception e)
                {
                    TempData["Message"] = "There was an exception. Please try again later.";
                    LoggerHelper.LogException(MethodBase.GetCurrentMethod().Name, e, JsonConvert.SerializeObject(presentationReq));
                    return RedirectToAction("CreatePresentation", new { id = presentationReq.Id, flag = presentationReq.flag });
                }
            }
            return RedirectToAction("Index");
        }

        [Authorize, ValidateInput(false)]
        public ActionResult UploadPresentationFolder(PresentationApp.Domain.Presentation.Presentation presentation)
        {
            UserData.createSession();
            ViewBag.PresentationId = presentation.Id;
            ViewBag.PresentationName = presentation.Name;
            ViewBag.Flag = 1;
            return View();
        }

        [Authorize, ValidateInput(false)]
        [HttpPost]
        public ActionResult UploadPresentationFolder(string presentationId, string presentationName, HttpPostedFileBase zipfile)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            UserData.createSession();

            try
            {
                UserData usrData = new UserData();
                PresentationApp.Domain.Presentation.Presentation presentation = usrData.GetPresentation(Convert.ToInt32(presentationId));

                IList<string> list = new List<string>();
                //TODO: check the contents/structure of the uploaded zip folder
                if (zipfile == null || zipfile.ContentLength == 0)
                {
                    TempData["Message"] = "Sorry. The folder was not uploaded as it is empty.";
                }
                else if (!zipfile.FileName.Contains("zip"))
                {
                    TempData["Message"] = "Please upload a zip folder contatining all the files required for presentation.";
                }

                else
                {
                    var extractPath = Path.Combine(Server.MapPath("~/Presentations"), presentationName);

                    CreatePresentationDirectory(presentationName);

                    ZipFile zip = ZipFile.Read(zipfile.InputStream);
                    zip.ExtractAll(extractPath);

                    foreach (ZipEntry e in zip)
                    {
                        if (e.FileName.EndsWith(".html") || e.FileName.EndsWith(".htm"))
                        {
                            list.Add(e.FileName.Substring(e.FileName.LastIndexOf('/') + 1));
                        }
                    }

                    presentation.PresentationFileSequence = String.Join(",", list.ToArray());

                    if (usrData.UpdatePresentation(presentation))
                    {
                        TempData["Message"] = "Presentation was uploaded successfully.";
                    }
                }

                ViewBag.PresentationId = presentationId;
                ViewBag.PresentationName = presentationName;
                ViewBag.List = list;
                ViewBag.Flag = 1;
                return View();
            }
            catch (Exception e)
            {
                LoggerHelper.LogException(MethodBase.GetCurrentMethod().Name, e, JsonConvert.SerializeObject(zipfile));
                return View();
            }
        }

        [Authorize]
        [HttpPost]
        public JsonResult SubmitList(string presentationId, string PresentationFileSequence, int flag, string PostBackTo)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            UserData.createSession();

            try
            {
                UserData userData = new UserData();
                PresentationApp.Domain.Presentation.Presentation presentation;
                presentation = userData.GetPresentation((Convert.ToInt64(presentationId)));
                presentation.PresentationFileSequence = PresentationFileSequence;

                if (userData.UpdatePresentation(presentation))
                {
                    if (flag == 2)
                    {
                        return Json(new { redirectTo = Url.Action(PostBackTo, new { id = presentation.Id, presentationName = presentation.Name, flag = 2 }), Message = "The sequence has been reordered." });
                    }
                    else
                    {
                        return Json(new { redirectTo = Url.Action("AddPresentationUsers", new { id = presentation.Id, presentationName = presentation.Name, flag = 1 }) });
                    }
                }
                else
                {
                    ViewBag.Message = "Could not save the file sequence.";
                    return Json(new { });
                }
            }
            catch (Exception e)
            {
                return Json(new { });
            }
        }

        [Authorize]
        public ActionResult AddPresentationUsers(int id, string presentationName, int flag)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            UserData usrData = new UserData();

            if (!CheckPresentationFolderExistsAndNotEmpty(presentationName))
            {
                TempData["Message"] = "You cannot allot users to presentation before uploading the presentation.";
                return RedirectToAction("AddFiles", new { id = id, presentationName = presentationName });
            }

            UserData.createSession();

            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            ViewBag.Layout = "~/Views/Shared/_LinkLayout.cshtml";

            if (flag == 2) { ViewBag.Layout = "~/Views/Shared/_EditPresentationNav.cshtml"; ViewBag.PresentationName = presentationName; ViewBag.PostBackTo = "AddPresentationUsers"; }

            UserCheckList userChkList = new UserCheckList(presentationName, id, flag);
            ViewBag.id = id;
            ViewBag.Flag = flag;
            return View(userChkList);
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddPresentationUsers(UserCheckList userChkList)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            UserData.createSession();

            UserData usrData = new UserData();
            UserPresentation usrPresentation;
            if (userChkList != null)
            {
                PresentationApp.Domain.Presentation.Presentation presentation = usrData.GetPresentation(userChkList.PresentationId);
                usrData.DeleteUsersForPresentation(userChkList.PresentationId);
                presentation.Presenter = usrData.GetPresenter(userChkList.PresenterId);
                string domain = Request.Url.Authority;
                string presenterUrl = "http://" + domain + "/Account/Login?id=" + presentation.PresenterKey;

                if (usrData.UpdatePresentation(presentation))
                {
                    SendEmail(presentation.Presenter.Name, presentation.Presenter.EmailId, presentation.Name, "Host", presentation.StartTime.ToString(), presentation.EndTime.ToString(), presentation.Timezone, presenterUrl, presentation.Description);

                    foreach (var item in userChkList.userCheckList)
                    {
                        if (item.Checked == true)
                        {
                            usrPresentation = new UserPresentation();
                            usrPresentation.PresentationKey = Guid.NewGuid();
                            usrPresentation.Presentation = usrData.GetPresentation(userChkList.PresentationId);

                            Users usr = usrData.GetUsers(item.Id);

                            usrPresentation.Users = usr;
                            usrData.UpdateUserPresentation(usrPresentation);

                            string UserUrl = "http://" + domain + "/Account/Login?id=" + usrPresentation.PresentationKey;
                            SendEmail(usr.Name, usr.EmailId, presentation.Name, "Client", presentation.StartTime.ToString(), presentation.EndTime.ToString(), presentation.Timezone, UserUrl, presentation.Description);
                        }
                    }

                    TempData["Message"] = "The Presenters and Users have been successfully updated for presentation.";
                }

                if (userChkList.flag == 2)
                {
                    ViewBag.Layout = "~/Views/Shared/_EditPresentationNav.cshtml";
                    ViewBag.Flag = userChkList.flag;
                    ViewBag.id = presentation.Id;
                    ViewBag.PresentationName = presentation.Name;
                    return RedirectToAction("AddPresentationUsers", new { id = userChkList.PresentationId, presentationName = userChkList.PresentationName, flag = userChkList.flag });
                }
                else if (userChkList.flag == 1)
                {
                    return RedirectToAction("Index");
                }
            }

            return View();
        }

        [Authorize]
        public ActionResult DeletePresentation(int id)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);
            UserData.createSession();
            UserData userData = new UserData();
            PresentationApp.Domain.Presentation.Presentation presentation = userData.GetPresentation(id);
            ViewBag.PresentationName = presentation.Name;
            ViewBag.id = id;
            ViewBag.Flag = 2;
            return PartialView(presentation);
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeletePresentation(PresentationApp.Domain.Presentation.Presentation presentation)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            UserData.createSession();

            try
            {
                UserData userData = new UserData();
                PresentationApp.Domain.Presentation.Presentation temp = userData.GetPresentation(presentation.Id);
                var pathOriginalFolder = Path.Combine(Server.MapPath("~/Presentations"), temp.Name);
                var pathOriginalView = Path.Combine(Server.MapPath("~/Views/Presentations"), temp.Name);
                temp.Status = false;
                temp.Name = temp.Name + "_deleted";
                if (userData.UpdatePresentation(temp))
                {
                    if (Directory.Exists(pathOriginalFolder)) { Directory.Move(pathOriginalFolder, Path.Combine(Server.MapPath("~/Presentations"), temp.Name)); }
                    if (Directory.Exists(pathOriginalView)) { Directory.Move(pathOriginalView, Path.Combine(Server.MapPath("~/Views/Presentations"), temp.Name)); }
                    TempData["Message"] = "Presentation was successfully deleted";
                }
                else
                {
                    TempData["Message"] = "Presentation was not deleted. Please try again later.";
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                LoggerHelper.LogException(MethodBase.GetCurrentMethod().Name, e, JsonConvert.SerializeObject(presentation));
                TempData["Message"] = "Presentation was not deleted. Please try again later.";
                return RedirectToAction("Index");
            }
        }

        public ActionResult ReorderFiles(int id, string presentationName)
        {
            UserData.createSession();
            ViewBag.PresentationId = id;
            ViewBag.PresentationName = presentationName;
            UserData userData = new UserData();
            PresentationApp.Domain.Presentation.Presentation presentation = userData.GetPresentation(Convert.ToInt64(id));
            string files = presentation.PresentationFileSequence;

            DirectoryInfo presentationDir = new DirectoryInfo(Path.Combine(Server.MapPath("~/Presentations"), presentation.Name));

            FileInfo[] filesCollection = getFilesFromDirectory(presentationDir);

            filesCollection = (from f in filesCollection where f.Extension.Contains("htm") select f).ToArray();

            IList<string> filesIncluded = new List<string>();
            IList<string> filesExcluded = new List<string>();

            if (filesCollection == null)
            {
                if (presentation.PresentationFileSequence != null)
                {
                    presentation.PresentationFileSequence = null;
                    userData.UpdatePresentation(presentation);
                }
                TempData["Message"] = "Please upload the presentation inorder to choose the file sequence.";
                return RedirectToAction("AddFiles", new { id = id, presentationName = presentationName });
            }
            else
            {
                if (files != null)
                {
                    IList<string> currentSeq = files.Split(',').ToList<string>();

                    if (filesCollection != null && filesCollection.Count() != 0)
                    {
                        foreach (FileInfo f in filesCollection)
                        {
                            if (currentSeq.Contains(f.Name))
                            {
                                filesIncluded.Add(f.Name);
                            }
                            else
                            {
                                filesExcluded.Add(f.Name);
                            }
                        }
                    }
                }
            }

            ViewBag.List = filesIncluded;
            ViewBag.ListExcluded = filesExcluded;
            ViewBag.id = id;
            ViewBag.Flag = 2;
            ViewBag.PostBackTo = "ReorderFiles";
            return View();
        }

        public ActionResult AddFiles(int id, string presentationName)
        {
            UserData.createSession();
            checkPresentationFolderExists(presentationName);
            ViewBag.PresentationId = id;
            ViewBag.id = id;
            ViewBag.Flag = 2;
            ViewBag.PresentationName = presentationName;
            ViewBag.PostBackTo = "AddFiles";
            return View();
        }

        [HttpPost]
        public ActionResult AddFiles(string presentationId, HttpPostedFileBase zipFile)
        {
            UserData.createSession();
            var cntHtml = 0;
            var fileExists = 0;
            var pathTemp = Path.Combine(Server.MapPath("~/Presentations"), "temp");

            UserData userData = new UserData();
            PresentationApp.Domain.Presentation.Presentation presentation = userData.GetPresentation(Convert.ToInt64(presentationId));
            var pathPresentationFolder = Path.Combine(Server.MapPath("~/Presentations"), presentation.Name);
            string pathSpecificFolder;
            string fileseq = "";
            IList<string> filesList = new List<string>();

            checkPresentationFolderExists(presentation.Name);

            if (presentation.PresentationFileSequence != null)
            {
                fileseq = presentation.PresentationFileSequence;
                filesList = fileseq.Split(',').ToList();
            }
            FileInfo[] files;

            if (zipFile != null && zipFile.ContentLength > 0)
            {
                if (Directory.Exists(pathTemp))
                {
                    Directory.Delete(pathTemp, true);
                }

                Directory.CreateDirectory(pathTemp);

                if (zipFile.ContentType.Contains("zip"))
                {
                    ZipFile zip = ZipFile.Read(zipFile.InputStream);
                    zip.ExtractAll(pathTemp);

                    DirectoryInfo dir = new DirectoryInfo(pathTemp);
                    files = getFilesFromDirectory(dir);
                }
                else
                {
                    files = new FileInfo[1];
                    zipFile.SaveAs(Path.Combine(pathTemp, zipFile.FileName));
                    files[0] = new FileInfo(Path.Combine(pathTemp, zipFile.FileName));
                }

                foreach (FileInfo file in files)
                {
                    fileExists = 0;
                    pathSpecificFolder = pathPresentationFolder;

                    string fileExtension = file.Name.Substring(file.Name.LastIndexOf(".") + 1);

                    if (fileExtension == "htm")
                    {
                        fileExtension = "html";
                    }

                    FileInfo f = new FileInfo(Path.Combine(pathSpecificFolder, file.Name));
                    if (f.Exists)
                    {
                        fileExists = 1;
                        f.Delete();
                    }

                    var fname_deleted = "";
                    if (f.Name.LastIndexOf('.') > 0)
                    {
                        fname_deleted = f.Name.Substring(0, f.Name.LastIndexOf('.')) + "_deleted" + f.Name.Substring(f.Name.LastIndexOf('.') + 1);

                        FileInfo f_deleted = new FileInfo(Path.Combine(pathSpecificFolder, fname_deleted));
                        if (f_deleted.Exists)
                        {
                            f_deleted.Delete();
                        }

                        if (Directory.Exists(Path.Combine(pathPresentationFolder, fileExtension)))
                        {
                            pathSpecificFolder = Path.Combine(pathPresentationFolder, fileExtension);
                        }

                        f = new FileInfo(Path.Combine(pathSpecificFolder, file.Name));
                        if (f.Exists)
                        {
                            fileExists = 1;
                            f.Delete();
                        }

                        fname_deleted = f.Name.Substring(0, f.Name.LastIndexOf('.')) + "_deleted" + f.Extension;
                        f_deleted = new FileInfo(Path.Combine(pathSpecificFolder, fname_deleted));
                        if (f_deleted.Exists)
                        {
                            f_deleted.Delete();
                        }
                    }
                    file.MoveTo(Path.Combine(pathSpecificFolder, file.Name));

                    if (fileExtension == "html" && fileExists != 1)
                    {
                        filesList.Add(file.Name);
                        cntHtml = cntHtml + 1;
                    }
                }

                TempData["Message"] = "Files uploaded successfully.";

                if (cntHtml > 0)
                {
                    presentation.PresentationFileSequence = string.Join(",", filesList.ToArray());
                    userData.UpdatePresentation(presentation);
                    ViewBag.PresentationId = presentationId;
                    ViewBag.PresentationName = presentation.Name;
                    ViewBag.List = filesList;
                    ViewBag.id = presentationId;
                    ViewBag.Flag = 2;
                    ViewBag.PostBackTo = "AddFiles";
                    return View();
                }
            }
            else
            {
                TempData["Message"] = "Sorry. The folder/file was not uploaded as it is empty.";
            }

            return RedirectToAction("AddFiles", new { id = presentation.Id, presentationName = presentation.Name });
        }

        [Authorize]
        public ActionResult DeleteFiles(int id, string presentationName)
        {
            UserData.createSession();
            UserData userData = new UserData();
            PresentationApp.Domain.Presentation.Presentation presentation = userData.GetPresentation(id);
            var path1 = Path.Combine(Server.MapPath("~/Presentations"), presentation.Name);
            DirectoryInfo dir = new DirectoryInfo(path1);

            if (!dir.Exists)
            {
                TempData["Message"] = "Cannot delete files. The presentation folder doesnt exist.";
                return RedirectToAction("AddFiles", new { id = id, presentationName = presentationName });
            }

            DeleteFilesRequest deleteFilesReq = new DeleteFilesRequest();

            deleteFilesReq.PresentationId = presentation.Id;
            deleteFilesReq.PresentationName = presentation.Name;

            IList<FileInfo> filesList = new List<FileInfo>();
            filesList = getFilesFromDirectory(dir);

            filesList = (from fi in filesList where fi.Name.LastIndexOf('.') >= 0 select fi).ToList<FileInfo>();

            if (filesList.Count != 0)
            {
                var extensions = new string[] { ".html", ".htm" };

                foreach (var file in (from fi in filesList where extensions.Contains(fi.Name.Substring(fi.Name.LastIndexOf('.'))) select fi).ToList<FileInfo>())
                {
                    deleteFilesReq.HTMLList.Add(new DeleteFilesListItem() { FileLocation = file.FullName.Replace('\\', '~'), FileName = file.Name });
                }

                extensions = new string[] { ".css" };

                foreach (var file in (from fi in filesList where extensions.Contains(fi.Name.Substring(fi.Name.LastIndexOf('.'))) select fi).ToList<FileInfo>())
                {
                    deleteFilesReq.CSSList.Add(new DeleteFilesListItem() { FileLocation = file.FullName.Replace('\\', '~'), FileName = file.Name });
                }

                extensions = new string[] { ".js" };

                foreach (var file in (from fi in filesList where extensions.Contains(fi.Name.Substring(fi.Name.LastIndexOf('.'))) select fi).ToList<FileInfo>())
                {
                    deleteFilesReq.JSList.Add(new DeleteFilesListItem() { FileLocation = file.FullName.Replace('\\', '~'), FileName = file.Name });
                }

                extensions = new string[] { ".jpg", ".png", ".bmp", ".ico", ".gif", ".tif" };

                foreach (var file in (from fi in filesList where extensions.Contains(fi.Name.Substring(fi.Name.LastIndexOf('.'))) select fi).ToList<FileInfo>())
                {
                    deleteFilesReq.ImagesList.Add(new DeleteFilesListItem() { FileLocation = file.FullName.Replace('\\', '~'), FileName = file.Name });
                }

                if (deleteFilesReq.CheckListsEmpty())
                {
                    ViewBag.PresentationId = id;
                    ViewBag.PresentationName = presentationName;
                    ViewBag.id = id;
                    ViewBag.Flag = 2;
                    return View(deleteFilesReq);
                }
                else
                {
                    TempData["Message"] = "Cannot delete files. The presentation folder does not contain any HTML/CSS/JS/Image files.";
                    return RedirectToAction("AddFiles", new { id = id, presentationName = presentationName });
                }
            }
            else
            {
                TempData["Message"] = "Cannot delete files. The presentation folder is empty.";
                return RedirectToAction("AddFiles", new { id = id, presentationName = presentationName });
            }
        }

        [Authorize]
        [HttpPost]
        //ToDo: check if this is required... not used currently
        public ActionResult DeleteFiles(long presentationId, string presentationName, IList<string> list)
        {
            UserData.createSession();
            UserData userData = new UserData();

            PresentationApp.Domain.Presentation.Presentation presentation = userData.GetPresentation(presentationId);

            IList<string> fileSeq = presentation.PresentationFileSequence.Split(',').ToList();

            FileInfo file;

            foreach (string s in list)
            {
                file = new FileInfo(s);

                if (fileSeq.Contains(file.Name))
                {
                    fileSeq.Remove(file.Name);
                }

                if (file.Exists)
                {
                    string temp1 = s.Substring(0, s.LastIndexOf("\\"));
                    string temp2 = file.Name.Substring(0, file.Name.LastIndexOf(".")) + "_deleted" + file.Extension;
                    var temp3 = Path.Combine(temp1, temp2);
                    file.MoveTo(temp3);
                }
            }

            string newFileSequence = string.Join(",", fileSeq.ToArray());
            presentation.PresentationFileSequence = newFileSequence;
            userData.UpdatePresentation(presentation);

            return View();
        }

        [Authorize]
        public JsonResult DeleteFile(long presentationId, string presentationName, string fileLocation)
        {
            UserData.createSession();
            fileLocation = fileLocation.Replace('~', '\\');

            UserData userData = new UserData();
            IList<string> fileSeq = new List<string>();

            try
            {
                PresentationApp.Domain.Presentation.Presentation presentation = userData.GetPresentation(presentationId);

                if (presentation.PresentationFileSequence != null)
                {
                    fileSeq = presentation.PresentationFileSequence.Split(',').ToList();
                }

                FileInfo file = new FileInfo(fileLocation);

                if (fileSeq.Contains(file.Name))
                {
                    fileSeq.Remove(file.Name);
                }

                if (file.Exists)
                {
                    string temp1 = fileLocation.Substring(0, fileLocation.LastIndexOf("\\"));
                    string temp2 = file.Name.Substring(0, file.Name.LastIndexOf(".")) + "_deleted" + file.Extension;
                    var temp3 = Path.Combine(temp1, temp2);
                    file.MoveTo(temp3);

                    if (fileSeq.Count == 0)
                        presentation.PresentationFileSequence = null;
                    else
                        presentation.PresentationFileSequence = string.Join(",", fileSeq.ToArray());

                    if (userData.UpdatePresentation(presentation))
                    {
                        TempData["Message"] = "The selected file deleted successfully.";
                    }
                    else
                    {
                        TempData["Message"] = "The file was not deleted.";
                    }
                }
            }
            catch (Exception e)
            {
                TempData["Message"] = "There was an exception. Please try again later.";
                LoggerHelper.LogException(MethodBase.GetCurrentMethod().Name, e, JsonConvert.SerializeObject(new { presentationId = presentationId, presentationName = presentationName }));
            }

            return Json(new { redirectTo = Url.Action("DeleteFiles", new { id = presentationId, presentationName = presentationName }) });
        }

        public ActionResult ModifyFile(long id, string presentationName)
        {
            ViewBag.PresentationId = id;
            ViewBag.PresentationName = presentationName;
            ViewBag.id = id;
            ViewBag.Flag = 2;

            FileInfo f = new FileInfo(Path.Combine(Server.MapPath("~/Views/Presentations"), presentationName, "Presentation.cshtml"));

            if (f.Exists)
            {
                using (FileStream presentationFile = new FileStream(Path.Combine(Server.MapPath("~/Views/Presentations"), presentationName, "Presentation.cshtml"), FileMode.Open, FileAccess.Read))
                {
                    StreamReader sr = new StreamReader(presentationFile);

                    ViewBag.Contents = sr.ReadToEnd();
                }
            }

            f = null;

            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult ModifyFile(long presentationId, string presentationName, string file)
        {
            using (FileStream presentationFile = new FileStream(Path.Combine(Server.MapPath("~/Views/Presentations"), presentationName, "Presentation.cshtml"), FileMode.Truncate, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(presentationFile);

                sw.Write(file);
                sw.Flush();
            }
            return RedirectToAction("ModifyFile", "Presentation", new { id = presentationId, presentationName = presentationName });
        }

        public void SendEmail(string name, string address, string presentationName, string userType, string startTime, string endTime, string timezone, string url, string description)
        {
            UserData.createSession();
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            string email = ConfigurationManager.AppSettings["smtpEmailFrom"];
            var msg = new MailMessage();
            using (var smtpClient = new SmtpClient(ConfigurationManager.AppSettings["smtpServer"]))
            {
                msg.From = new MailAddress(email);
                msg.To.Add(new MailAddress(address));
                msg.Subject = ConfigurationManager.AppSettings["smtpEmailSubject"];

                FileStream fs = new FileStream(Server.MapPath("~/Content/Templates/EmailTemplate.htm"), FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);

                string temp = sr.ReadToEnd();

                description = HttpUtility.HtmlDecode(description);

                temp = string.Format(temp, name, presentationName, userType, startTime, endTime, timezone, url, description);

                msg.Body = temp;
                msg.IsBodyHtml = true;

                smtpClient.EnableSsl = false;
                smtpClient.UseDefaultCredentials = false;

                smtpClient.Send(msg);
            }
        }

        public JsonResult CheckPresentationExists(string Name, string data)
        {
            int flag = Convert.ToInt32(data.Substring(data.Length - 1));
            string oldName = "";
            if (flag == 2) oldName = data.Substring(0, data.Length - 1);

            UserData.createSession();
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            result.Data = true;

            if (flag == 1 || (flag == 2 && oldName != Name))
            {
                UserData userData = new UserData();

                if (userData.GetPresentation(Name) != null)
                {
                    result.Data = false;
                }
            }

            return result;
        }

        public FileInfo[] getFilesFromDirectory(DirectoryInfo dir)
        {
            UserData.createSession();
            FileInfo[] fileinfo;
            fileinfo = dir.GetFiles();
            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo d in dirs)
            {
                fileinfo = (FileInfo[])fileinfo.Concat(d.GetFiles()).ToArray();
            }

            return
                (from f in fileinfo where !f.Name.Contains("_deleted") select f).ToArray();
        }

        public void checkPresentationFolderExists(string presentationName)
        {
            UserData.createSession();
            var presentationFolderpath = Path.Combine(Server.MapPath("~/Presentations"), presentationName);
            if (!Directory.Exists(presentationFolderpath)) { Directory.CreateDirectory(presentationFolderpath); TempData["Message"] = "Could not find the presentation folder hence a new blank folder was created."; }
            if (!Directory.Exists(Path.Combine(presentationFolderpath, "html"))) { Directory.CreateDirectory(Path.Combine(presentationFolderpath, "html")); }
            if (!Directory.Exists(Path.Combine(presentationFolderpath, "css"))) { Directory.CreateDirectory(Path.Combine(presentationFolderpath, "css")); }
            if (!Directory.Exists(Path.Combine(presentationFolderpath, "js"))) { Directory.CreateDirectory(Path.Combine(presentationFolderpath, "js")); }
        }

        public bool CheckPresentationFolderExistsAndNotEmpty(string presentationName)
        {
            JsonResult result = new JsonResult();

            var presentationFolderPath = Path.Combine(Server.MapPath("~/Presentations"), presentationName);

            if (Directory.Exists(presentationFolderPath))
            {
                DirectoryInfo dir = new DirectoryInfo(presentationFolderPath);
                if (getFilesFromDirectory(dir).Count() > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public void CreatePresentationDirectory(string presentationName)
        {
            var presentationDirectoryPath = Path.Combine(Server.MapPath("~/Presentations"), presentationName);
            DirectoryInfo dirPresentationDirectory = new DirectoryInfo(presentationDirectoryPath);
            if (dirPresentationDirectory.Exists) { dirPresentationDirectory.Delete(true); }
            dirPresentationDirectory.Create();
            dirPresentationDirectory = null;
        }

        public void CreatePresentationViewDirectory(string presentationName)
        {
            var viewDirectoryPath = Path.Combine(Server.MapPath("~/Views/Presentations"), presentationName);
            DirectoryInfo dirViewDirectory = new DirectoryInfo(viewDirectoryPath);
            if (dirViewDirectory.Exists) { dirViewDirectory.Delete(true); }
            dirViewDirectory.Create();
            dirViewDirectory = null;
        }

        public void CreatePresentationViewFile(string presentationName)
        {
            var presentationFilePath = Path.Combine(Server.MapPath("~/Views/Presentations"), presentationName, "Presentation.cshtml");
            using (FileStream presentationFile = new FileStream(presentationFilePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(presentationFile);
                using (FileStream fs = new FileStream(Server.MapPath("~/Content/Templates/Presentation.cshtml"), FileMode.Open, FileAccess.Read))
                {
                    StreamReader sr = new StreamReader(fs);
                    string temp = sr.ReadToEnd();
                    temp = string.Format(temp, presentationName, presentationName, presentationName, presentationName);
                    sw.WriteLine(temp);
                    sw.Flush();
                }
            }
        }

        public void RenamePresentationFolder(string oldName, string newName)
        {
            var folderPath = Path.Combine(Server.MapPath("~/Presentations"), oldName);
            {
                var dir = new DirectoryInfo(folderPath);
                if (dir.Exists)
                {
                    var newDir = new DirectoryInfo(Path.Combine(Server.MapPath("~/Presentations"), newName));
                    if (newDir.Exists) { newDir.Delete(true); }
                    newDir = null;
                    dir.MoveTo(Path.Combine(Server.MapPath("~/Presentations"), newName));
                }
                else
                    CreatePresentationDirectory(newName);
                dir = null;
            }
        }

        public void RenamePresentationViewFolder(string oldName, string newName)
        {
            var viewDirectoryPath = Path.Combine(Server.MapPath("~/Views/Presentations"), oldName);
            {
                var dir = new DirectoryInfo(viewDirectoryPath);
                if (dir.Exists)
                {
                    var newDir = new DirectoryInfo(Path.Combine(Server.MapPath("~/Views/Presentations"), newName));
                    if (newDir.Exists) { newDir.Delete(true); }
                    newDir = null;
                    dir.MoveTo(Path.Combine(Server.MapPath("~/Views/Presentations"), newName));
                }
                else
                {
                    CreatePresentationViewDirectory(newName);
                    CreatePresentationViewFile(newName);
                }
                dir = null;
            }
        }
    }
}