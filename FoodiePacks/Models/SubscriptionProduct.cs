using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodiePacks.Models
{
    public class SubscriptionProduct
    {
        public string Name { get; set; }
        public int Week { get; set; }
        public int Quantity { get; set; }
        public long Id { get; set; }
    }
}