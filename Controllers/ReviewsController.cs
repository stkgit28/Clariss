using System;
using Proiect.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace Proiect.Controllers
{
    public class ReviewsController : Controller
    {
        private Proiect.Models.ApplicationDbContext db = new Proiect.Models.ApplicationDbContext();

        // GET: Reviews
        public ActionResult Index()
        {
            return View();
        }

        [HttpDelete]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public ActionResult Delete(int id)
        {
            Review rev = db.Reviews.Find(id);
            var ProductId = rev.ProductId;
            if (rev.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                db.Reviews.Remove(rev);
                db.SaveChanges();
                TempData["message"] = "Review-ul a fost sters!";
                return RedirectToAction("Show", "Products", new { id = ProductId });
                //return Redirect("/Products/Show/" + rev.ProductId); 
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                return RedirectToAction("Index", "Products");
            }

        }


        [Authorize(Roles = "User,Colaborator,Admin")]
        public ActionResult Edit(int id)
        {
            Review rev = db.Reviews.Find(id);
            ViewBag.ReviewId = id;
            if (rev.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                return View(rev);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                return RedirectToAction("Index", "Products");
            }

        }

        [HttpPut]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public ActionResult Edit(FormCollection form)
        {
            try
            {
                var idUser = User.Identity.GetUserId();
                var ReviewId = int.Parse(form["ReviewId"]);
                Review rev = db.Reviews.Find(ReviewId);
                var rating = int.Parse(form["Rating"]);
                var comment = form["Content"].ToString();


                if (idUser == User.Identity.GetUserId() || User.IsInRole("Admin"))
                {

                    if (TryUpdateModel(rev))
                    {

                        rev.Content = comment;
                        rev.Rating = rating;

                        db.SaveChanges();
                        TempData["message"] = "Review-ul a fost editat cu succes!";
                    }
                    return Redirect("/Products/Show/" + rev.ProductId);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                    return RedirectToAction("Index", "Products");
                }
            }
            catch (Exception e)
            {
                return View();
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Add(FormCollection form)
        {
            var idUser = User.Identity.GetUserId();

            var comment = form["Content"].ToString();
            var ProductId = int.Parse(form["ProductId"]);
            var rating = int.Parse(form["Rating"]);
            var review = from rev in db.Reviews
                         where (rev.UserId == idUser) && (rev.ProductId == ProductId)
                         select rev;
            if (review.Any())
            { TempData["message"] = "Nu aveti voie sa adaugati mai mult de un review"; }
            else
            {

                Review artComment = new Review()
                {
                    ProductId = ProductId,
                    Content = comment,
                    Rating = rating,
                    Date = DateTime.Now,
                    UserId = User.Identity.GetUserId()
                };

                db.Reviews.Add(artComment);
                db.SaveChanges();
                TempData["message"] = "Review-ul a fost adaugat";
            }

            return RedirectToAction("Show", "Products", new { id = ProductId });


        }


    }
}