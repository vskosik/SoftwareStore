using SoftwareStore.Data;
using SoftwareStore.Models;
using SoftwareStore.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareStore.Controllers
{
    public class UsersController : Controller
    {
        private readonly SoftwareStoreContext _context;
        private const string LoggedUser = "LoggedUser";
        public UsersController(SoftwareStoreContext context)
        {
            this._context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        // AddToCart POST: Users/AddToCart/2
        [HttpPost]
        public IActionResult AddToCart(int? id)
        {
            int? loggegUserId = HttpContext.Session.GetInt32(LoggedUser);

            if (loggegUserId == null)
            {
                return RedirectToAction("Login");
            }

            var cart = _context.Carts.FirstOrDefault(cart => cart.UserId == loggegUserId && cart.ProductId == id);

            if (cart != null)
            {
                cart.Qty++;
                _context.Entry<Cart>(cart).State = EntityState.Modified;
            }
            else
            {
                cart = new Cart { ProductId = (int)id, UserId = (int)loggegUserId, Qty = 1 };
                _context.Carts.Add(cart);
            }
            
            _context.SaveChanges();

            return RedirectToAction("ViewCart", "Users");
        }

        // View Cart GET: Users/ViewCart/3
        [HttpGet]
        public IActionResult ViewCart()
        {
            int? loggegUserId = HttpContext.Session.GetInt32(LoggedUser);

            if (loggegUserId == null)
            {
                return RedirectToAction("Login");
            }

            var carts = _context.Carts.Where(cart => cart.UserId == loggegUserId).ToList();

            List<Product> products = new List<Product>();

            foreach (Cart item in carts)
            {
                var product = _context.Products.Include(p => p.Vendor).
                    Where(p => p.Id == item.ProductId).FirstOrDefault();
                product.Qty = item.Qty;
                products.Add(product);
            }

            return View(products);
        }

        [HttpPost]
        public void ChangeCartAjax(string productId, string Qty)
        {
            int prodId = int.Parse(productId);
            int qty = int.Parse(Qty);
            int? userId = HttpContext.Session.GetInt32(LoggedUser);
            var cart = _context.Carts.Where(c => c.ProductId == prodId &&
                c.UserId == (int)userId).FirstOrDefault();
            cart.Qty = qty;
            _context.Entry<Cart>(cart).State = EntityState.Modified;
            _context.SaveChanges();
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public void Delete(int id)
        {
            int? userId = HttpContext.Session.GetInt32(LoggedUser);
            var cart = _context.Carts.Where(c => c.ProductId == id &&
                c.UserId == (int)userId).FirstOrDefault();
            _context.Carts.Remove(cart);
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
            if (_context.Users.Any(u => u.Email == user.Email))
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
                HttpContext.Session.SetInt32(LoggedUser, userFromDB.Id);
                return RedirectToAction("Index", "Products");
            }

            return RedirectToAction("Login");
        }
    }
}
