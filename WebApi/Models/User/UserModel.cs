﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models.User
{
    public class UserModel
    {
        public long ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
    }
}