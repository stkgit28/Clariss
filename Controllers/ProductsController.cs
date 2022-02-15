using Proiect.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ApplicationDbContext = Proiect.Models.ApplicationDbContext;
using System.IO;
using System.Data.Entity;

namespace Proiect.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new Proiect.Models.ApplicationDbContext();

        // GET: Product
        //[Authorize(Roles = "Admin,Colaborator,User")]
        public ActionResult Index()
        {
            var products = from prod in db.Products.Include("Category").Include("User")
                           where prod.Status == "acceptat"
                           orderby prod.CategoryId
                           select prod;
            ViewBag.Products = products;

            var search = "";
            if (Request.Params.Get("search") != null)
            {
                search = Request.Params.Get("search").Trim();

                List<int> productIds = db.Products.Where(
                    at => at.Title.Contains(search)
                    ).Select(a => a.ProductId).ToList();

                // products = db.Products.Where(product => productIds.Contains(product.ProductId)).Include("category").Include("User");

                products = from prod in db.Products.Include("Category").Include("User")
                           where prod.Status == "acceptat" && productIds.Contains(prod.ProductId)
                           orderby prod.CategoryId
                           select prod;


            }

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            ViewBag.SearchString = search;
            ViewBag.Products = products;

            return View();
        }

        //[Authorize(Roles = "Admin,Colaborator,User")]
        public ActionResult Show(int id)
        {
            var product = db.Products.Find(id);

            SetAccessRights();

            ViewBag.ProductId = id;

            var comments = db.Reviews.Where(d => d.ProductId.Equals(id)).ToList();
            ViewBag.Comments = comments;

            var ratings = db.Reviews.Where(d => d.ProductId.Equals(id)).ToList();
            if (ratings.Count() > 0)
            {
                var ratingSum = ratings.Sum(d => d.Rating);
                ViewBag.RatingSum = ratingSum;
                var ratingCount = ratings.Count();
                ViewBag.RatingCount = ratingCount;
                decimal medie = 0;
                if (ratingCount > 0)
                {
                    medie = ((decimal)ratingSum / (decimal)ratingCount);
                    medie = decimal.Round(medie, 2);
                }
                product.ProductRating = decimal.Round(medie, 2);
                db.SaveChanges();
                ViewBag.Rating = product.ProductRating;
            }
            else
            {
                product.ProductRating = 0;
                db.SaveChanges();
                ViewBag.RatingSum = 0;
                ViewBag.RatingCount = 0;
            }

            return View(product);

        }

        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public ActionResult Show(Review rev)
        {
            rev.Date = DateTime.Now;
            rev.UserId = User.Identity.GetUserId();
            try
            {
                if (ModelState.IsValid)
                {
                    db.Reviews.Add(rev);
                    db.SaveChanges();
                    return Redirect("/Products/Show/" + rev.ProductId);
                }

                else
                {
                    Product a = db.Products.Find(rev.ProductId);

                    SetAccessRights();

                    return View(a);
                }

            }

            catch (Exception e)
            {
                Product a = db.Products.Find(rev.ProductId);

                SetAccessRights();

                return View(a);
            }

        }



        [Authorize(Roles = "Admin,Colaborator")]
        public ActionResult New()
        {
            Product product = new Product();

            // preluam lista de categorii din metoda GetAllCategories()
            product.Categ = GetAllCategories();
            product.UserId = User.Identity.GetUserId();

            return View(product);
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Colaborator")]
        public ActionResult New(Product product, HttpPostedFileBase UploadedFile)
        {
            product.Categ = GetAllCategories();
            product.UserId = User.Identity.GetUserId();

            try
            {
                if (ModelState.IsValid)
                {
                    string uploadedFileName = UploadedFile.FileName;
                    string uploadedFileExtension = Path.GetExtension(uploadedFileName);

                    if (uploadedFileExtension == ".png" || uploadedFileExtension == ".jpeg" || uploadedFileExtension == ".jpg")
                    {
                        string uploadFolderPath = Server.MapPath("~//Images//");
                        UploadedFile.SaveAs(uploadFolderPath + uploadedFileName);

                        product.Photo = "/Images/" + uploadedFileName;

                        if (User.IsInRole("Admin"))
                        {
                            product.Status = "acceptat";
                            TempData["message"] = "Produsul a fost adaugat";
                        }
                        else
                        {
                            product.Status = "in asteptare";
                            TempData["message"] = "Produsul a fost trimis spre evaluare";
                        }

                        db.Products.Add(product);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    product.Categ = GetAllCategories();
                    return View(product);
                }
            }
            catch (Exception e)
            {
                product.Categ = GetAllCategories();
                return View(product);
            }

            product.Categ = GetAllCategories();
            return View(product);
        }


        [Authorize(Roles = "Admin,Colaborator")]
        public ActionResult Edit(int id)
        {
            Product product = db.Products.Find(id);
            product.Categ = GetAllCategories();
            if (product.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                return View(product);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine!";
                return RedirectToAction("Index");
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin,Colaborator")]
        public ActionResult Edit(int id, Product requestProduct, HttpPostedFileBase UploadedFile)
        {
            requestProduct.Categ = GetAllCategories();

            try
            {
                if (ModelState.IsValid)
                {
                    Product product = db.Products.Find(id);

                    if (product.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {

                        if (TryUpdateModel(product))
                        {
                            if (UploadedFile != null)
                            {
                                string uploadedFileName = UploadedFile.FileName;
                                string uploadedFileExtension = Path.GetExtension(uploadedFileName);

                                if (uploadedFileExtension == ".png" || uploadedFileExtension == ".jpeg" || uploadedFileExtension == ".jpg")
                                {
                                    string uploadFolderPath = Server.MapPath("~//Images//");
                                    UploadedFile.SaveAs(uploadFolderPath + uploadedFileName);

                                    product.Photo = "/Images/" + uploadedFileName;
                                }
                            }
                            product.Title = requestProduct.Title;
                            product.Description = requestProduct.Description;
                            product.Price = requestProduct.Price;
                            product.CategoryId = requestProduct.CategoryId;
                            db.SaveChanges();
                            TempData["message"] = "Produsul a fost modificat!";
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            return View(requestProduct);
                        }
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine!";
                        return RedirectToAction("Index");
                    }
                }
                return View(requestProduct);
            }
            catch (Exception e)
            {
                return View(requestProduct);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Admin,Colaborator")]
        public ActionResult Delete(int id)
        {
            Product product = db.Products.Find(id);
            if (product.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                db.Products.Remove(product);
                db.SaveChanges();
                TempData["message"] = "Produsul a fost sters!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un produs care nu va apartine!";
                return RedirectToAction("Index");
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista goala
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryId.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }
            /*
            foreach (var category in categories)
            {
                var listItem = new SelectListItem();
                listItem.Value = category.CategoryId.ToString();
                listItem.Text = category.CategoryName.ToString();

                selectList.Add(listItem);
            }*/

            // returnam lista de categorii
            return selectList;
        }

        private void SetAccessRights()
        {
            ViewBag.afisareButoane = false;
            if (User.IsInRole("Colaborator") || User.IsInRole("Admin"))
            {
                ViewBag.afisareButoane = true;
            }

            ViewBag.esteAdmin = User.IsInRole("Admin");
            ViewBag.utilizatorCurent = User.Identity.GetUserId();
        }

        public ActionResult SortareAsc()
        {
            var products = from prod in db.Products.Include("Category").Include("User")
                           where prod.Status == "acceptat"
                           orderby prod.Price ascending
                           select prod;
            ViewBag.Products = products;
            return View("Index");
        }

        public ActionResult SortareDesc()
        {
            var products = from prod in db.Products.Include("Category").Include("User")
                           where prod.Status == "acceptat"
                           orderby prod.Price descending
                           select prod;
            ViewBag.Products = products;
            return View("Index");
        }

        public ActionResult AscRating()
        {
            var products = from prod in db.Products.Include("Category").Include("User")
                           orderby prod.ProductRating ascending
                           select prod;
            ViewBag.Products = products;
            return View("Index");
        }

        public ActionResult DescRating()
        {
            var products = from prod in db.Products.Include("Category").Include("User")
                           orderby prod.ProductRating descending
                           select prod;
            ViewBag.Products = products;
            return View("Index");
        }
    }
}