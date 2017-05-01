using System;
using Raven.Client.UniqueConstraints;

namespace CCTV.Entities.Users
{
    public abstract class UserBase
    {
        protected UserBase()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public abstract string EntityName { get; }

        public abstract UserTypeOption UserType { get; }

        public Guid Self
        {
            get
            {
                return Id;
            }
        }

        [UniqueConstraint]
        public string NameIdentifier { get; set; }

        public string EmailAddress { get; set; }

        public string DisplayName { get; set; }
    }
}