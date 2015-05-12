using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WealthHealth.Models.Custom
{
    public class BudgetItem
    {
        public BudgetItem()
        {
            this.Transactions = new HashSet<Transaction>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int CategoryId { get; set; }
        public int HouseholdId { get; set; }

        public virtual Category Category { get; set; }
        public virtual Household Household { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}