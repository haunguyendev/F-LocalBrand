using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Org.BouncyCastle.Crypto.Generators;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Business.DTO.Customer;
using SWD.F_LocalBrand.Business.Helpers;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Data.Models;
using SWD.F_LocalBrand.Data.Repositories;
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
        public async Task<OrderModel?> GetOrderHistoryByCustomerId(int customerId, int orderId)
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
        public async Task<List<CustomerProductModel>?> GetCustomerProductByCustomerId(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            var customerProduct = await _unitOfWork.CustomerProducts.FindByCondition(c => c.CustomerId == id, false).Where(c => c.Status == "true").Include(c => c.Product).ToListAsync();
            if(customer != null)
            {
                var customerModel = _mapper.Map<List<CustomerProductModel>>(customerProduct);
                return customerModel;
            }
            return null;
        }

        //Get customer product by customer id (see product recommended of products)
        public async Task<List<CustomerProductModel>?> GetCustomerProductAndProductRecommendByCustomerId(int customerId)
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

        //update customer product
        public async Task<bool> UpdateCustomerProduct(int customerId, int productId)
        {
            var customerProduct = await _unitOfWork.CustomerProducts.FindByCondition(c => c.CustomerId == customerId && c.ProductId == productId).FirstOrDefaultAsync();
            if (customerProduct != null)
            {
                customerProduct.Status = customerProduct.Status == "false" ? "true" :  "false";
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
            return false;
        }
        #region update customer
        public async Task<CustomerUpdateModel?> UpdateCustomerAsync(CustomerUpdateModel customerUpdateModel)
        {
            var customer = await _unitOfWork.Customers.FindAsync(c => c.Id == customerUpdateModel.Id);

            if (customer == null)
            {
                return null;
            }

            
            if (!string.IsNullOrEmpty(customerUpdateModel.FullName))
                customer.FullName = customerUpdateModel.FullName;

            if (!string.IsNullOrEmpty(customerUpdateModel.Email))
                customer.Email = customerUpdateModel.Email;

            if (!string.IsNullOrEmpty(customerUpdateModel.Image))
                customer.Image = customerUpdateModel.Image;

            if (!string.IsNullOrEmpty(customerUpdateModel.Phone))
                customer.Phone = customerUpdateModel.Phone;

            if (!string.IsNullOrEmpty(customerUpdateModel.Address))
                customer.Address = customerUpdateModel.Address;

            
            await _unitOfWork.Customers.UpdateAsync(customer);
            await _unitOfWork.CommitAsync();

            return customerUpdateModel;
        }
        #endregion
        #region register customer
        public async Task RegisterCustomerAsync(CustomerCreateModel model)
        {
            var customer = new Customer
            {
                UserName = model.UserName,
                FullName = model.FullName,
                Password = Helpers.SecurityUtil.Hash(model.Password), 
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                RegistrationDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Image = "default.jpg" 
            };

            await _unitOfWork.Customers.CreateAsync(customer);
            await _unitOfWork.CommitAsync();
        }

        public bool UsernameExistsAsync(string username)
        {
            var customer=  _unitOfWork.Customers.FindByCondition(c => c.UserName == username);
            if(customer== null) return true;
            return false;
        }
        #endregion

    }
}
