namespace PresentationApp.DataAccess.Models.Presentation
{
    public class UserCheckListItem
    {
        public virtual long Id { get; set; }

        public virtual string Name { get; set; }

        public virtual bool Checked { get; set; }
    }
}