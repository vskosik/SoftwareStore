using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SoftwareStore.Models;
using SoftwareStore.Repository;

namespace SoftwareStore.Controllers
{
    public class UsersController : Controller
    {
        private readonly CartRepository _cartRepository;
        private readonly ProductRepository _productRepository;
        private readonly UserRepository _userRepository;
        private readonly HistoryRepository _historyRepository;

        public UsersController(CartRepository cartRepository, ProductRepository productRepository,
            UserRepository userRepository, HistoryRepository historyRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _historyRepository = historyRepository;
        }
        /*public IActionResult Index()
        {
            return View();
        }*/

        // AddToCart POST: Users/AddToCart/2
        [HttpPost]
        public async Task<IActionResult> AddToCart(int? id)
        {
            var loggedUserId = HttpContext.Session.GetInt32("LoggedUser");

            if (loggedUserId == null)
            {
                return RedirectToAction("Login");
            }

            var cart = await _cartRepository.Find((int)loggedUserId, (int)id);

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
            var loggedUserId = HttpContext.Session.GetInt32("LoggedUser");

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
                return BadRequest("Invalid product ID or quantity.");
            }

            var userId = HttpContext.Session.GetInt32("LoggedUser");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var cart = await _cartRepository.Find((int)userId, prodId);
            if (cart == null)
            {
                return NotFound("Cart item not found.");
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
            var userId = HttpContext.Session.GetInt32("LoggedUser");

            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var cart = await _cartRepository.Find((int)userId, id);
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

            if (await _userRepository.IsExist(user.Email))
            {
                return RedirectToAction("Register");
            }

            user.Password = GetHash(user.Password);
            await _userRepository.AddAsync(user);
            HttpContext.Session.SetInt32("LoggedUser", user.Id);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserEmail", user.Email);
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
        public async Task<IActionResult> Login(User user)
        {
            var userFromDb = await _userRepository.Find(user.Email);
            if (userFromDb == null)
            {
                return RedirectToAction("Register");
            }

            if (GetHash(user.Password) != userFromDb.Password)
            {
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetInt32("LoggedUser", userFromDb.Id);
            HttpContext.Session.SetString("UserRole", userFromDb.Role);
            HttpContext.Session.SetString("UserEmail", userFromDb.Email);
            return RedirectToAction("Index", "Products");
        }

        // GET: Users/Logout/
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Products");
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        private bool IsLogged(int id)
        {
            return HttpContext.Session.GetInt32("LoggedUser") == id;
        }

        // GET: Users/AllUsers/
        [HttpGet]
        public async Task<IActionResult> AllUsers()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login");
            }

            var users = await _userRepository.GetAllAsync();
            return View(users);
        }

        // GET: Users/Edit/2
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin() && !IsLogged((int)id))
            {
                return RedirectToAction("Login");
            }

            var user = await _userRepository.GetByIdAsync((int)id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Edit/2
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,FirstName,LastName,Email,Password,Role")]
            User user)
        {
            if (!IsAdmin() && !IsLogged(id))
            {
                return RedirectToAction("Login");
            }

            if (id != user.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            try
            {
                await _userRepository.UpdateAsync(user.Id, user);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _userRepository.IsExist(user.Email))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(IsLogged(id) ? "ViewProfile" : "AllUsers");
        }

        // GET: Users/Delete/2
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin() && !IsLogged((int)id))
            {
                return RedirectToAction("Login");
            }

            var user = await _userRepository.GetByIdAsync((int)id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/2
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin() && !IsLogged(id))
            {
                return RedirectToAction("Login");
            }

            await _userRepository.DeleteAsync(id);

            if (IsAdmin())
            {
                return RedirectToAction("AllUsers");
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login");

        }

        // GET: Users/ViewProfile/
        [HttpGet]
        public async Task<IActionResult> ViewProfile()
        {
            var loggedUserId = HttpContext.Session.GetInt32("LoggedUser");

            if (loggedUserId == null)
            {
                return RedirectToAction("Login");
            }

            var user = await _userRepository.GetByIdAsync((int)loggedUserId);

            return View(user);
        }

        // GET: Users/History/
        [HttpGet]
        public IActionResult History()
        {
            var loggedUserId = HttpContext.Session.GetInt32("LoggedUser");

            if (loggedUserId == null)
            {
                return RedirectToAction("Login");
            }

            var history = _historyRepository.Find((int)loggedUserId).ToList();

            return View(history);
        }

        // POST: Users/Checkout/
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var loggedUserId = HttpContext.Session.GetInt32("LoggedUser");

            if (loggedUserId == null)
            {
                return RedirectToAction("Login");
            }

            var carts = _cartRepository.Find((int)loggedUserId).ToList();

            foreach (var history in carts.Select(async cart => new History()
            {
                ProductId = cart.ProductId,
                UserId = cart.UserId,
                Price = (await _productRepository.GetByIdAsync(cart.ProductId)).Price,
                Qty = cart.Qty,
                PurchaseDate = DateTime.Now
            }))
            {
                await _historyRepository.AddAsync(await history);
            }

            foreach (var cart in carts)
            {
                await _cartRepository.DeleteAsync(cart.Id);
            }

            return RedirectToAction("History");
        }
    }
}