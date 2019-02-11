using StoreHouse.Models;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;

namespace StoreHouse.Controllers
{
    public class StockController : Controller
    {
        private StorageContext db = new StorageContext();

        public ViewResult Index()
        {
            var stocks = db.Stocks.Include(x => x.Product);
            var stockList = stocks.ToList();
            double currency = GetExchangeRate();
            foreach (var item in stockList)
            {
                item.Product.UnitPrice = Math.Round(item.Product.UnitPrice / currency, 3);
            }
            return View(stockList);
        }

        [HttpPost]
        public ActionResult Modify(string Quantity, int? id, string submitButton)
        {
            if (!ModelState.IsValid)
            {
                return View("Error");
            }

            var itemToModify = db.Stocks.Where(d => d.ProductID == id).First();
            bool result = Int32.TryParse(Quantity, out int newQuantity);

            switch (submitButton)
            {
                case "Add":
                    itemToModify.Quantity += newQuantity;
                    break;
                case "Remove":
                    if (itemToModify.Quantity - newQuantity >= 0)
                        itemToModify.Quantity -= newQuantity;
                    else
                        itemToModify.Quantity = 0;
                    break;
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public double GetExchangeRate()
        {
            try
            {
                MNB.MNBArfolyamServiceSoapImpl client = new MNB.MNBArfolyamServiceSoapImpl();
                MNB.GetCurrentExchangeRatesRequestBody GCERRB = new MNB.GetCurrentExchangeRatesRequestBody();
                MNB.GetCurrentExchangeRatesResponseBody current = client.GetCurrentExchangeRates(GCERRB);

                string result = string.Empty;

                XDocument xdoc = XDocument.Parse(current.GetCurrentExchangeRatesResult);
                var rates = xdoc.Descendants("Rate");
           
                var euroRate = rates.Where(rate => rate.Attribute("curr").Value.Equals("EUR")).First();
                result = euroRate.Value.Substring(0, euroRate.Value.Length - 1);
                
                return double.Parse(result, CultureInfo.GetCultureInfo("hu-HU"));
            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't get euro via webservice.", ex);
            }
        }
    }
}