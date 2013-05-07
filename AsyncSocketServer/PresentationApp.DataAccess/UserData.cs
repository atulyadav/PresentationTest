using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NHibernate.Engine.Query;
using PresentationApp.DataAccess.Models.Account;
using PresentationApp.Domain;
using PresentationApp.Domain.Account;
using PresentationApp.Domain.Presentation;

namespace PresentationApp.DataAccess
{
    public class UserData
    {
        StackTrace stackTrace;
        MethodBase methodBase;

        public static void createSession()
        {
            Persister.createSession();
        }

        public bool IsHost(Guid hostGuid)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            Presentation presentation = Persister.Query<Presentation>()
                .Where(x => x.PresenterKey == hostGuid && x.Status == true)
                .SingleOrDefault<Presentation>();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            if (presentation != null)
                return true;
            else
                return false;
        }

        public bool IsClient(Guid clientGuid)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            UserPresentation userPresentation = Persister.Query<UserPresentation>()
                .Where(x => x.PresentationKey == clientGuid && x.Presentation.Status == true)
                .SingleOrDefault<UserPresentation>();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            if (userPresentation != null)
                return true;
            else
                return false;
        }

        public IList<UserPresentation> GetClientUrls()
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            IList<UserPresentation> clientUrls = null;
            try
            {
                clientUrls = Persister.Session.CreateCriteria<UserPresentation>().List<UserPresentation>().Where(x => x.Presentation.Status == true).ToList();
            }
            catch (Exception ex)
            {
            }

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return clientUrls;
        }

        public IList<Presentation> GetHostUrls()
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            IList<Presentation> hostUrls = Persister.Session.CreateCriteria<Presentation>().List<Presentation>().Where(x => x.Status == true).ToList();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return hostUrls;
        }

        public String GetClientName(Guid guid)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            UserPresentation userPresentation = Persister.Query<UserPresentation>()
                    .Where(x => x.PresentationKey == guid && x.Presentation.Status == true).SingleOrDefault<UserPresentation>();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return userPresentation.Users.Name;
        }

        public String GetHostName(Guid guid)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            Presentation presentation = Persister.Query<Presentation>()
                    .Where(x => x.PresenterKey == guid && x.Status == true).SingleOrDefault<Presentation>();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return presentation.Presenter.Name;
        }

        public List<Users> GetClientsName()
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            IList<UserPresentation> clientUrls = Persister.Session.CreateCriteria<UserPresentation>().List<UserPresentation>().Where(x => x.Presentation.Status == true).ToList();
            List<Users> users = new List<Users>();
            foreach (var u in clientUrls)
            {
                users.Add(u.Users);
            }

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return users;
        }

        //-----------------------------------------------------------

        public Users GetAdmin(string username, string password)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            Users presenter = Persister.Query<Users>()
                .Where(x => x.UserName == username && x.Password == password && x.IsAdmin == true && x.IsActive == true)
                .SingleOrDefault<Users>();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return presenter;
        }

        public Users GetPresenter(string username, string password)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            Users presenter = Persister.Query<Users>()
                .Where(x => x.UserName == username && x.Password == password && x.IsAdmin == false)
                .SingleOrDefault<Users>();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return presenter;
        }

        public Users GetPresenter(long Id)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            Users presenter = Persister.Query<Users>()
                .Where(x => x.Id == Id)
                .SingleOrDefault<Users>();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return presenter;
        }

        public Users GetUsers(long id)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            Users user = Persister.Query<Users>()
                .Where(x => x.Id == id)
                .SingleOrDefault<Users>();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return user;
        }

        public Users GetUsers(string username, string password)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            Users user = Persister.Query<Users>()
                .Where(x => x.UserName == username && x.Password == password && x.IsActive == true)
                .SingleOrDefault<Users>();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return user;
        }

        public Users GetUsers(string username)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            Users user = Persister.Query<Users>()
                .Where(x => x.UserName == username)
                .SingleOrDefault<Users>();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return user;
        }

        public Presentation GetPresentation(long id)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            Presentation presentation = Persister.Query<Presentation>()
                .Where(x => x.Id == id && x.Status == true)
                .SingleOrDefault<Presentation>();

            return presentation;
        }

        public Presentation GetPresentation(string name)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            Presentation presentation = Persister.Query<Presentation>()
                .Where(x => x.Name == name && x.Status == true)
                .SingleOrDefault<Presentation>();

            return presentation;
        }

        public Presentation GetPresentation(Guid guid)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            Presentation presentation = Persister.Query<Presentation>()
                .Where(x => x.PresenterKey == guid && x.Status == true)
                .SingleOrDefault<Presentation>();

            return presentation;
        }

        public IList<Presentation> GetPresentationList(long presenterId)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            IList<Presentation> presentations = Persister.Session.CreateCriteria<Presentation>().List<Presentation>().Where(x => x.Presenter == GetPresenter(presenterId) && x.Status == true).ToList();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return presentations;
        }

        public IList<Presentation> GetPresentationList()
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            IList<Presentation> presentations = Persister.Session.CreateCriteria<Presentation>().List<Presentation>().Where(x => x.Status == true).ToList();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return presentations;
        }

        public int CheckPresentationStartTime(DateTime startTime)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            int cnt = Persister.Session.CreateCriteria<Presentation>().List<Presentation>().Where(x => (startTime >= x.StartTime && startTime < x.EndTime) && x.Status == true).ToList().Count();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return cnt;
        }

        public int CheckPresentationEndTime(DateTime endTime)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            int cnt = Persister.Session.CreateCriteria<Presentation>().List<Presentation>().Where(x => (endTime > x.StartTime && endTime <= x.EndTime) && x.Status == true).ToList().Count();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return cnt;
        }

        public IList<Users> GetUnoccupiedPresenterList(long presentationId)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            Presentation presentation = GetPresentation(presentationId);

            IList<Users> presenters = Persister.Session.CreateCriteria<Users>().List<Users>().Where(x => x.IsActive == true).ToList();

            for (int i = 0; i < presenters.Count; i++)
            {
                if (checkOccupiedAsPresenter(presenters[i].Id, presentation.Id, presentation.StartTime, presentation.EndTime))
                {
                    presenters.RemoveAt(i);
                    i--;
                }
                else if (checkOccupiedAsUser(presenters[i].Id, presentation.Id, presentation.StartTime, presentation.EndTime))
                {
                    presenters.RemoveAt(i);
                    i--;
                }
            }

            presenters.Remove(presentation.Presenter);
            presenters.Insert(0, presentation.Presenter);

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return presenters;
        }

        public bool checkOccupiedAsPresenter(long userId)
        {
            IList<Presentation> presentationList = Persister.Session.CreateCriteria<Presentation>().List<Presentation>()
                    .Where(x => x.Status == true && x.Presenter != null && x.Presenter.Id == userId).ToList();

            return (presentationList.Count() > 0);
        }

        public bool checkOccupiedAsPresenter(long userId, long presentationId, DateTime startTime, DateTime endTime)
        {
            IList<Presentation> presentationsList = Persister.Session.CreateCriteria<Presentation>().List<Presentation>()
                    .Where(x => x.Presenter != null && x.Status == true && x.Id != presentationId && x.Presenter.Id == userId && ((startTime >= x.StartTime && startTime <= x.EndTime) || (endTime > x.StartTime && endTime <= x.EndTime))).ToList();

            return (presentationsList.Count() > 0);
        }

        public bool checkOccupiedAsUser(long userId, long presentationId, DateTime startTime, DateTime endTime)
        {
            IList<UserPresentation> userPresentationList = Persister.Session.CreateCriteria<UserPresentation>().List<UserPresentation>()
                .Where(x => x.Users.Id == userId && x.Presentation.Id != presentationId && ((startTime >= x.Presentation.StartTime && startTime <= x.Presentation.EndTime) || (endTime > x.Presentation.StartTime && endTime <= x.Presentation.EndTime))).ToList();

            return (userPresentationList.Count() > 0);
        }

        public IList<Users> GetUsersList()
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            IList<Users> users = Persister.Session.CreateCriteria<Users>().List<Users>().Where(x => x.IsActive == true).ToList();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return users;
        }

        public UserPresentation GetUsersPresentation(long presentationId, long userId)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            UserPresentation userPresentation = Persister.Session.CreateCriteria<UserPresentation>().List<UserPresentation>().Where(x => x.Presentation.Id == presentationId && x.Users.Id == userId && x.Presentation.Status == true).SingleOrDefault<UserPresentation>();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return userPresentation;
        }

        public IList<UserPresentation> GetUsersPresentationList(long presentationId)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            IList<UserPresentation> userPresentations = Persister.Session.CreateCriteria<UserPresentation>().List<UserPresentation>().Where(x => x.Presentation.Id == presentationId && x.Presentation.Status == true).ToList();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return userPresentations;
        }

        public bool UpdateUser(Users user)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            using (var transaction = Persister.Session.BeginTransaction())
            {
                Persister.Session.SaveOrUpdate(user);
                transaction.Commit();

                stackTrace = new StackTrace();
                methodBase = stackTrace.GetFrame(1).GetMethod();
                LoggerHelper.LogResponseToClient(methodBase.Name);

                return true;
            }
        }

        public bool UpdatePresentation(Presentation presentation)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            using (var transaction = Persister.Session.BeginTransaction())
            {
                Persister.Session.SaveOrUpdate(presentation);
                transaction.Commit();

                stackTrace = new StackTrace();
                methodBase = stackTrace.GetFrame(1).GetMethod();
                LoggerHelper.LogResponseToClient(methodBase.Name);

                return true;
            }
        }

        public PresentationStates GetPresentationState(int id)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            PresentationStates presentationState = Persister.Query<PresentationStates>()
                .Where(x => x.Id == id)
                .SingleOrDefault<PresentationStates>();

            stackTrace = new StackTrace();
            methodBase = stackTrace.GetFrame(1).GetMethod();
            LoggerHelper.LogResponseToClient(methodBase.Name);

            return presentationState;
        }

        public bool UpdateUserPresentation(UserPresentation userPresentation)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            using (var transaction = Persister.Session.BeginTransaction())
            {
                Persister.Session.Save(userPresentation);
                transaction.Commit();

                stackTrace = new StackTrace();
                methodBase = stackTrace.GetFrame(1).GetMethod();
                LoggerHelper.LogResponseToClient(methodBase.Name);

                return true;
            }
        }

        public bool DeleteUsersForPresentation(long presentationId)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            using (var transaction = Persister.Session.BeginTransaction())
            {
                Persister.Session.CreateSQLQuery("delete UserPresentation where PresentationId =:PresentationId")
                    .SetInt64("PresentationId", presentationId)
                    .ExecuteUpdate();

                transaction.Commit();

                stackTrace = new StackTrace();
                methodBase = stackTrace.GetFrame(1).GetMethod();
                LoggerHelper.LogResponseToClient(methodBase.Name);

                return true;
            }
        }

        public LoginReply CheckPresenterOrUser(Guid presentationKey, long userId)
        {
            LoginReply loginReply = new LoginReply();
            loginReply.PresentationKey = presentationKey;

            int userOrPresenter = 0; // 1 - user , 2 - presenter, 3 - user offline, 4 - presenter offline, 5 - presentation deleted, 0 - Invalid presentationKey
            long presentationId = 0L;
            Presentation presentation;

            UserPresentation userPresentation = Persister.Session.CreateCriteria<UserPresentation>().List<UserPresentation>()
                                                    .Where(x => x.PresentationKey == presentationKey && x.Users.Id == userId).SingleOrDefault();

            if (userPresentation != null)
            {
                userOrPresenter = 1;
                presentationId = userPresentation.Presentation.Id;
                presentation = userPresentation.Presentation;
            }
            else
            {
                presentation = Persister.Session.CreateCriteria<Presentation>().List<Presentation>().Where(x => x.PresenterKey == presentationKey && x.Presenter.Id == userId).SingleOrDefault();
                if (presentation != null)
                {
                    userOrPresenter = 2;
                    presentationId = presentation.Id;
                }
            }

            if (presentation != null)
            {
                if (presentation.Status == false)
                {
                    userOrPresenter = 5;
                }
                else if (!(DateTime.Now >= presentation.StartTime && DateTime.Now <= presentation.EndTime))
                {
                    userOrPresenter = userOrPresenter + 2;
                }

                loginReply.PresentationId = presentation.Id;
                loginReply.PresentationName = presentation.Name;
                loginReply.Description = presentation.Description;
            }

            loginReply.Status = userOrPresenter;

            if (userPresentation != null)
            {
                loginReply.UserId = userPresentation.Users.Id;
                loginReply.UserName = userPresentation.Users.Name;
            }
            else
            {
                if (presentation != null)
                {
                    loginReply.UserId = presentation.Presenter.Id;
                    loginReply.UserName = presentation.Presenter.Name;
                }
            }

            return loginReply;
        }

        public long GetPresentationId(Guid guid, bool isHost)
        {
            Presentation presentation = null;
            UserPresentation userPresentation = null;
            if (isHost)
            {
                presentation = GetPresentation(guid);
                return presentation.Id;
            }
            else
            {
                userPresentation = GetUsersPresentation(guid);
                return userPresentation.Presentation.Id;
            }
        }

        public bool UpdateUserConnectionStatus(Guid guid, bool isHost)
        {
            Presentation presentation = null;
            UserPresentation userPresentation = null;
            if (isHost)
            {
                presentation = GetPresentation(guid);
                presentation.IsHostConnected = false;
                UpdatePresentation(presentation);
            }
            else
            {
                userPresentation = GetUsersPresentation(guid);
                userPresentation.IsUserConnected = false;
                UpdateUserPresentation(userPresentation);
            }
            return false;
        }

        public UserPresentation GetUsersPresentation(Guid guid)
        {
            LoggerHelper.LogRequestRecieved(MethodBase.GetCurrentMethod().Name);

            UserPresentation userPresentation = Persister.Query<UserPresentation>()
                .Where(x => x.PresentationKey == guid && x.Presentation.Status == true)
                .SingleOrDefault<UserPresentation>();

            return userPresentation;
        }
    }
}