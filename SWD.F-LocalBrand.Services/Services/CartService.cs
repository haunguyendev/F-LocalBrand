using SWD.F_LocalBrand.Business.DTO.Cart;
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
        #region add to cart service
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

                cartOrder.TotalAmount = cartOrder.OrderDetails.Sum(od => od.Quantity * od.Price);
                await _unitOfWork.Orders.UpdateAsync(cartOrder);
                await _unitOfWork.CommitAsync();
            }catch(Exception ex)
            {
                _unitOfWork.Dispose();
                throw new Exception("Error while add to cart .");

            }
            #endregion

           
        }
        #region update cart
        public async Task UpdateCartAsync(int customerId, ListCartUpdateModel cartUpdateModel)
        {
            var cartOrder = await _unitOfWork.Orders.GetCartByCustomerId(customerId);
            if (cartOrder == null)
            {
                throw new Exception("Cart not found.");
            }

            // Remove existing order details
            await _unitOfWork.OrderDetails.DeleteListAsync(cartOrder.OrderDetails);

            foreach (var product in cartUpdateModel.Products)
            {
                var productEntity = await _unitOfWork.Products.FindAsync(x => x.Id == product.ProductId);
                if (productEntity == null || productEntity.StockQuantity < product.Quantity)
                {
                    throw new Exception($"Insufficient stock for product ID {product.ProductId}.");
                }

                var orderDetail = new OrderDetail
                {
                    OrderId = cartOrder.Id,
                    ProductId = product.ProductId,
                    Quantity = product.Quantity,
                    Price = productEntity.Price
                };

                cartOrder.OrderDetails.Add(orderDetail);

                               
                await _unitOfWork.Products.UpdateAsync(productEntity);
            }

            cartOrder.TotalAmount = cartOrder.OrderDetails.Sum(od => od.Quantity * od.Price);
            await _unitOfWork.Orders.UpdateAsync(cartOrder);
            await _unitOfWork.CommitAsync();
        }
        #endregion
        #region delete item in cart
        public async Task DeleteCartItemAsync(int customerId, int productId)
        {
            var cartOrder = await _unitOfWork.Orders.GetCartByCustomerId(customerId);
            if (cartOrder == null)
            {
                throw new Exception("Cart not found.");
            }

            var orderDetail = cartOrder.OrderDetails.FirstOrDefault(od => od.ProductId == productId);
            if (orderDetail == null)
            {
                throw new Exception("Product not found in cart.");
            }

            cartOrder.OrderDetails.Remove(orderDetail);
            await _unitOfWork.OrderDetails.DeleteAsync(orderDetail);

            // Update stock quantity
           

            cartOrder.TotalAmount = cartOrder.OrderDetails.Sum(od => od.Quantity * od.Price);
            await _unitOfWork.Orders.UpdateAsync(cartOrder);
            await _unitOfWork.CommitAsync();
        }
        #endregion
    }
}
