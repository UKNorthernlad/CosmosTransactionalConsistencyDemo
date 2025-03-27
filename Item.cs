using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDBUpdateTest
{
    // C# record type for items in the container
    public class BalanceItem
    {
        public string id { get; set; }
        public string accountid { get; set; }
        public string documenttype { get; set; }
        public int balance { get; set; }
    }

    public class TransactionItem
    {
        public string id { get; set; }
        public string accountid { get; set; }
        public string documenttype { get; set; }
        public int transaction { get; set; }
    }

}
