using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace NorthwindDAL
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DbProviderFactory ProviderFactory;
        private readonly string ConnectionString;

        public OrderRepository(string connectionString, string provider)
        {
            ProviderFactory = DbProviderFactories.GetFactory(provider);
            ConnectionString = connectionString;
        }
        public IEnumerable<Order> GetOrders()
        {
            var ordersList = new List<Order>();
            using (var connection = ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        select OrderID, 
                        CustomerID, 
                        EmployeeID, 
                        OrderDate, 
                        RequiredDate, 
                        ShippedDate, 
                        ShipVia, 
                        Freight, 
                        ShipName, 
                        ShipAddress, 
                        ShipCity, 
                        ShipRegion, 
                        ShipPostalCode, 
                        ShipCountry,
                        CASE 
                        	  WHEN OrderDate IS NULL THEN 'New'
                        	  WHEN ShippedDate IS NULL THEN 'InProgress'
                        	  WHEN ShippedDate IS NOT NULL THEN 'Completed'
                        	  END AS Status
                        from dbo.Orders";
                    command.CommandType = CommandType.Text;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var order = new Order();
                            order.OrderId = reader.GetInt32(0);
                            order.CustomerID = reader.GetString(1);
                            order.EmployeeID = reader.GetInt32(2);
                            order.OrderDate = reader.GetDateTime(3);
                            order.ShippedDate = reader.GetDateTime(4);
                            order.ShipVia = reader.GetInt32(5);
                            order.Freight = reader.GetDecimal(6);

                            order.EmployeeID = reader.GetInt32(2);



                            order.OrderDate = reader.GetDateTime(1);

                            ordersList.Add(order);
                        }
                    }
                }
            }

            return ordersList;
        }

        public IEnumerable<Order> GetOrderByOrderDate(DateTime orderDate)
        {
            throw new NotImplementedException();
        }

        public Order GetOrderById(int id)
        {
            using (var connection = ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select OrderID, OrderDate from dbo.Orders where OrdeId=@id" +
                        "select * from dbo.[Order Details] where OrderId = @id";
                    command.CommandType = CommandType.Text;

                    var paramId = command.CreateParameter();
                    paramId.ParameterName = "@id";
                    paramId.Value = id;

                    command.Parameters.Add(paramId);

                    using (var reader = command.ExecuteReader())
                    {
                        var order = new Order();
                        order.OrderId = reader.GetInt32(0);
                        order.OrderDate = reader.GetDateTime(1);

                        reader.NextResult();
                        order.Details = new List<OrderDetail>();
                        while (reader.Read())
                        {
                            var detail = new OrderDetail();
                            detail.UnitPrice = (decimal)reader["unitPrice"];
                            detail.Quantity = (int)reader["Quantity"];

                            order.Details.Add(detail);
                        }
                        return order;
                    }
                }
            }
        }

        public Order AddNew(Order newOrder)
        {
            throw new NotImplementedException();
        }

        public Order Updater(Order order)
        {
            throw new NotImplementedException();
        }
    }
}
