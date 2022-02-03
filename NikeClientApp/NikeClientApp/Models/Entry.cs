﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NikeClientApp.Models
{
    public class Entry
    {
        public string Description { get; set; }
        public string UserName { get; set; }
        public long? Rating { get; set; }
        public POI POI { get; set; }
    }
}
