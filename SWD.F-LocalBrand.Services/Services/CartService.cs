using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Data.Models;
using SWD.F_LocalBrand.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.Services
{
    public class CartService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CartService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
    
        }

        public async Task AddToCartAsync(int customerId, int productId, int quantity)
        {
            try
            {
                var product = await _unitOfWork.Products.FindAsync(x => x.Id == productId);
                if (product == null || product.StockQuantity < quantity)
                {
                    throw new Exception("Product not found or insufficient stock");
                }

                var cartOrder = await _unitOfWork.Orders.GetCartByCustomerId(customerId);
                if (cartOrder == null)
                {
                    cartOrder = new Order
                    {
                        CustomerId = customerId,
                        OrderDate = DateOnly.FromDateTime(DateTime.Now),
                        OrderStatus = "Cart",
                        TotalAmount = 0m
                    };
                    await _unitOfWork.Orders.CreateAsync(cartOrder);
                }

                var orderDetail = cartOrder.OrderDetails.FirstOrDefault(od => od.ProductId == productId);
                if (orderDetail == null)
                {
                    orderDetail = new OrderDetail
                    {
                        OrderId = cartOrder.Id,
                        ProductId = productId,
                        Quantity = quantity,
                        Price = product.Price
                    };
                    cartOrder.OrderDetails.Add(orderDetail);
                    await _unitOfWork.OrderDetails.CreateAsync(orderDetail);
                }
                else
                {
                    orderDetail.Quantity += quantity;
                    await _unitOfWork.OrderDetails.UpdateAsync(orderDetail);
                }

                product.StockQuantity -= quantity;
                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.CommitAsync();

                cartOrder.TotalAmount = cartOrder.OrderDetails.Sum(od => od.Quantity * od.Price);
                await _unitOfWork.Orders.UpdateAsync(cartOrder);
                await _unitOfWork.CommitAsync();
            }catch(Exception ex)
            {
                _unitOfWork.Dispose();
                throw new Exception("Product not found or insufficient stock");

            }
        }
    }
}
