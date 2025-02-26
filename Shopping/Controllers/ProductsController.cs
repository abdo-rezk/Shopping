using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Models.Db;
using X.PagedList.Extensions;
namespace Shopping.Controllers
{
    public class ProductsController : Controller
    {
        private readonly OnlineShopContext _context;
        public ProductsController(OnlineShopContext context)
        {
            _context = context;
        }
        public IActionResult Index(int? page)
        {
            var products = _context.Products.OrderByDescending(x => x.Id).ToList();
            return View(products.ToPagedList(page ?? 1, 3));  // begin from page 1 and show 3 product per page
        }
        public IActionResult Search(string searchtext, int? page)
        {
            if (searchtext != null)
            {
                searchtext = searchtext.Trim();
            }
            var products = _context.Products.Where(x => EF.Functions.Like(x.Titel, $"%{searchtext}%") || searchtext == null ||
            (EF.Functions.Like(x.Tag, $"%{searchtext}%"))).OrderByDescending(x => x.Id).ToList();
            ViewBag.searchtext = products;
            return View("Index", products.ToPagedList(page ?? 1, 3));
        }
        public IActionResult ProductDetails(int id)
        {
            var product = _context.Products.FirstOrDefault(x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.gallery = _context.ProductGaleries.Where(x => x.ProductId == id).ToList();
            ViewBag.NewProducts = _context.Products.Where(x => x.Id != id).Take(6).OrderByDescending(x=>x.Id).ToList();

            return View(product);
        }
    }
}
