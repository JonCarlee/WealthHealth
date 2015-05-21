using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WealthHealth.Models;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace WealthHealth.Controllers
{
    
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                if (user.HouseholdId == null)
                {
                    return RedirectToAction("RegisteredIndex");
                }
                return RedirectToAction("Household", "Home", new { Id = user.HouseholdId });
            }
            else
            {
                return View();
            }
        }
        [AllowAnonymous]
        [Authorize]
        public ActionResult RegisteredIndex()
        {
            return View();
        }

        public ActionResult Leave(int id)
        {
            var bye = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(u => u.Id == bye);
            user.HouseholdId = null;
            db.Households.FirstOrDefault(h => h.Id == id).Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //The main page
        //goes here
        //look at it
        public ActionResult Household(string Id)
        {
            var house = db.Households.FirstOrDefault(h => h.HouseId == Id);
            decimal a = 0;
            if (house.Accounts.Count == 0)
            {

            }
            else
            {
                foreach (var account in house.Accounts)
                {
                    a += account.Amount;
                }
            }
            if (house.BudgetItems.Count == 0)
            {
                ViewBag.NoCat = true;
            }
            else
            {
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            ViewBag.Balances = a;
            return View(house);
        }

        //you are now 
        //leaving the main page
        //we will miss you

        public ActionResult InviteUser(int id)
        {
            if (db.Households.FirstOrDefault(h => h.Id == id).Keyword == null)
            {
                return RedirectToAction("SetKeyword", new { Id = id });
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
            var callbackUrl = Url.Action("InviteRegister", "Home", new { Id = house.HouseId }, protocol: Request.Url.Scheme);
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
            return RedirectToAction("InviteUser", new { Id = id });
        }

        public ActionResult InviteRegister(string id)
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

        public ActionResult Test()
        {
            return View();
        }
    }
}