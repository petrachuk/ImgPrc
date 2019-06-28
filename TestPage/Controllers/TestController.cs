using System;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using TestPage.Models;
using ImgLib;
using System.Diagnostics;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        // POST api/values
        [HttpPost]
        public ActionResult<ResponseWrapper> Post(RequestWrapper model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var file1 = Path.GetTempFileName();
            var file2 = Path.GetTempFileName();

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFile(model.RequestMergingImages.Request.UrlImage1, file1);
                webClient.DownloadFile(model.RequestMergingImages.Request.UrlImage2, file2);
            }

            var result = new ResponseWrapper();
            result.ResponseMergingImages.Response.RequestId = model.RequestMergingImages.Request.RequestId;

            long aHash1;
            long pHash1;
            long dHash1;
            long aHash2;
            long pHash2;
            long dHash2;

            var sw = Stopwatch.StartNew();
            sw.Start();

            using (var img1 = new ImgProcessing(file1))
            {
                aHash1 = img1.Processing(Mode.CropAHash);
                pHash1 = img1.Processing(Mode.CropPHash);
                dHash1 = img1.Processing(Mode.CropDHash);
            }

            using (var img2 = new ImgProcessing(file2))
            {
                aHash2 = img2.Processing(Mode.CropAHash);
                pHash2 = img2.Processing(Mode.CropPHash);
                dHash2 = img2.Processing(Mode.CropDHash);
            }

            var DiffrenseHashA = Convert.ToString(aHash1 ^ aHash2, 2).PadLeft(64, '0');
            var DiffrenseHashP = Convert.ToString(pHash1 ^ pHash2, 2).PadLeft(64, '0');
            var DiffrenseHashD = Convert.ToString(dHash1 ^ dHash2, 2).PadLeft(64, '0');

            var HammingDistanseHashA = DiffrenseHashA.ToCharArray().Count(x => x == '1');
            var HammingDistanseHashP = DiffrenseHashP.ToCharArray().Count(x => x == '1');
            var HammingDistanseHashD = DiffrenseHashD.ToCharArray().Count(x => x == '1');

            result.ResponseMergingImages.Response.ResultA = 100 - (HammingDistanseHashA / 64.0) * 100;
            result.ResponseMergingImages.Response.ResultP = 100 - (HammingDistanseHashP / 64.0) * 100;
            result.ResponseMergingImages.Response.ResultD = 100 - (HammingDistanseHashD / 64.0) * 100;

            sw.Stop();
            result.ResponseMergingImages.Response.ExecutionTimeMsec = sw.ElapsedMilliseconds;

            var file = new FileInfo(file1);
            if (file.Exists) file.Delete();

            file = new FileInfo(file2);
            if (file.Exists) file.Delete();

            return result;
        }
    }
}
