using System;
using System.Linq;
using ADO;
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
            var orders = orderRepository.GetOrders();
            orders.Should().NotBeEmpty();
        }

        [Test]
        public void GetOrderByIdTest()
        {
            var orders = orderRepository.GetOrders();

            var order = orderRepository.GetOrderById(10285);
            //orders.Should().NotBeEmpty();
        }
    }
}
