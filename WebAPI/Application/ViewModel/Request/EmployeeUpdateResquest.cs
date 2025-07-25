﻿using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Request
{
    public class EmployeeUpdateResquest
    {

        public string? IdentityCard { get; set; }

        public DateOnly? Dob { get; set; }

        public string? FullName { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public Sex Sex { get; set; }
        public string? Avatar { get; set; }
        public string? Assign { get; set; }
        public double Salary { get; set; }
        public string? Position { get; set; }
    }
}
