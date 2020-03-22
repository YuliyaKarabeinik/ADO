using System;

namespace ADO.Models
{
    public class OrderDetail : BaseViewModel
    {
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public decimal UnitPrice { get; set; }
        public short Quantity { get; set; }
        public float Discount { get; set; }
        public Product Product { get; set; }

    }
}