﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.data.elasticsearch.responses
{
    public class DeleteResponse
    {
        public bool found { get; set; }
        public string _index { get; set; }
        public string _type { get; set; }
        public string _id { get; set; }
        public int _version { get; set; }
        public string result { get; set; }
    }
}