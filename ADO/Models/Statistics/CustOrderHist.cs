using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO.Models.Statistics
{
    public class CustOrderHist : BaseViewModel
    {
        public string ProductName { get; set; }
        public int Total { get; set; }
    }
}
