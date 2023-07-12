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
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SoftwareStore.Controllers
{
    public class UsersController : Controller
    {
        private readonly CartRepository _cartRepository;
        private readonly ProductRepository _productRepository;
        private readonly UserRepository _userRepository;
        private const string LoggedUser = "LoggedUser";

        public UsersController(CartRepository cartRepository, ProductRepository productRepository,
            UserRepository userRepository)
        {
            this._cartRepository = cartRepository;
            this._productRepository = productRepository;
            this._userRepository = userRepository;
        }
        /*public IActionResult Index()
        {
            return View();
        }*/

        // AddToCart POST: Users/AddToCart/2
        [HttpPost]
        public async Task<IActionResult> AddToCart(int? id)
        {
            var loggedUserId = HttpContext.Session.GetInt32(LoggedUser);

            if (loggedUserId == null)
            {
                return RedirectToAction("Login");
            }

            var cart = _cartRepository.Find((int)loggedUserId, (int)id).Result;

            if (cart != null)
            {
                cart.Qty++;
                await _cartRepository.UpdateAsync(cart.Id, cart);
            }
            else
            {
                cart = new Cart { ProductId = (int)id, UserId = (int)loggedUserId, Qty = 1 };
                await _cartRepository.AddAsync(cart);
            }

            return RedirectToAction("ViewCart", "Users");
        }

        // View Cart GET: Users/ViewCart/
        [HttpGet]
        public async Task<IActionResult> ViewCart()
        {
            var loggedUserId = HttpContext.Session.GetInt32(LoggedUser);

            if (loggedUserId == null)
            {
                return RedirectToAction("Login");
            }

            var carts = _cartRepository.Find((int)loggedUserId).ToList();

            var products = new List<Product>();

            foreach (var item in carts)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                product.Qty = item.Qty;
                products.Add(product);
            }

            return View(products);
        }

        // POST: Change Item Qty In Runtime
        [HttpPost]
        public async Task<IActionResult> ChangeCartAjax(string productId, string Qty)
        {
            if (!int.TryParse(productId, out var prodId) || !int.TryParse(Qty, out var qty))
            {
                return BadRequest("Invalid product ID or quantity."); // Add appropriate error handling here
            }

            var userId = HttpContext.Session.GetInt32(LoggedUser);
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var cart = await _cartRepository.Find((int)userId, prodId);
            if (cart == null)
            {
                return NotFound("Cart item not found."); // Add appropriate error handling here
            }

            if (qty == 0)
            {
                await _cartRepository.DeleteAsync(cart.Id);
            }
            else
            {
                cart.Qty = qty;
                await _cartRepository.UpdateAsync(cart.Id, cart);
            }

            return RedirectToAction("ViewCart");
        }

        // POST: Users/DeleteItem/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var userId = HttpContext.Session.GetInt32(LoggedUser);

            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var cart = _cartRepository.Find((int)userId, id).Result;
            await _cartRepository.DeleteAsync(cart.Id);

            return RedirectToAction("ViewCart");
        }

        // GET: Users/Register/
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST Users/Register/{User}
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (user == null)
            {
                return RedirectToAction("Register");
            }
            if (_userRepository.IsExist(user.Email).Result)
            {
                return RedirectToAction("Register");
            }

            user.Password = GetHash(user.Password);
            await _userRepository.AddAsync(user);
            return RedirectToAction("Index", "Products");
        }

        // Get Password Hash
        public string GetHash(string input)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hash);
        }

        // GET: Users/Login/
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Users/Login/{User}
        [HttpPost]
        public IActionResult Login(User user)
        {
            var userFromDb = _userRepository.Find(user.Email).Result;
            if (userFromDb == null)
            {
                return RedirectToAction("Register");
            }

            if (GetHash(user.Password) != userFromDb.Password)
            {
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetInt32(LoggedUser, userFromDb.Id);
            return RedirectToAction("Index", "Products");
        }
    }
}
