using WebGoatCore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebGoatCore.ViewModels;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System;

namespace WebGoatCore.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ProductRepository _productRepository;

        public HomeController(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public IActionResult Index()
        {
            return View(new HomeViewModel()
            {
                TopProducts = _productRepository.GetTopProducts(4)
            });
        }

        [HttpGet]
        public IActionResult About() => View();

        [HttpPost("About")]
        public async Task<IActionResult> UploadFile(IFormFile FormFile)
        {
            ViewBag.Message = "";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload", FormFile.FileName);
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await FormFile.CopyToAsync(fileStream);
                }
                ViewBag.Message = $"File {FormFile.FileName} Uploaded Successfully at /upload";
                return View("About");
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel()
                { ExceptionInfo = (IExceptionHandlerPathFeature)ex });
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
