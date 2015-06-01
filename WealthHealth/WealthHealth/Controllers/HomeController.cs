using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WealthHealth.Models;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using WealthHealth.Models.Custom;

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

       /* public JsonResult GetDate(int type){
            if (type == 1){
                return Json(System.DateTimeOffset.Now.Month, JsonRequestBehavior.AllowGet);
    }
            else if (type == 2)
            {
                if(System.DateTimeOffset.Now.Day + 7 > System.DateTime.DaysInMonth(System.DateTimeOffset.Now.Year,System.DateTimeOffset.Now.Month){
                    var currMonth = System.DateTimeOffset.Now.Month - 1;
                    var 
                }
                return Json()
            }
            }*/
        public JsonResult GetCategories()
        {
            var list = new List<string>();

            var results = from c in db.Categories
                          let count = c.Transactions.Count
                          select new {
                              label = c.Name,
                              value = (from t in c.Transactions
                                       where t.Type == "Expense"
                                       select t.Amount).DefaultIfEmpty().Sum()
                          };

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBudget()
        {
            var id = User.Identity.GetHouseholdId();
            var days = System.DateTime.DaysInMonth(System.DateTime.Now.Year, System.DateTime.Now.Month);
            var list = new List<string>();
            for (int i = 1; i <= days; i++)
            {
                list.Add(i.ToString());
            }
            var numOfDays = (from day in Enumerable.Range(1, System.DateTime.DaysInMonth(System.DateTime.Now.Year, System.DateTime.Now.Month))
                             select new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, day)).ToList();
            var results = from b in db.BudgetItems
                          from day in numOfDays
                          select new{
                              day = day.Day.ToString(),
                              moneyBudgeted = (b.Amount - (from t in b.Category.Transactions
                                                          where t.Type == "Expense" && (1 <= t.Date.Day && t.Date.Day <= day.Day) & t.Account.HouseholdId == id
                                                          select t.Amount).DefaultIfEmpty().Sum()) > 0 ? (b.Amount - (from t in b.Category.Transactions
                                                                                                                 where t.Type == "Expense" && (1 <= t.Date.Day && t.Date.Day <= day.Day) & t.Account.HouseholdId == id
                                                                                                                 select t.Amount).DefaultIfEmpty().Sum()) : 0,
                              moneySpent = (from t in b.Category.Transactions
                                            where t.Type == "Expense" && (1 <= t.Date.Day  && t.Date.Day <= day.Day) & t.Account.HouseholdId == id
                                                select t.Amount).DefaultIfEmpty().Sum(),
                              moneyOver = ((from t in b.Category.Transactions
                                                          where t.Type == "Expense" && (1 <= t.Date.Day && t.Date.Day <= day.Day) & t.Account.HouseholdId == id
                                                          select t.Amount).DefaultIfEmpty().Sum() - b.Amount) > 0 ? (from t in b.Category.Transactions
                                                          where t.Type == "Expense" && (1 <= t.Date.Day && t.Date.Day <= day.Day) & t.Account.HouseholdId == id
                                                          select t.Amount).DefaultIfEmpty().Sum() - b.Amount : 0
                          };
            return Json(results, JsonRequestBehavior.AllowGet);
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
            if (db.Categories.Count() == 0)
            {
                ViewBag.NoCat = true;
            }
            else
            {
                var list = db.Categories.ToList();
                var categories = new List<string>();
                foreach (var item in list)
                {
                    if (item.Transactions.Any(t => t.Account.HouseholdId == house.HouseId))
                    {
                        categories.Add(item.Name);
                     
                    }
                }
                ViewBag.Categories = categories;
                ViewBag.NoCat = false;
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