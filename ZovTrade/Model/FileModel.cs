using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZovTrade.Model
{
    public class FileModel
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public byte[] Data { get; set; }
    }
}
