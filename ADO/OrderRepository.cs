using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using ADO.Models;

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

        public IEnumerable<Order> Get()
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
                            var order = ReadOrderDefaultData(reader, new Order());
                            ordersList.Add(order);
                        }
                    }
                }
            }

            return ordersList;
        }

        public Order GetById(int id)
        {
            using (var connection = ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"SELECT                     
                        Orders.OrderID,                        
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
                        [Order Details].UnitPrice,
                        Quantity,
                        Discount,
                        Products.ProductID, ProductName,
                        CASE 
                        	  WHEN OrderDate IS NULL THEN 'New'
                        	  WHEN ShippedDate IS NULL THEN 'InProgress'
                        	  WHEN ShippedDate IS NOT NULL THEN 'Completed'
                        END AS Status
                        FROM Orders
                        LEFT JOIN[Order Details] on Orders.OrderID = [Order Details].OrderID
                        LEFT JOIN Products on [Order Details].ProductID = Products.ProductID
                        WHERE Orders.OrderID = @id";
                    command.CommandType = CommandType.Text;

                    var paramId = command.CreateParameter();
                    paramId.ParameterName = "@id";
                    paramId.Value = id;

                    command.Parameters.Add(paramId);

                    using (var reader = command.ExecuteReader())
                    {
                        var order = new Order();
                        var orderDetails = new List<OrderDetail>();
                      
                        while (reader.Read())
                        {
                            var orderDetail = new OrderDetail();
                            orderDetail.Product = new Product();

                            order = ReadOrderDefaultData(reader, order);

                            orderDetail.Discount = reader.GetSafe<float>("Discount");
                            orderDetail.Quantity = reader.GetSafe<short>("Quantity");

                            orderDetail.Product.ProductId = reader.GetSafe<int>("ProductID");
                            orderDetail.Product.ProductName = reader.GetSafe<string>("ProductName");

                            orderDetails.Add(orderDetail);
                        }
                        order.OrderDetails = orderDetails;
                        return order;
                    }
                }
            }
        }
        
        public void Create(Order orderModel)
        {
            using (var connection = ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO Orders(CustomerID, EmployeeID, OrderDate,
                        RequiredDate, ShippedDate,ShipVia,Freight,ShipName,ShipAddress,ShipCity,ShipRegion, 
                        ShipPostalCode, ShipCountry)  
                        VALUES(@CustomerID, @EmployeeID, @OrderDate, @RequiredDate, @ShippedDate, @ShipVia,
                        @Freight, @ShipName, @ShipAddress, @ShipCity, @ShipRegion, 
                        @ShipPostalCode, @ShipCountry)";
                    command.CommandType = CommandType.Text;

                    foreach (var propertyInfo in orderModel.GetType().GetProperties())
                    {
                        if (propertyInfo.Name == "OrderId") continue;
                        var param = command.CreateParameter();
                        param.ParameterName = $"@{propertyInfo.Name}";
                        param.Value = propertyInfo.GetValue(orderModel, null) ?? DBNull.Value;
                        command.Parameters.Add(param);
                    }
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(Order order)
        {
            var initialOrder = GetById(order.OrderId);
            if (initialOrder.OrderStatus == OrderStatus.InProgress ||
                initialOrder.OrderStatus == OrderStatus.Completed) return;
            using (var connection = ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $@"
                        UPDATE Orders
                        SET CustomerID=@CustomerID, EmployeeID=@EmployeeID,
                        RequiredDate=@RequiredDate, ShipVia=@ShipVia,
                        Freight=@Freight,ShipName=@ShipName,ShipAddress=@ShipAddress,ShipCity=@ShipCity,
                        ShipRegion=@ShipRegion,ShipPostalCode=@ShipPostalCode, ShipCountry=@ShipCountry
                        WHERE Orders.OrderID = {order.OrderId}";

                    command.CommandType = CommandType.Text;
                    foreach (var propertyInfo in order.GetType().GetProperties())
                    {
                        if (propertyInfo.Name == "OrderId" || propertyInfo.Name == "OrderDate" || propertyInfo.Name == "ShippedDate") continue;
                        var param = command.CreateParameter();
                        param.ParameterName = $"@{propertyInfo.Name}";
                        param.Value = propertyInfo.GetValue(order, null) ?? DBNull.Value;
                        command.Parameters.Add(param);
                    }
                    command.ExecuteNonQuery();
                }
            }
        }
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        private Order ReadOrderDefaultData(DbDataReader reader, Order order)
        {
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
            return order;
        }
    }
}
