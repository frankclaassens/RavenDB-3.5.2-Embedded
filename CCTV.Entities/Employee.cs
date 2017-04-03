using System;
using System.Collections.Generic;
using Raven.Client.UniqueConstraints;

namespace CCTV.Entities
{
    public class Employee
    {
        //public Guid Id { get; set; }

        [UniqueConstraint]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        //public string Title { get; set; }
        //public string HomePhone { get; set; }
        //public List<string> Notes { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public int EmployeeGroup { get; set; }

        public string Title { get; set; }
        //public List<string> Territories { get; set; }
    }
}