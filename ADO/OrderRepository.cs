using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using ADO.Models;
using ADO.Models.Order;
using ADO.Models.Statistics;

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
                        	  WHEN OrderDate IS NULL and ShippedDate IS NULL THEN 'New'
                        	  WHEN OrderDate IS NOT NULL and ShippedDate IS NULL THEN 'InProgress'
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
                    command.CommandText = $@"UPDATE Orders
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
            var initialOrder = GetById(id);
            if (initialOrder.OrderStatus == OrderStatus.Completed) return;

            using (var connection = ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $@"DELETE FROM [Order Details] WHERE OrderID = {id}
                                            DELETE FROM Orders WHERE OrderID = {id}";

                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }

        public void ChangeToInProgress(int id)
        {
            var initialOrder = GetById(id);
            if (initialOrder.OrderStatus == OrderStatus.InProgress || initialOrder.OrderStatus == OrderStatus.Completed) return;
            using (var connection = ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $@"UPDATE Orders
                        SET OrderDate='{DateTime.Now}'
                        WHERE OrderID = {id}";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }

        public void ChangeToCompleted(int id)
        {
            var initialOrder = GetById(id);
            if (initialOrder.OrderStatus == OrderStatus.Completed) return;
            using (var connection = ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $@"UPDATE Orders
                        SET ShippedDate='{DateTime.Now}'
                        WHERE OrderID = {id}";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<CustOrderHist> GetCustOrderHist(string customerId)
        {
            string spName = "dbo.CustOrderHist";
            var custOrderHistList = new List<CustOrderHist>();
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                using (var command = new SqlCommand(spName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    var customerIdParam = new SqlParameter("@CustomerID", customerId);
                    command.Parameters.Add(customerIdParam);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var custOrderHist = new CustOrderHist();
                            custOrderHist.ProductName = reader.GetSafe<string>("ProductName");
                            custOrderHist.Total = reader.GetSafe<int>("Total");

                            custOrderHistList.Add(custOrderHist);
                        }

                        return custOrderHistList;
                    }
                }
            }
        }

        public List<CustOrderDetail> GetCustOrderDetail(int orderId)
        {
            string spName = "dbo.CustOrdersDetail";
            var custOrderDetailList = new List<CustOrderDetail>();
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(spName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    var customerIdParam = new SqlParameter("@OrderID", orderId);
                    command.Parameters.Add(customerIdParam);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var custOrderDetail = new CustOrderDetail();
                            custOrderDetail.ProductName = reader.GetSafe<string>("ProductName");
                            custOrderDetail.UnitPrice = reader.GetSafe<decimal>("UnitPrice");
                            custOrderDetail.Quantity = reader.GetSafe<short>("Quantity");
                            custOrderDetail.Discount = reader.GetSafe<int>("Discount");
                            custOrderDetail.ExtendedPrice = reader.GetSafe<decimal>("ExtendedPrice");
                            
                            custOrderDetailList.Add(custOrderDetail);
                        }

                        return custOrderDetailList;
                    }
                }
            }
        }

        private Order ReadOrderDefaultData(DbDataReader reader, Order order)
        {
            order.OrderId = reader.GetSafe<int>("OrderID");
            order.CustomerID = reader.GetSafe<string>("CustomerID");
            order.EmployeeID = reader.GetNullableInt("EmployeeID");
            order.OrderDate = reader.GetNullableDateTime("OrderDate");
            order.RequiredDate = reader.GetNullableDateTime("RequiredDate");
            order.ShippedDate = reader.GetNullableDateTime("ShippedDate");
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
