﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicTextFile
{
    class User
    {
        public int Id { get; set; } 
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool IsActive { get; set; }
    }
}

