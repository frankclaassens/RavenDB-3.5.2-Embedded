﻿using System;
using System.Collections.Generic;
using Raven.Client.UniqueConstraints;

namespace CCTV.Domain.Entities
{
    public class Employee
    {
        public string Id { get; set; }
        public string LastName { get; set; }

        [UniqueConstraint]
        public string FirstName { get; set; }
        public string Title { get; set; }
        public Address Address { get; set; }
        public DateTime HiredAt { get; set; }
        public DateTime Birthday { get; set; }
        public string HomePhone { get; set; }
        public string Extension { get; set; }
        public string ReportsTo { get; set; }
        public List<string> Notes { get; set; }
        public string Description { get; set; }


        public string Email { get; set; }

        public List<string> Territories { get; set; }
    }
}