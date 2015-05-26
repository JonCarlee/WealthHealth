using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WealthHealth.Models.Custom
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool MoneyFlow { get; set; }
        public decimal Amount { get; set; }
        public decimal? ReconciledAmount { get; set; } //I don't know about this one
        public DateTimeOffset Date { get; set; }
        public int AccountId { get; set; }
        public int CategoryId { get; set; }

        public virtual Account Account { get; set; }
        public virtual Category Category { get; set; }
    }
}