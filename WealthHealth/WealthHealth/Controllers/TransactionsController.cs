using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WealthHealth.Models;
using WealthHealth.Models.Custom;

namespace WealthHealth.Controllers
{
    public class TransactionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Transactions
        public ActionResult Index()
        {
            var transactions = db.Transactions.Include(t => t.Account).Include(t => t.Category);
            return View(transactions.ToList());
        }

        // GET: Transactions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // GET: Transactions/Create
        public ActionResult Create()
        {
            ViewBag.AccountId = new SelectList(db.Accounts.Where(a => a.HouseholdId == User.Identity.GetHouseholdId()).ToList(), "Id", "Name");
            if (db.Categories.Count() == 0)
            {
                ViewBag.NoCat = true;
            }
            else
            {
                ViewBag.CategoryId = new SelectList(db.Categories.Where(c => c.Transactions.Any(t => t.Account.HouseholdId == User.Identity.GetHouseholdId())).ToList(), "Id", "Name");

            }
            
            
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Type,Description,MoneyFlow,Amount,ReconciledAmount,Date,AccountId,CategoryId")] Transaction transaction, string newCat)
        {
            if (newCat != "")
            {
                var cat = new Category{
                    Name = newCat
                };
                db.Categories.Add(cat);
                db.SaveChanges();
                transaction.CategoryId = cat.Id;
            }
            var account = db.Accounts.FirstOrDefault(a => a.Id == transaction.AccountId);
            if (transaction.MoneyFlow == true)
            {
                transaction.Type = "Income";
            }
            else { transaction.Type = "Expense"; }
            if (ModelState.IsValid)
            {
                if (transaction.Type == "Income")
                {
                    account.Amount += transaction.Amount;
                }
                else
                {
                    account.Amount -= transaction.Amount;
                }
                db.Transactions.Add(transaction);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            var house = User.Identity.GetHouseholdId();
            ViewBag.AccountId = new SelectList(db.Accounts.Where(a => a.HouseholdId == house).ToList(), "Id", "Name", transaction.AccountId);
            ViewBag.CategoryId = new SelectList(db.Categories.Where(c => c.Transactions.Any(t => t.Account.HouseholdId == house)).ToList(), "Id", "Name", transaction.CategoryId);
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            var house = User.Identity.GetHouseholdId();
            ViewBag.AccountId = new SelectList(db.Accounts.Where(a => a.HouseholdId == house).ToList(), "Id", "Name", transaction.AccountId);
            ViewBag.CategoryId = new SelectList(db.Categories.Where(c => c.Transactions.Any(t => t.Account.HouseholdId == house)).ToList(), "Id", "Name", transaction.CategoryId);
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Type,Description,MoneyFlow,Amount,ReconciledAmount,Date,AccountId,CategoryId")] Transaction transaction, string newCat)
        {

            if (newCat != "")
            {
                var cat = new Category
                {
                    Name = newCat
                };
                db.Categories.Add(cat);
                db.SaveChanges();
                transaction.CategoryId = cat.Id;
            }
                var oldTransaction = (from t in db.Transactions.AsNoTracking()
                                      where t.Id == transaction.Id
                                      select t).FirstOrDefault();
                var oldAccount = db.Accounts.FirstOrDefault(a => a.Id == oldTransaction.AccountId);

                var account = db.Accounts.FirstOrDefault(a => a.Id == transaction.AccountId);
                if (transaction.MoneyFlow == true)
                {
                    transaction.Type = "Income";
                }
                else { transaction.Type = "Expense"; }
            if (ModelState.IsValid)
            {
                if (oldTransaction.Type == "Expense")
                {
                    oldAccount.Amount += oldTransaction.Amount;
                }
                else
                {
                    oldAccount.Amount -= oldTransaction.Amount;
                }
                if (transaction.Type == "Income")
                {
                    account.Amount += transaction.Amount;
                }
                else
                {
                    account.Amount -= transaction.Amount;
                }
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AccountId = new SelectList(db.Accounts.Where(a => a.HouseholdId == User.Identity.GetHouseholdId()).ToList(), "Id", "Name", transaction.AccountId);
            ViewBag.CategoryId = new SelectList(db.Categories.Where(c => c.Transactions.Any(t => t.Account.HouseholdId == User.Identity.GetHouseholdId())).ToList(), "Id", "Name", transaction.CategoryId);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Transaction transaction = db.Transactions.Find(id);
            Account account = db.Accounts.Find(transaction.AccountId);
            if (transaction.Type == "Income")
            {
                account.Amount -= transaction.Amount;
            }
            else
            {
                account.Amount += transaction.Amount;
            }
            db.Transactions.Remove(transaction);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
