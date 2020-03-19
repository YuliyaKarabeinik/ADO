using System;
using System.Collections.Generic;

namespace ADO
{
    public interface IOrderRepository
    {
        IEnumerable<Order> GetOrders();

        IEnumerable<Order> GetOrderByOrderDate(DateTime orderDate);

        Order GetOrderById(int id);

        Order AddNew(Order newOrder);

        Order Updater(Order order);
    }
}
