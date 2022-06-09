using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocFormatterFace.API.Models
{
    public class ScanResponse
    {
        public double Confidence { get; set; }
        public bool IsIdentical { get; set; }
    }
}
