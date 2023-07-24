using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoftwareStore.Data;
using SoftwareStore.Models;
using SoftwareStore.Repository;

namespace SoftwareStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly SoftwareStoreContext _context;
        private readonly IRepository<ProductImage> _productImageRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductRepository productRepository, SoftwareStoreContext context,
            IRepository<ProductImage> productImageRepository, ILogger<ProductsController> logger)
        {
            _productRepository = productRepository;
            _context = context;
            _productImageRepository = productImageRepository;
            _logger = logger;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var softwareStoreContext = await _productRepository.GetAllAsync();
            ViewBag.Thumbnails = await _productImageRepository.GetAllAsync();
            return View(softwareStoreContext);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productRepository.GetByIdAsync((int)id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Images = _context.ProductImages.Where(p => p.ProductId == id).ToList();

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Users");
            }

            ViewData["VendorId"] = new SelectList(_context.Set<Vendor>(), "Id", "VendorName");
            return View();
        }
        
        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Title,Description,Rate,Price,VendorId,Qty,State")]
            Product product)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Users");
            }

            if (ModelState.IsValid)
            {
                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }

            ViewData["VendorId"] = new SelectList(_context.Set<Vendor>(), "Id", "VendorName", product.VendorId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Users");
            }

            if (id == null)
            {
                return NotFound();
            }

            var product = await _productRepository.GetByIdAsync((int)id);
            if (product == null)
            {
                return NotFound();
            }

            ViewData["VendorId"] = new SelectList(_context.Set<Vendor>(), "Id", "VendorName", product.VendorId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Title,Description,Rate,Price,VendorId,Qty,State")]
            Product product)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Users");
            }

            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _productRepository.UpdateAsync(product.Id, product);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _productRepository.IsExist(id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["VendorId"] = new SelectList(_context.Set<Vendor>(), "Id", "VendorName", product.VendorId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Users");
            }

            if (id == null)
            {
                return NotFound();
            }

            var product = await _productRepository.GetByIdAsync((int)id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Users");
            }

            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }
    }
}