using StoreHouse.Models;
using System.Linq;
using System.Web.Mvc;


namespace StoreHouse.Controllers
{
    public class ProductController : Controller
    {       
        private StorageContext db = new StorageContext();

        public ActionResult Add() 
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add([Bind(Include = "ProductID,Name,UnitPrice,Description,Mass")] Product products)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(products);
                Stock stock = new Stock
                {
                    ProductID = products.ProductID,
                    Quantity = 0,
                };

                db.Stocks.Add(stock);
                db.SaveChanges();
                return RedirectToAction("Index", "Stock");
            }
            else
            {
                return View("Error");
            }          
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }
            Product products = db.Products.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }

            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,Name,UnitPrice,Description,Mass")] Product products)
        {
            int id = products.ProductID;
            var product = db.Products.Where(d => d.ProductID == id).First();
            product.Name = products.Name;
            product.Description = products.Description;
            product.UnitPrice = products.UnitPrice;
            product.Mass = products.Mass;
            db.SaveChangesAsync();
            return RedirectToAction("Index", "Stock");
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }
            else
            {
                Product products = db.Products.Find(id);
                db.Products.Remove(products);
                db.SaveChanges();
                return RedirectToAction("Index", "Stock");
            }
        }

        public ActionResult Statistics() 
        {
            using (db)
            {
                if (db.Products.Count() > 0 && db.Stocks.Count() > 0)
                {
                    ViewBag.TotalValue = db.Products.Sum(item => item.UnitPrice);
                    ViewBag.TotalMass = db.Products.Sum(item => item.Mass);

                    var ProductWithMaxQuantity = db.Stocks
                        .Join(db.Products, x => x.ProductID, y => y.ProductID, (x, y) => new { y.Name, x.Quantity })
                        .Where(s => s.Quantity == db.Stocks.Max(item => item.Quantity));

                    ViewBag.MaxQuantity = string.Join(", ", ProductWithMaxQuantity.Select(x => x.Name));
                    ViewBag.MaxMass = db.Products.Max(item => item.Mass);
                }
                else
                {
                    ViewBag.TotalValue = 0;
                    ViewBag.TotalMass = 0;
                    ViewBag.MaxQuantity = "-";
                    ViewBag.MaxMass = 0;
                }

                return View();
            }
        }
    }
} 