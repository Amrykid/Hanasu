using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Data.ID3
{
    public struct ID3Data
    {
        public ID3Version ID3Version { get; set; }
        public string Header { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
    }
}
