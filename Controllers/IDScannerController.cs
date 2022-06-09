using DocFormatterFace.API.Models;
using DocFormatterFace.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DocFormatterFace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IDScannerController : ControllerBase
    {
        private readonly IIdScannerService _scannerService;
        public IDScannerController(IIdScannerService scannerService)
        {
            _scannerService = scannerService;
        }

        [HttpPost("scan-id")]
        public async Task<IActionResult> ScanIdDocument([FromBody] IdDocScanRequest scanRequest)
        {
            var res = await _scannerService.ScanIdentityDoc(scanRequest.imageString);

            return Ok(res);
        }

        [HttpPost("verify_face")]
        //[Authorize]
        public async Task<IActionResult> ScandocumentAzure([FromBody] ScanRequestAzure scanRequest)
        {
            var res = await _scannerService.Verify(scanRequest);

            return Ok(res);
        }
    }
}
