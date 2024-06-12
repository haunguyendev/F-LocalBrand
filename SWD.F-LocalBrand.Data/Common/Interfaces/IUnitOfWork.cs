using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Data.Common.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        //IProductRepository Products { get; }
        //ICampaignRepository Campaigns { get; }
        //ICategoryRepository Categorys{ get; }
        //ICollectionRepository Collections { get; }
        //ICustomerRepository Customers { get; }
        //IOrderDetailRepository OrderDetails { get; }
        //IOrderHistoryRepository OrderHistories { get; }
        //IOrderRepository Orders { get; }
        //IPaymentRepository Payments { get; }
        //IRoleRepository Roles { get; }
        IUserRepository Users { get; }
        ICustomerRepository Customers { get; }
        
        Task<int> CommitAsync();
    }
}
