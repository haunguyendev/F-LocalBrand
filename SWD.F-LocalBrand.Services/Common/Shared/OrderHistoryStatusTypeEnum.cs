using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.Common.Shared
{
    public class OrderHistoryStatusTypeEnum
    {
        public const string Preparing = "Preparing";
        public const string Prepared = "Prepared";
        public const string ShipperReceived = "ShipperReceived";
        public const string InTransit = "InTransit";
        public const string Delivered = "Delivered";
        public const string Cancelled = "Cancelled";
    }

}
