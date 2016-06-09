﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawingManagerDll.Models
{
    public class IFCProjectInfo
    {
        public List<CopyDetails> Folders { get; set; }
        public List<CopyDetails> Files { get; set; }
        public List<StartFile> StartFiles { get; set; }
    }
}
