using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImgLib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestPage.Models;

namespace TestPage.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ViewResult> Upload(List<IFormFile> files)
        {
            var sw = Stopwatch.StartNew();
            var result = new ResultModel();
            var first = true;

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var filePath = Path.GetTempFileName();

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }

                    var img = new ImgProcessing(filePath);

                    if (first)
                    {
                        sw.Start();
                        result.Img1Hash = img.Processing(Mode.CropAHash);
                        sw.Stop();
                        result.TimeHash += sw.ElapsedMilliseconds;

                        sw.Start();
                        result.Img1PHash = img.Processing(Mode.CropPHash);
                        sw.Stop();
                        result.TimePHash += sw.ElapsedMilliseconds;

                        sw.Start();
                        result.Img1Magic = img.Processing(Mode.CropDHash);
                        sw.Stop();
                        result.TimeMagic += sw.ElapsedMilliseconds;

                        first = false;
                    }
                    else
                    {
                        sw.Start();
                        result.Img2Hash = img.Processing(Mode.CropAHash);
                        sw.Stop();
                        result.TimeHash += sw.ElapsedMilliseconds;

                        sw.Start();
                        result.Img2PHash = img.Processing(Mode.CropPHash);
                        sw.Stop();
                        result.TimePHash += sw.ElapsedMilliseconds;

                        sw.Start();
                        result.Img2Magic = img.Processing(Mode.CropDHash);
                        sw.Stop();
                        result.TimeMagic += sw.ElapsedMilliseconds;
                    }

                    var file = new FileInfo(filePath);
                    file.Delete();
                }
            }

            return View("Results", result);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
