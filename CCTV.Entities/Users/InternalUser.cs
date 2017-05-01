namespace CCTV.Entities.Users
{
    public class InternalUser : UserBase
    {
        private static readonly string Name = typeof(InternalUser).Name;

        private InternalUser()
        {
        }

        public override string EntityName
        {
            get
            {
                return Name;
            }
        }

        public override UserTypeOption UserType
        {
            get
            {
                return UserTypeOption.Internal;
            }
        }

        public InternalUserType InternalUserType { get; private set; }

        public string ExternalLabelFamilyId { get; internal set; }

        public string BusinessRole { get; set; }

        public string DefaultOfferEmailSignature { get; set; }           
    }
}