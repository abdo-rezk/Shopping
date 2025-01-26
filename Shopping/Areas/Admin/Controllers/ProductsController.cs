using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping.Models.Db;

namespace Shopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly OnlineShopContext _context;

        public ProductsController(OnlineShopContext context)
        {
            _context = context;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        // GET: Admin/Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Titel,Description,FullDesc,Price,Discount,ImageName,Qty,Tag,VideoUrl")] Product product,IFormFile? MainImage, IFormFile[]? GalleryImages)
        {
            if (ModelState.IsValid)
            {

                if (MainImage != null)
                {
                    product.ImageName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(MainImage.FileName);
                    string fn;
                    fn = Directory.GetCurrentDirectory();
                    // string ImagePath = fn + "\\wwwroot\\images\\products\\" + product.ImageName;
                    string ImagePath = Path.Combine(fn, "wwwroot", "images", "products", product.ImageName);

                    using (var stream = new FileStream(ImagePath, FileMode.Create))
                    {
                        await MainImage.CopyToAsync(stream);
                    }
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                if (GalleryImages != null)
                {
                    foreach (var item in GalleryImages)
                    {
                        var galleryiamge = new ProductGalery();
                        galleryiamge.ProductId = product.Id;
                        galleryiamge.ImageName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(item.FileName);
                        string fn = Directory.GetCurrentDirectory();
                        // string ImagePath = fn + "\\wwwroot\\images\\products\\" + product.ImageName;
                        string ImagePath = Path.Combine(fn, "wwwroot", "images", "products", galleryiamge.ImageName);

                        using (var stream = new FileStream(ImagePath, FileMode.Create))
                        {
                            await item.CopyToAsync(stream);
                        }
                    _context.ProductGaleries.Add(galleryiamge);
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
                return View(product);
        }

        // GET: Admin/Products/Edit/5
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
            ViewData["gallery"]=_context.ProductGaleries.Where(p => p.ProductId == id).ToList();
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titel,Description,FullDesc,Price,Discount,ImageName,Qty,Tag,VideoUrl")] Product product ,IFormFile? MainImage,IFormFile[]? GalleryImages)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    if (MainImage != null)
                    {
                        //-----------------
                        string org_fn;
                        org_fn = Path.Combine(Directory.GetCurrentDirectory() + "/wwwroot/images/products/" + product.ImageName);

                        if (System.IO.File.Exists(org_fn))
                        {
                            System.IO.File.Delete(org_fn);
                        }
                        //-----------------
                        product.ImageName = Guid.NewGuid() + Path.GetExtension(MainImage.FileName);
                        //-----------------
                        string ImagePath = Directory.GetCurrentDirectory() + "\\wwwroot\\images\\products\\" + product.ImageName;
                        //string ImagePath = Path.Combine(fn , "wwwroot","images","banners" , banner.ImageName);          
                        using (var stream = new FileStream(ImagePath, FileMode.Create))
                        {
                            MainImage.CopyTo(stream);
                        }

                    }
                    if (GalleryImages != null)
                    {
                        foreach (var item in GalleryImages)
                        {

                            var imageName = Guid.NewGuid() + Path.GetExtension(item.FileName);
                            //------------------------------------------------
                            string fn = Directory.GetCurrentDirectory() + "\\wwwroot\\images\\products\\" + imageName;
                            //------------------------------------------------
                            using (var stream = new FileStream(fn, FileMode.Create))
                            {
                                item.CopyTo(stream);
                            }
                            //------------------------------------------------
                            var galleryItem = new ProductGalery();
                            galleryItem.ImageName = imageName;
                            galleryItem.ProductId = product.Id;
                            //------------------------------------------------
                            _context.ProductGaleries.Add(galleryItem);

                        }
                    }
                   
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
            return View(product);
        }
        public async Task<IActionResult> DeleteGallery(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var productGalery = await _context.ProductGaleries
                .FirstOrDefaultAsync(m => m.Id == id);
            string org_fn = Directory.GetCurrentDirectory() + "/wwwroot/images/products/" + productGalery.ImageName;

            if (System.IO.File.Exists(org_fn))
            {
                System.IO.File.Delete(org_fn);
            }
            _context.ProductGaleries.Remove(productGalery);
            _context.SaveChanges();
            
            return RedirectToAction(nameof(Edit),new { Id=productGalery.ProductId});
        }
        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                var frImage = Directory.GetCurrentDirectory() + "/wwwroot/image/products/" + product.ImageName;
                if(System.IO.File.Exists(frImage))
                {
                    System.IO.File.Delete(frImage);
                }
                var objOfGallery=_context.ProductGaleries.Where(p => p.ProductId == id).ToList();
                if (objOfGallery != null)
                {

                    foreach (var item in objOfGallery)
                    {
                        string org_fn = Directory.GetCurrentDirectory() + "/wwwroot/images/products/" + item.ImageName;
                        if (System.IO.File.Exists(org_fn))
                        {
                            System.IO.File.Delete(org_fn);
                        }
                    }
                    _context.ProductGaleries.RemoveRange(objOfGallery);
                }
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
