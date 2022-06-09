using DocFormatterFace.API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DocFormatterFace.API.Services
{
    public interface IIdScannerService
    {
        public Task<IdentityDoc> ScanIdentityDoc(string imageString);
        public Task<ScanResponse> Verify(ScanRequestAzure scanRequest);
    }
}
