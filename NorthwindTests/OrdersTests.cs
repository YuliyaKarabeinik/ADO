using System;
using System.Linq;
using ADO;
using ADO.Models;
using Common;
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
        public void UpdateOrderStatusNew_Allowed_Test()
        {
            var orderStatusNew = orderRepository.Get().FirstOrDefault(ordr => ordr.OrderStatus == OrderStatus.New);
            var country = orderStatusNew.ShipCountry = $"Test{RandomGenerator.GetIntWithNDigits(3)}";
            orderRepository.Update(orderStatusNew);
            var modifiedOrder = orderRepository.GetById(orderStatusNew.OrderId);
            modifiedOrder.ShipCountry.Should().BeEquivalentTo(country);
        }

        [Test]
        public void UpdateOrderStatusInProgress_Denied_Test()
        {
            var orderStatusInProgress = orderRepository.Get().FirstOrDefault(ordr => ordr.OrderStatus == OrderStatus.InProgress);
            var initialCountry = orderStatusInProgress.ShipCountry;
            var country = orderStatusInProgress.ShipCountry = $"Test{RandomGenerator.GetIntWithNDigits(3)}";
            orderRepository.Update(orderStatusInProgress);
            var modifiedOrder = orderRepository.GetById(orderStatusInProgress.OrderId);
            modifiedOrder.ShipCountry.Should().BeEquivalentTo(initialCountry);
        }

        [Test]
        public void DeleteOrderStatusInProgress_Allowed_Test()
        {
            var orderId = orderRepository.Get().FirstOrDefault(ordr => ordr.OrderStatus == OrderStatus.InProgress).OrderId;
            orderRepository.Delete(orderId);
            var order = orderRepository.Get().FirstOrDefault(ordr => ordr.OrderId == orderId);
            order.Should().BeNull();
        }

        [Test]
        public void DeleteOrderStatusCompleted_Denied_Test()
        {
            var orderId = orderRepository.Get().FirstOrDefault(ordr => ordr.OrderStatus == OrderStatus.Completed).OrderId;
            orderRepository.Delete(orderId);
            var order = orderRepository.Get().FirstOrDefault(ordr => ordr.OrderId == orderId);
            order.Should().NotBeNull();
        }

        [Test]
        public void ChangeToInProgressTest()
        {
            var orderId = orderRepository.Get().FirstOrDefault(ordr => ordr.OrderStatus == OrderStatus.New).OrderId;

            orderRepository.ChangeToInProgress(orderId);
            var order = orderRepository.Get().FirstOrDefault(ordr => ordr.OrderId == orderId);
            order.OrderStatus.Should().BeEquivalentTo(OrderStatus.InProgress);
        }

        [Test]
        public void ChangeToCompletedTest()
        {
            var orderId = orderRepository.Get().FirstOrDefault(ordr => ordr.OrderStatus == OrderStatus.InProgress).OrderId;
            orderRepository.ChangeToCompleted(orderId);
            var order = orderRepository.Get().FirstOrDefault(ordr => ordr.OrderId == orderId);
            order.OrderStatus.Should().BeEquivalentTo(OrderStatus.Completed);
        }
    }
}
