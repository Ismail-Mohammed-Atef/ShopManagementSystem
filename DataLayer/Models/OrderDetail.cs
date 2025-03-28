﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models
{
    public class OrderDetail : BaseEntity
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public double Quantity { get; set; }
        public double SellPrice { get; set; }
        public double BuyPrice { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}
