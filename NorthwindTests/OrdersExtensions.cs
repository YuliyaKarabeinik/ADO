﻿using System;
using ADO.Models;
using ADO.Models.Order;

namespace NorthwindTests
{
    public static class OrdersExtensions
    {
        public static Order GenerateOrderModel()
        {
            return new Order()
            {
                OrderDate = DateTime.Now,
                RequiredDate = DateTime.Now.AddDays(2),
                ShippedDate = DateTime.Now.AddDays(1)
            };
        }
    }
}