namespace CCTV.Entities.Users
{
    public class ExternalUser : UserBase
    {
        public ExternalUser()
        {
        }

        private static readonly string Name = typeof(ExternalUser).Name;

        public override string EntityName
        {
            get { return Name; }
        }

        public override UserTypeOption UserType
        {
            get { return UserTypeOption.External; }
        }

        public ExternalUserType ExternalUserType { get; set; }
    }
}