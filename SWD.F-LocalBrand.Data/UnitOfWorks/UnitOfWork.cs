using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        private IDbContextTransaction _currentTransaction;

        public ICampaignRepository Campaigns { get; }

        public ICategoryRepository Categories { get; }

        public ICollectionRepository Collections { get; }


        public IOrderDetailRepository OrderDetails { get; }

        //public IOrderHistoryRepository OrderHistories { get; }

        public IOrderRepository Orders { get; }

        public IPaymentRepository Payments { get; }

        public IRoleRepository Roles { get; }


        public IUserRepository Users { get; }

        public ICustomerRepository Customers { get; }

        public IProductRepository Products { get; }

        public ICustomerProductRepository CustomerProducts { get; }

        public ICompapilityRepository Compapilities { get; }

        public IOrderHistoryRepository OrderHistories { get; }

        public ICollectionProductRepository CollectionProducts { get; }

        // ICampaignRepository campaignRepository,
        //ICategoryRepository categoryRepository, ICollectionRepository collectionRepository,
        //    ICustomerRepository customerRepository, IOrderDetailRepository orderDetailRepository,
        //    IOrderHistoryRepository orderHistoryRepository, IOrderRepository orderRepository,
        //    IPaymentRepository paymentRepository, IRoleRepository roleRepository,
        //    IUserRepository userRepository
         public SwdFlocalBrandContext GetDbContext()
        {
            return _context;
        }
        public UnitOfWork(SwdFlocalBrandContext context ,
            IUserRepository userRepository,
            ICustomerRepository
            customerRepository,
            IProductRepository products,
            ICategoryRepository category,
            ICampaignRepository campaigns,
            ICollectionRepository collections,
            IOrderRepository orders,
            ICustomerProductRepository customerProducts,
            IOrderDetailRepository orderDetails,
            IPaymentRepository payments,
            ICompapilityRepository compapilities,
            IRoleRepository roles,
            IOrderHistoryRepository orderHistories)
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
            CustomerProducts = customerProducts;
            OrderDetails = orderDetails;
            Payments = payments;
            Compapilities = compapilities;
            Roles = roles;
            OrderHistories = orderHistories;
        }
        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
            
        }


        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task<int> CommitAsync()
        {
            try
            {
                var result = await _context.SaveChangesAsync();

                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync();
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }

                return result;
            }
            catch
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync();
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }

                throw;
            }
        }

        public async Task RollbackAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        
    }
}
