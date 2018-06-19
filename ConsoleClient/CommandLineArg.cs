﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient
{
    internal class CommandLineArg
    {
        public string Url { get; set; }

        public string IgnoreUrlFile { get; set; }

        public bool Verbose { get; set; }

        public int MaxDepth { get; set; }
    }
}
