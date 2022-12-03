using Microsoft.AspNetCore.Http;
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
        private readonly IRepository<ProductImage> repository;
        private readonly SoftwareStoreContext _context;
        private const string LOGGED_USER = "LoggedUser";

        public AdminController(IRepository<ProductImage> repository, SoftwareStoreContext context)
        {
            this.repository = repository;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        //Upload
        [HttpGet]
        public IActionResult AddImages()
        {
            ViewBag.Products = new SelectList(_context.Products, "Id", "Title");
            return View();
        }

        [HttpPost]
        public IActionResult AddImages(ProductImage pimage, List<IFormFile> Picture)
        {
            if (Picture == null)
            {
                return View();
            }
            var list = new List<ProductImage>();
            foreach (var item in Picture)
            {
                using var ms = new MemoryStream();
                item.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var productImage = new ProductImage { ProductId = pimage.ProductId, Picture = ms.ToArray() };
                list.Add(productImage);
            }
            _context.ProductImages.AddRange(list);
            _context.SaveChanges();
            return View();
        }

        // Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (user == null)
            {
                return RedirectToAction("Register");
            }
            user.Password = GetHash(user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("Index", "Products");
        }

        public string GetHash(string input)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hash);
        }

        // Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(User user)
        {
            var userFromDB = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (userFromDB == null)
            {
                return RedirectToAction("Register");
            }
            if (GetHash(user.Password) == userFromDB.Password)
            {
                HttpContext.Session.SetInt32(LOGGED_USER, userFromDB.Id);
                return RedirectToAction("Index", "Products");
            }
            return RedirectToAction("Login");
        }
    }
}
