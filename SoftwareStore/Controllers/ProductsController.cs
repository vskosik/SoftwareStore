using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SoftwareStore.Data;
using SoftwareStore.Models;

namespace SoftwareStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly SoftwareStoreContext _context;
        private const string LOGGED_USER = "LoggedUser";

        public ProductsController(SoftwareStoreContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var softwareStoreContext = _context.Products.Include(p => p.Vendor);
            return View(await softwareStoreContext.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["VendorId"] = new SelectList(_context.Set<Vendor>(), "Id", "Id");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Rate,Price,VendorId,State")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["VendorId"] = new SelectList(_context.Set<Vendor>(), "Id", "Id", product.VendorId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["VendorId"] = new SelectList(_context.Set<Vendor>(), "Id", "Id", product.VendorId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Rate,Price,VendorId,State")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["VendorId"] = new SelectList(_context.Set<Vendor>(), "Id", "Id", product.VendorId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
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
			return RedirectToAction("Index");
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
                return RedirectToAction("Index");
            }
            return RedirectToAction("Login");
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
    }
}
