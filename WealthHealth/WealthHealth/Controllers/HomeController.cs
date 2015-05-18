using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WealthHealth.Models;
using Microsoft.AspNet.Identity;

namespace WealthHealth.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                return RedirectToAction("Household", "Home", new { Id = user.HouseholdId });
            }
            else
            {
                return View();
            }
        }

        public ActionResult Household(string Id)
        {
            var house = db.Households.FirstOrDefault(h => h.HouseId == Id);
            return View(house);
        }

        public ActionResult InviteUser(int id)
        {
            if (db.Households.FirstOrDefault(h => h.Id == id).Keyword == null)
            {
                return RedirectToAction("SetKeyword", id);
            }
            var model = new InviteViewModel();
            model.Id = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InviteUser(InviteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please fill out the entire form";
                return View(model);
            }
            var house = db.Households.FirstOrDefault(h => h.Id == model.Id);
            var sender = db.Users.FirstOrDefault(u => u.Id == model.Sender);
            var callbackUrl = Url.Action("Register", "Home", house.HouseId);
            new EmailService().SendAsync(new IdentityMessage
                {
                    Subject = "Wealth Health Invite",
                    Destination = model.Email,
                    Body = "Hello, " + model.First + " " + model.Last + "." + Environment.NewLine + Environment.NewLine +
                    "You have been invited to join " + sender.DisplayName + "'s household.  If you want to accept the invitation, please click <a href=\"" + callbackUrl + "\">here</a>.  If you think that this is a mistake, please ignore this email."
                });
            ViewBag.Email = true;
            return RedirectToAction("Household", new { Id = house.HouseId });
        }

        public ActionResult SetKeyword(int id)
        {
            ViewBag.Id = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetKeyword(string keyword, int id)
        {
            db.Households.FirstOrDefault(h => h.Id == id).Keyword = keyword;
            db.SaveChanges();
            ViewBag.Set = "Your keyword has been sucessfully set!";
            return RedirectToAction("InviteUser", id);
        }

        public ActionResult Register(string id)
        {
            ViewBag.Id = id;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}