using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace ADO
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
                            order.OrderId = reader.GetSafe<int>("OrderID");
                            order.CustomerID = reader.GetSafe<string>("CustomerID");
                            order.EmployeeID = reader.GetSafe<int>("EmployeeID");
                            order.OrderDate = reader.GetSafe<DateTime>("OrderDate");
                            order.RequiredDate = reader.GetSafe<DateTime>("RequiredDate");
                            order.ShippedDate = reader.GetSafe<DateTime>("ShippedDate");
                            order.ShipVia = reader.GetSafe<int>("ShipVia");
                            order.Freight = reader.GetSafe<decimal>("Freight");
                            order.ShipName = reader.GetSafe<string>("ShipName");
                            order.ShipAddress = reader.GetSafe<string>("ShipAddress");
                            order.ShipCity = reader.GetSafe<string>("ShipCity");
                            order.ShipRegion = reader.GetSafe<string>("ShipRegion");
                            order.ShipPostalCode = reader.GetSafe<string>("ShipPostalCode");
                            order.ShipCountry = reader.GetSafe<string>("ShipCountry");
                            order.OrderStatus = reader.GetSafe<OrderStatus>("Status");

                            ordersList.Add(order);
                        }
                    }
                }
            }

            return ordersList;
        }

        public Order GetOrderById(int id)
        {
            using (var connection = ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select OrderID, OrderDate from dbo.Orders where OrderID=@id; " +
                        "select * from dbo.[Order Details] where OrderID = @id";
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


        public IEnumerable<Order> GetOrderByOrderDate(DateTime orderDate)
        {
            throw new NotImplementedException();
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
