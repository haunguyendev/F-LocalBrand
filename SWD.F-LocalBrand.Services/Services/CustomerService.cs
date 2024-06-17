using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Business.Helpers;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Data.Models;
using SWD.F_LocalBrand.Data.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.Services
{
    public class CustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ProductService _productService;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, ProductService productService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _productService = productService;
        }



        //Get customer by username
        public async Task<bool?> GetCustomerByUsername(string email)
        {
            var customer = await _unitOfWork.Customers.FindByCondition(c => c.Email == email).FirstOrDefaultAsync();
            if (customer != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> UpdatePass(string email, string newPassword)
        {
            var user = await _unitOfWork.Customers.FindByCondition(c => c.Email == email, true).FirstOrDefaultAsync();

            if (user != null)
            {
                user.Password = SecurityUtil.Hash(newPassword);

                var result = await _unitOfWork.CommitAsync();
                if (result > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        //Get order history of customer by customer id and order id
        public async Task<OrderModel> GetOrderHistoryByCustomerId(int customerId, int orderId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer != null)
            {
                // Check if the order belongs to the customer
                var order = await _unitOfWork.Orders
                    .FindByCondition(o => o.CustomerId == customerId && o.Id == orderId, trackChanges: false)
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync();
                if (order != null)
                {
                    var orderWithHistories = await _unitOfWork.Orders
                        .FindByCondition(o => o.Id == orderId, false, o => o.OrderHistories)
                    .FirstOrDefaultAsync();
                    var orderModels = _mapper.Map<OrderModel>(orderWithHistories);
                    return orderModels;
                }
                throw new Exception("Order not found for the given customer");
            }
            return null;
            

            
        }


        //Get customer by id with customer products
        public async Task<List<CustomerProductModel>> GetCustomerProductByCustomerId(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            var customerProduct = await _unitOfWork.CustomerProducts.FindByCondition(c => c.CustomerId == id, false).ToListAsync();
            if(customer != null)
            {
                var customerModel = _mapper.Map<List<CustomerProductModel>>(customerProduct);
                return customerModel;
            }
            return null;
        }

        //Get customer product by customer id (see product recommended of products)
        public async Task<List<CustomerProductModel>> GetCustomerProductAndProductRecommendByCustomerId(int customerId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);

            var customerProducts = await _unitOfWork.CustomerProducts
            .FindByCondition(cp => cp.CustomerId == customerId, false, cp => cp.Product)
            .ToListAsync();
            if(customer != null)
            {
                var customerProductDtos = _mapper.Map<IEnumerable<CustomerProductModel>>(customerProducts);

                foreach (var customerProductDto in customerProductDtos)
                {
                    if (customerProductDto.Product != null)
                    {
                        var productWithRecommendations = await _productService.GetProductWithRecommendationsAsync(customerProductDto.Product.Id);

                        if (productWithRecommendations != null)
                        {
                            customerProductDto.Product = productWithRecommendations;
                        }
                    }
                }
                return customerProductDtos.ToList();
            }
            return null;
            
        }
    }
}
