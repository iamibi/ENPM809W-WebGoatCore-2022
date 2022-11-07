using WebGoatCore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebGoatCore.ViewModels;
using Microsoft.AspNetCore.Diagnostics;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace WebGoatCore.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ProductRepository _productRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(IWebHostEnvironment webHostEnvironment, ProductRepository productRepository)
        {
            _webHostEnvironment = webHostEnvironment;
            _productRepository = productRepository;
        }

        public IActionResult Index()
        {
            return View(new HomeViewModel()
            {
                TopProducts = _productRepository.GetTopProducts(4)
            });
        }

        public IActionResult About() => View();

        [HttpPost("About")]
        public async Task<IActionResult> UploadFile(IFormFile FormFile)
        {
            ViewBag.Message = "";
            try
            {
                string filename = Path.GetFileName(FormFile.FileName);
                string webRootPath = _webHostEnvironment.WebRootPath;
                string path = Path.Combine(webRootPath, "upload", filename);
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await FormFile.CopyToAsync(fileStream);
                }
                int firstNum = 0;
                int secondNum = 0;
                using (StreamReader sr = new StreamReader(path))
                {
                    firstNum = int.Parse(sr.ReadLine());
                    secondNum = int.Parse(sr.ReadLine());
                }
                int sum = firstNum + secondNum;
                System.IO.File.Delete(path);
                ViewBag.Message = $"<div class='success' style='text-align:center'>The sum is {sum} </div>";
                return View("About");
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"<div class='error' style='text-align:center'>File processing failed: {ex.Message} </div>";
                return View("About");
            }
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Admin() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ExceptionInfo = HttpContext.Features.Get<IExceptionHandlerPathFeature>(),
            });
        }
    }
}
