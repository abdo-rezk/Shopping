using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Models.Db;
using System.Text.RegularExpressions;
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
            ViewBag.Comments = _context.Comments.Where(x => x.ProductId == id).Take(6).OrderByDescending(x=>x.CreateDate).ToList();
            return View(product);
        }
        [HttpPost]
        public IActionResult SubmitComment(string name, string email, string comment, int productId)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(comment) && productId != 0)
            {
                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                Match match = regex.Match(email);
                if (!match.Success)
                {
                    TempData["ErrorMessage"] = "Email is not valid";
                    return Redirect("/Products/ProductDetails/" + productId);
                }

                Comment newComment = new Comment();
                newComment.Name = name;
                newComment.Email = email;
                newComment.CommentText = comment;
                newComment.ProductId = productId;
                newComment.CreateDate = DateTime.Now;

                _context.Comments.Add(newComment);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Youre comment submited success fully";
                return Redirect("/Products/ProductDetails/" + productId);
            }
            else
            {
                TempData["ErrorMessage"] = "Please complete youre information";
                return Redirect("/Products/ProductDetails/" + productId);
            }

        }

    }
}
