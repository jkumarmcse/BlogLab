﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogLap.Models
{
    public class Blog : BlogCreate
    {
        public string Username { get; set; }

        public int ApplicationUserId { get; set; }

        public DateTime PublishDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}