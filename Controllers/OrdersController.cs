using Microsoft.AspNet.Identity;
using Proiect.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

// cand lasi rating,stergi produs cos,cand adaugi produs cos

namespace Proiect.Controllers
{
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new Proiect.Models.ApplicationDbContext();
        // GET: Orders

        [Authorize(Roles = "User")]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var products = from item in db.Items.Include("Product")
                           join prod in db.Products on item.ProductId equals prod.ProductId
                           join order in db.Orders on item.OrderId equals order.OrderId
                           where order.UserId == userId
                           select item;
            bool existaComanda = false;
            if (products.Any() )
            {
                existaComanda = true;
            }
            ViewBag.Nada = existaComanda;
            ViewBag.Products = products;
            return View();
        }


        public ActionResult Add(int id)
        {
            var userId = User.Identity.GetUserId();
            var cos = from order in db.Orders
                           where order.UserId == userId
                           select order;
            if (cos.Any())
            {
                var exista = from ex in db.Items
                             join order in db.Orders on ex.OrderId equals order.OrderId
                             where order.UserId == userId && ex.ProductId == id
                             select ex;
                if (exista.Any())
                {
                    var idItm = exista.First().ItemId;
                    var itmm = db.Items.Find(idItm);
                    itmm.Quantity += 1;
                    db.SaveChanges();
                    TempData["message"] = "Cantitatea pentru produsul respectiv a fost marita!!";
                }
                else
                {

                    Item item = new Item { ProductId = id, OrderId = cos.First().OrderId, Quantity = 1 };
                    db.Items.Add(item);
                    db.SaveChanges();
                    TempData["message"] = "Produsul a fost adaugat cu succes!";
                }


            }
            else
            {
                var comanda = new Order { Date = DateTime.Now, UserId = userId };
                db.Orders.Add(comanda);
                Item item = new Item { ProductId = id, OrderId = comanda.OrderId, Quantity = 1 };
                db.Items.Add(item);
                db.SaveChanges();
                TempData["message"] = "Produsul a fost adaugat cu succes!";

            }

            return RedirectToAction("Index");
        }

        public ActionResult Remove(int id)
        {
            var userId = User.Identity.GetUserId();
            var exista = from ex in db.Items
                         join order in db.Orders on ex.OrderId equals order.OrderId
                         where order.UserId == userId && ex.ProductId == id
                         select ex;
            if (exista.Any())
            {
                var idItm = exista.First().ItemId;
                var itmm = db.Items.Find(idItm);
                db.Items.Remove(itmm);
                db.SaveChanges();
                TempData["message"] = "Produsul a fost sters din comanda cu succes!";

            }

            return RedirectToAction("Index");

        }

        public ActionResult Plus(int id)
        {
            var userId = User.Identity.GetUserId();
            var exista = from ex in db.Items
                         join order in db.Orders on ex.OrderId equals order.OrderId
                         where order.UserId == userId && ex.ProductId == id
                         select ex;
            var idItm = exista.First().ItemId;
            var itmm = db.Items.Find(idItm);
            itmm.Quantity += 1;
            db.SaveChanges();
            TempData["message"] = "Cantitatea a fost marita cu succes!";
            return RedirectToAction("Index");

        }
        public ActionResult Minus(int id)
        {
            var userId = User.Identity.GetUserId();
            var exista = from ex in db.Items
                         join order in db.Orders on ex.OrderId equals order.OrderId
                         where order.UserId == userId && ex.ProductId == id
                         select ex;
            var idItm = exista.First().ItemId;
            var itmm = db.Items.Find(idItm);
            itmm.Quantity -= 1;
            db.SaveChanges();
            TempData["message"] = "Cantitatea a fost micsorata cu succes!";

            if (itmm.Quantity == 0)
            { db.Items.Remove(itmm); TempData["message"] = "Produsul a fost sters din comanda!"; }
            db.SaveChanges();

            return RedirectToAction("Index");

        }
    }
}