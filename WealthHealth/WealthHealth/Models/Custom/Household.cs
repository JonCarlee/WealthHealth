using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WealthHealth.Models.Custom
{
    public class Household
    {
        public Household()
        {
            this.Users = new HashSet<ApplicationUser>();
            this.Accounts = new HashSet<Account>();
            this.BudgetItems = new HashSet<BudgetItem>();
            this.Transactions = new HashSet<Transaction>();
        }
        public int Id { get; set; }
        public string HouseId { get; set; }
        public string Name { get; set; }
        public string Keyword { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<BudgetItem> BudgetItems { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }

    }
}