using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Data.Common.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICampaignRepository Campaigns { get; }
        ICategoryRepository Categories{ get; }
        ICollectionRepository Collections { get; }
        //IOrderDetailRepository OrderDetails { get; }
        //IOrderHistoryRepository OrderHistories { get; }
        IOrderRepository Orders { get; }
        //IPaymentRepository Payments { get; }
        //IRoleRepository Roles { get; }
        IUserRepository Users { get; }
        ICustomerRepository Customers { get; }
        IProductRepository Products { get; }
        
        Task<int> CommitAsync();
    }
}
