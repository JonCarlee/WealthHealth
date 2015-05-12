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
        }
        public int Id { get; set; }
        public ICollection<ApplicationUser> Users { get; set; }
        public ICollection<Account> Accounts { get; set; }
        public ICollection<BudgetItem> BudgetItems { get; set; }
    }
}