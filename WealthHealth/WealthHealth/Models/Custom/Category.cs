using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WealthHealth.Models.Custom
{
    public class Category
    {
        public Category()
        {
            this.Transactions = new HashSet<Transaction>();
            this.BudgetItems = new HashSet<BudgetItem>();
        }
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<BudgetItem> BudgetItems { get; set; }
    }
}