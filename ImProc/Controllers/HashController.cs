using System.IO;
using System.Threading.Tasks;
using ImgLib;
using ImProc.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImProc.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HashController : ControllerBase
    {
        public HashController()
        {
        }

        [HttpPost("gethashes")]
        public async Task<ActionResult> CalculateHashes()
        {
            // Получим информацию о файле
            var mime = Request.ContentType;
            if (mime == null) return StatusCode(415);   // Unsupported Media Type

            var filePath = Path.GetTempFileName();

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await Request.Body.CopyToAsync(fs);
            }

            var result = new HashResult();

            using (var img = new ImgProcessing(filePath))
            {
                result.AHash = img.Processing(Mode.CropAHash);
                result.PHash = img.Processing(Mode.CropPHash);
                result.DHash = img.Processing(Mode.CropDHash);
            }

            var file = new FileInfo(filePath);
            file.Delete();

            return Ok(result);
        }
    }
}
