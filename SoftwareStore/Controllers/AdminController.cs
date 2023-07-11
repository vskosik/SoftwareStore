﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SoftwareStore.Data;
using SoftwareStore.Models;
using SoftwareStore.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SoftwareStore.Controllers
{
    public class AdminController : Controller
    {
        private readonly IRepository<ProductImage> _repository;
        private readonly SoftwareStoreContext _context;

        public AdminController(IRepository<ProductImage> repository, SoftwareStoreContext context)
        {
            this._repository = repository;
            _context = context;
        }
        /*public IActionResult Index()
        {
            return View();
        }*/

        //Upload
        [HttpGet]
        public IActionResult AddImages()
        {
            ViewBag.Products = new SelectList(_context.Products, "Id", "Title");
            return View();
        }

        [HttpPost]
        public IActionResult AddImages(ProductImage prodImage, List<IFormFile> picture)
        {
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
            return View();
        }
    }
}
