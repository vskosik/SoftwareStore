﻿using System.Collections.Generic;
using System.IO;
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
        private readonly IRepository<ProductImage> _repository;

        public AdminController(IRepository<ProductImage> repository, SoftwareStoreContext context)
        {
            _repository = repository;
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
        public IActionResult AddImages(ProductImage prodImage, List<IFormFile> picture)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Users");
            }

            if (picture == null)
            {
                return View();
            }

            var list = new List<ProductImage>();
            foreach (var item in picture)
            {
                using var ms = new MemoryStream();
                item.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var productImage = new ProductImage { ProductId = prodImage.ProductId, Picture = ms.ToArray() };
                list.Add(productImage);
            }

            _context.ProductImages.AddRange(list);
            _context.SaveChanges();
            return RedirectToAction("Details", "Products", new { Id = prodImage.ProductId });
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }
    }
}