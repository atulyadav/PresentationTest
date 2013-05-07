using System.Collections.Generic;
using System.Linq;
using PresentationApp.Domain.Account;

namespace PresentationApp.DataAccess.Models.Presentation
{
    public class UserCheckList
    {
        public long PresentationId { get; set; }

        public string PresentationName { get; set; }

        public long PresenterId { get; set; }

        public IList<Users> PresenterList;

        public IList<UserCheckListItem> userCheckList { get; set; }

        public int flag { get; set; }

        public UserCheckList()
        {
        }

        public UserCheckList(int flg)
        {
            flag = flg;
        }

        public UserCheckList(string presentationName, long presentationId, int flg)
        {
            PresentationId = presentationId;
            PresentationName = presentationName;
            UserData userData = new UserData();
            IList<Users> userList = userData.GetUsersList();

            IList<UserPresentation> userPresentationList = userData.GetUsersPresentationList(presentationId);
            userCheckList = GenerateUserCheckList(userList, userPresentationList);

            PresenterList = userData.GetUnoccupiedPresenterList(PresentationId);

            flag = flg;
        }

        public IList<UserCheckListItem> GenerateUserCheckList(IList<Users> userList, IList<UserPresentation> userPresentationList)
        {
            IList<UserCheckListItem> userChkList = new List<UserCheckListItem>();

            bool chk;

            foreach (Users usr in userList)
            {
                chk = (userPresentationList.Where(x => x.Users == usr).ToList().Count()) == 1;

                userChkList.Add(new UserCheckListItem { Id = usr.Id, Name = usr.Name, Checked = chk });
            }

            return userChkList;
        }
    }
}