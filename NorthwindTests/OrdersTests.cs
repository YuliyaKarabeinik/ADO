using System;
using System.Linq;
using ADO;
using ADO.Models;
using FluentAssertions;
using NUnit.Framework;
using static NorthwindTests.Configuration.Config;

namespace NorthwindTests
{
    [TestFixture]
    public class OrdersTests
    {
        private OrderRepository orderRepository;

        [SetUp]
        public void SetUp()
        {
            orderRepository = new OrderRepository(ConnectionString, ProviderName);
        }

        [Test]
        public void GetOrdersTest()
        {
            var orders = orderRepository.Get();
            orders.Should().NotBeEmpty();
        }

        [Test]
        public void GetOrderByIdTest()
        {
            var orderId = orderRepository.Get().FirstOrDefault().OrderId;
            var order = orderRepository.GetById(orderId);

            foreach (var orderDetail in order.OrderDetails)
            {
                orderDetail.Product.ProductId.Should().BePositive();
                orderDetail.Product.ProductName.Should().NotBeNullOrEmpty();
            }
        }

        [Test]
        public void AddOrderTest()
        {
            var orderModel = OrdersExtensions.GenerateOrderModel();
            orderRepository.Create(orderModel);

            var order = orderRepository.Get().Where(ordr => ordr.ShippedDate == orderModel.ShippedDate);
            order.Should().NotBeNull();
        }

        [Test]
        public void UpdateOrderTest()
        {
            var order = orderRepository.Get().FirstOrDefault();
            var country = order.ShipCountry = "Belgium";
            orderRepository.Update(order);
            var modifiedOrder = orderRepository.GetById(order.OrderId);
            modifiedOrder.ShipCountry.Should().BeEquivalentTo(country);
        }
    }
}
