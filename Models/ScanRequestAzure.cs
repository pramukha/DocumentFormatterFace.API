using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocFormatterFace.API.Models
{
    public class ScanRequestAzure
    {
        public string FaceImage { get; set; }
        public string PPImage { get; set; }
    }
}
