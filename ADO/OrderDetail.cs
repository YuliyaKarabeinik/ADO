using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ADO
{
    public class OrderDetail
    {
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public double Discount { get; set; }

        public List<Product> Products { get; set; }

    }
}