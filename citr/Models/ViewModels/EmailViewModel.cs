﻿using System.Collections.Generic;

namespace citr.Models.ViewModels
{
    public class EmailViewModel
    {
        public Employee Recipient { get; set; }
        public Request Request { get; set; }
        public List<Resource> Resources { get; set; }
        public string Url { get; set; }
        public List<RequestDetail> Details { get; set; }
    }
}
