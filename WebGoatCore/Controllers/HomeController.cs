﻿using WebGoatCore.Data;
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
using Microsoft.Extensions.Logging;

namespace WebGoatCore.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ProductRepository _productRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger _logger;

        public HomeController(IWebHostEnvironment webHostEnvironment, ProductRepository productRepository, ILogger<HomeController> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _productRepository = productRepository;
            _logger = logger;
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
                // Create a temporary filename with .txt extension
                string newFilename = $"{Path.GetRandomFileName()}{Guid.NewGuid()}.txt"; ;
                string tempFolderPath = GetTemporaryDirectory();

                // Generate a path with the filename
                string path = Path.Combine(tempFolderPath, newFilename);

                // Copy the contents of the file to the new location
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await FormFile.CopyToAsync(fileStream);
                }

                // Read the contents of the copied file
                string? content;
                using (StreamReader sr = new StreamReader(path))
                {
                    content = sr.ReadToEnd();
                }
                
                // Check whether the content value is present or not
                if (content != null && content.Length > 0)
                {
                    // Verify whether the content has unicode characters or not
                    bool hasUnicode = System.Text.Encoding.UTF8.GetByteCount(content) != content.Length;

                    // Throw error if the contents of the file contains unicode characters.
                    if (hasUnicode == true)
                    {
                        throw new InvalidDataException("The given brochure contains some invalid characters. Please remove them.");
                    }

                    // Clean up resources
                    System.IO.File.Delete(path);
                    Directory.Delete(tempFolderPath, true);

                    ViewBag.Message = "Successfully uploaded your feedback! Thank you!";
                }
            }
            catch (Exception ex)
            {
                string message = $"Error occurred while reading the file. {ex.Message}";
                _logger.LogError(ex, message);
                ViewBag.Message = $"File processing failed: {ex.Message}";
            }
            return View("About");
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

        // Utility method to generate a temporary directory
        private string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}
