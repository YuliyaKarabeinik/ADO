using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthwindDAL
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
