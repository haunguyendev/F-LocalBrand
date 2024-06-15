using Microsoft.EntityFrameworkCore;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Data.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Data.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SwdFlocalBrandContext _context;


        public ICampaignRepository Campaigns { get; }

        public ICategoryRepository Categories { get; }

        public ICollectionRepository Collections { get; }


        //public IOrderDetailRepository OrderDetails { get; }

        //public IOrderHistoryRepository OrderHistories { get; }

        public IOrderRepository Orders { get; }

        //public IPaymentRepository Payments { get; }

        //public IRoleRepository Roles { get; }


        public IUserRepository Users { get; }

        public ICustomerRepository Customers { get; }

        public IProductRepository Products { get; }

        // ICampaignRepository campaignRepository,
        //ICategoryRepository categoryRepository, ICollectionRepository collectionRepository,
        //    ICustomerRepository customerRepository, IOrderDetailRepository orderDetailRepository,
        //    IOrderHistoryRepository orderHistoryRepository, IOrderRepository orderRepository,
        //    IPaymentRepository paymentRepository, IRoleRepository roleRepository,
        //    IUserRepository userRepository

        public UnitOfWork(SwdFlocalBrandContext context , IUserRepository userRepository, ICustomerRepository customerRepository, IProductRepository products, ICategoryRepository category, ICampaignRepository campaigns, ICollectionRepository collections, IOrderRepository orders)
        {
            _context = context;
            //Products = productRepository;
            //Campaigns = campaignRepository;
            //Categorys = categoryRepository;
            //Orders = orderRepository;
            //Collections=collectionRepository;
            //Customers=customerRepository;
            //OrderDetails= orderDetailRepository;
            //OrderHistories= orderHistoryRepository;
            //Payments= paymentRepository;
            //Roles= roleRepository;
            Users = userRepository;
            Customers = customerRepository;
            Products = products;
            Categories = category;
            Campaigns = campaigns;
            Collections = collections;
            Orders = orders;
        }
        public void Dispose() => _context.Dispose();

        public Task<int> CommitAsync() => _context.SaveChangesAsync();
    }
}
