using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SoftwareStore.Data;
using SoftwareStore.Models;
using SoftwareStore.Repository;

namespace SoftwareStore.Controllers
{
    public class AdminController : Controller
    {
        private readonly SoftwareStoreContext _context;
        private readonly IRepository<ProductImage> _productImageRepository;

        public AdminController(IRepository<ProductImage> productImageRepository, SoftwareStoreContext context)
        {
            _productImageRepository = productImageRepository;
            _context = context;
        }

        // GET: Admin/AddImage/
        [HttpGet]
        public IActionResult AddImages()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Users");
            }

            ViewBag.Products = new SelectList(_context.Products, "Id", "Title");
            return View();
        }

        // POST: Admin/AddImage/{Pictures}
        [HttpPost]
        public async Task<IActionResult> AddImages(ProductImage prodImage, List<IFormFile> picture)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Users");
            }

            if (picture == null)
            {
                return View();
            }

            foreach (var item in picture)
            {
                using var ms = new MemoryStream();
                await item.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var productImage = new ProductImage { ProductId = prodImage.ProductId, Picture = ms.ToArray() };
                await _productImageRepository.AddAsync(productImage);
            }

            return RedirectToAction("Details", "Products", new { Id = prodImage.ProductId });
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }
    }
}