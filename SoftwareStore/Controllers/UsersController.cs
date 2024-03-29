﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoftwareStore.Models;
using SoftwareStore.Repository;

namespace SoftwareStore.Controllers
{
    public class UsersController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly IHistoryRepository _historyRepository;
        private readonly ILogger<UsersController> _logger;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;

        public UsersController(ICartRepository cartRepository, IProductRepository productRepository,
            IUserRepository userRepository, IHistoryRepository historyRepository, ILogger<UsersController> logger)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _historyRepository = historyRepository;
            _logger = logger;
        }

        // AddToCart POST: Users/AddToCart/2
        [HttpPost]
        public async Task<IActionResult> AddToCart(int? id)
        {
            var loggedUserId = HttpContext.Session.GetInt32("LoggedUser");

            if (loggedUserId == null)
            {
                _logger.LogInformation("User try to AddToCart without logging in");
                return RedirectToAction("Login");
            }

            if (id == null)
            {
                return RedirectToAction("ViewCart", "Users");
            }

            var cart = await _cartRepository.Find((int)loggedUserId, (int)id);

            if (cart != null)
            {
                cart.Qty++;
                await _cartRepository.UpdateAsync(cart.Id, cart);
                _logger.LogInformation("Product(id: {ProductId}) qty in cart(id: {CartId}) changed", cart.ProductId, cart.Id);
            }
            else
            {
                cart = new Cart { ProductId = (int)id, UserId = (int)loggedUserId, Qty = 1 };
                await _cartRepository.AddAsync(cart);
                _logger.LogInformation("Product(id: {Id}) added to cart", (int)id);
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
                _logger.LogInformation("User try to ViewCart without logging in");
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
        public async Task<IActionResult> ChangeCartAjax(string productId, string qty)
        {
            if (!int.TryParse(productId, out var prodId) || !int.TryParse(qty, out var quantity))
            {
                _logger.LogError("Invalid product ID or quantity");
                return BadRequest("Invalid product ID or quantity.");
            }

            var userId = HttpContext.Session.GetInt32("LoggedUser");
            if (userId == null)
            {
                _logger.LogInformation("User try to ChangeQty without logging in");
                return RedirectToAction("Login");
            }

            var cart = await _cartRepository.Find((int)userId, prodId);
            if (cart == null)
            {
                _logger.LogError("Cart item not found");
                return NotFound("Cart item not found.");
            }

            if (quantity == 0)
            {
                _logger.LogInformation("Cart(id: {CartId}) is about to be deleted", cart.Id);
                await _cartRepository.DeleteAsync(cart.Id);
                _logger.LogInformation("Cart deleted");
            }
            else
            {
                cart.Qty = quantity;
                await _cartRepository.UpdateAsync(cart.Id, cart);
                _logger.LogInformation("Item(id: {ProductId} qty in cart(id: {CartId}) changed", cart.ProductId, cart.Id);
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
                _logger.LogInformation("User try to DeleteItem without logging in");
                return RedirectToAction("Login");
            }

            var cart = await _cartRepository.Find((int)userId, id);
            _logger.LogInformation("Cart(id: {CartId}) is about to be deleted", cart.Id);
            await _cartRepository.DeleteAsync(cart.Id);
            _logger.LogInformation("Cart deleted");

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
                _logger.LogInformation("User didn't enter all fields");
                return RedirectToAction("Register");
            }

            if (await _userRepository.IsExist(user.Email))
            {
                _logger.LogInformation("User already exist");
                return RedirectToAction("Register");
            }

            user.Password = GetHash(user.Password);
            await _userRepository.AddAsync(user);
            _logger.LogInformation("User(email: {UserEmail}) added to database", user.Email);
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
                _logger.LogInformation("User doesn't exist");
                return RedirectToAction("Register");
            }

            if (GetHash(user.Password) != userFromDb.Password)
            {
                _logger.LogInformation("Incorrect password");
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetInt32("LoggedUser", userFromDb.Id);
            HttpContext.Session.SetString("UserRole", userFromDb.Role);
            HttpContext.Session.SetString("UserEmail", userFromDb.Email);
            _logger.LogInformation("User(id: {UserId}) logged in", userFromDb.Id);
            return RedirectToAction("Index", "Products");
        }

        // GET: Users/Logout/
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            _logger.LogInformation("Session cleared");
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
                _logger.LogInformation("User is not an admin");
                return RedirectToAction("Login");
            }

            var users = await _userRepository.GetAllAsync();
            return View(users);
        }

        // GET: Users/Edit/2
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || (!IsAdmin() && !IsLogged((int)id)))
            {
                _logger.LogInformation("Access restricted for editing user");
                return RedirectToAction("Login");
            }

            var user = await _userRepository.GetByIdAsync((int)id);
            if (user != null)
            {
                return View(user);
            }

            _logger.LogError("User not found");
            return NotFound();
        }

        // POST: Users/Edit/2
        // To protect from over posting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,FirstName,LastName,Email,Password,Role")]
            User user)
        {
            if (!IsAdmin() && !IsLogged(id))
            {
                _logger.LogInformation("Access restricted for editing user(id: {UserId})", id);
                return RedirectToAction("Login");
            }

            if (id != user.Id)
            {
                _logger.LogError("User not found");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid Model");
                return View(user);
            }

            try
            {
                _logger.LogDebug("Updating user(id: {UserId})", user.Id);
                await _userRepository.UpdateAsync(user.Id, user);
            }
            catch (DbUpdateConcurrencyException dbException)
            {
                if (!await _userRepository.IsExist(user.Email))
                {
                    _logger.LogError("User doesn't exist");
                    return NotFound();
                }

                _logger.LogError(dbException, "Something went wrong");
                throw;
            }

            _logger.LogInformation("User(id: {UserId}) updated", user.Id);
            return RedirectToAction(IsLogged(id) ? "ViewProfile" : "AllUsers");
        }

        // GET: Users/Delete/2
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || (!IsAdmin() && !IsLogged((int)id)))
            {
                _logger.LogInformation("Access restricted for deleting user");
                return RedirectToAction("Login");
            }

            var user = await _userRepository.GetByIdAsync((int)id);

            if (user != null)
            {
                return View(user);
            }

            _logger.LogInformation("User doesn't exist");
            return NotFound();
        }

        // POST: Users/Delete/2
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin() && !IsLogged(id))
            {
                _logger.LogInformation("Access restricted for deleting user(id: {Id})", id);
                return RedirectToAction("Login");
            }

            _logger.LogInformation("User(id: {Id}) is about to be deleted", id);
            await _userRepository.DeleteAsync(id);
            _logger.LogInformation("User deleted");

            if (IsAdmin())
            {
                return RedirectToAction("AllUsers");
            }

            HttpContext.Session.Clear();
            _logger.LogInformation("Session cleared");
            return RedirectToAction("Login");
        }

        // GET: Users/ViewProfile/
        [HttpGet]
        public async Task<IActionResult> ViewProfile()
        {
            var loggedUserId = HttpContext.Session.GetInt32("LoggedUser");

            if (loggedUserId == null)
            {
                _logger.LogInformation("User try to ViewProfile without logging in");
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
                _logger.LogInformation("User try to view History without logging in");
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
                _logger.LogInformation("User try to proceed to Checkout without logging in");
                return RedirectToAction("Login");
            }

            var carts = _cartRepository.Find((int)loggedUserId).ToList();

            foreach (var history in carts.Select(async cart => new History
                     {
                         ProductId = cart.ProductId,
                         UserId = cart.UserId,
                         Price = (await _productRepository.GetByIdAsync(cart.ProductId)).Price,
                         Qty = cart.Qty,
                         PurchaseDate = DateTime.Now
                     }))
            {
                await _historyRepository.AddAsync(await history);
                _logger.LogInformation("User(id: {UserId}) complete purchase(product id: {ProductId})", (await history).UserId, (await history).ProductId);
            }

            foreach (var cart in carts)
            {
                _logger.LogInformation("Cart(id: {CartId}) is about to be deleted", cart.Id);
                await _cartRepository.DeleteAsync(cart.Id);
                _logger.LogInformation("Cart deleted");
            }

            return RedirectToAction("History");
        }
    }
}