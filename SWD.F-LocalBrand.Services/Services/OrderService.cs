using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWD.F_LocalBrand.Business.Common.Shared;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Business.DTO.Cart;
using SWD.F_LocalBrand.Business.DTO.Order;
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
    public class OrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        //get list order have payment status is true
        public async Task<List<OrderModel>> GetOrdersWithPaymentStatusTrueAsync()
        {
            var orders = await _unitOfWork.Orders.FindAll()
                .Include(o => o.Payments)
                .Where(o => o.Payments.Any(p => p.PaymentStatus == "true")).ToListAsync();
            return _mapper.Map<List<OrderModel>>(orders);
        }


        //get order or list order have status from request
        public async Task<IEnumerable<OrderModel>> GetOrdersByStatusAsync(string status)
        {
            var orders = await _unitOfWork.Orders.FindByCondition(o => o.OrderStatus == status).ToListAsync();
            return _mapper.Map<IEnumerable<OrderModel>>(orders);
        }
        #region change status order
        public async Task<UpdateOrderStatusModel?> UpdateOrderStatus(UpdateOrderStatusModel model)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(model.Id);

            if (order == null)
            {
                return null;
            }

            // Update the order status
            order.OrderStatus = model.OrderStatus;

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.CommitAsync();

            return model;
        }
        #endregion

        //get product by order id
        public async Task<List<ProductModel>> GetProductsByOrderIdAsync(int orderId)
        {
            var products = await _unitOfWork.OrderDetails
            .FindByCondition(od => od.OrderId == orderId)
            .Include(od => od.Product)
            .Select(od => od.Product)
            .ToListAsync();
            return _mapper.Map<List<ProductModel>>(products);
        }
        #region create order with payment 
        public async Task CreateOrderAsync(int customerId, List<CartProductModel> products, string paymentMethod)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateOnly.FromDateTime(DateTime.Now),
                    TotalAmount = 0m,
                    OrderStatus = OrderStatusTypeEnum.Pending
                };

                await _unitOfWork.Orders.CreateAsync(order);
                await _unitOfWork.CommitAsync();

                decimal totalAmount = 0m;

                foreach (var product in products)
                {
                    var productEntity = await _unitOfWork.Products.FindAsync(p => p.Id == product.ProductId);
                    if (productEntity == null)
                    {
                        throw new Exception($"Product with ID {product.ProductId} not found");
                    }

                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = product.ProductId,
                        Quantity = product.Quantity,
                        Price = productEntity.Price
                    };

                    totalAmount += product.Quantity * productEntity.Price;
                    productEntity.StockQuantity -= product.Quantity;

                    await _unitOfWork.OrderDetails.CreateAsync(orderDetail);
                    await _unitOfWork.Products.UpdateAsync(productEntity);
                }

                order.TotalAmount = totalAmount;
                await _unitOfWork.Orders.UpdateAsync(order);

                var payment = new Payment
                {
                    OrderId = order.Id,
                    PaymentDate = DateOnly.FromDateTime(DateTime.Now),
                    PaymentMethod = paymentMethod,
                    PaymentStatus = PaymentStatusTypeEnum.Pending
                };

                await _unitOfWork.Payments.CreateAsync(payment);
                await _unitOfWork.CommitAsync();
               
            }
            catch
            {
                await _unitOfWork.RollbackAsync(); 
                throw;
            }
        }
        #endregion
        #region update payment 
        public async Task UpdatePaymentStatusAsync(int paymentId, string status, int statusCode)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var payment = await _unitOfWork.Payments.FindAsync(p => p.Id == paymentId);
                if (payment == null)
                {
                    throw new Exception("Payment not found");
                }

                payment.PaymentStatus = status;
                payment.StatusResponseCode = statusCode;
                await _unitOfWork.Payments.UpdateAsync(payment);

                var order = await _unitOfWork.Orders.FindAsync(o => o.Id == payment.OrderId);
                if (order == null)
                {
                    throw new Exception("Order not found");
                }

                if (status == PaymentStatusTypeEnum.Completed)
                {
                    order.OrderStatus = OrderStatusTypeEnum.Completed;

                    var orderHistory = new OrderHistory
                    {
                        OrderId = order.Id,
                        Status = OrderHistoryStatusTypeEnum.Preparing,
                        ChangeTime = DateTime.Now,
                        Description = "Order is being prepared"
                    };

                    await _unitOfWork.OrderHistories.CreateAsync(orderHistory);

                    var orderDetails = await _unitOfWork.OrderDetails.FindAllAsync(od => od.OrderId == order.Id);
                    foreach (var orderDetail in orderDetails)
                    {
                        var customerProduct = await _unitOfWork.CustomerProducts
                            .FindAsync(cp => cp.CustomerId == order.CustomerId && cp.ProductId == orderDetail.ProductId);

                        if (customerProduct != null)
                        {
                            customerProduct.BuyDate = DateOnly.FromDateTime(DateTime.Now);
                            await _unitOfWork.CustomerProducts.UpdateAsync(customerProduct);
                        }
                        else
                        {
                            customerProduct = new CustomerProduct
                            {
                                CustomerId = order.CustomerId,
                                ProductId = orderDetail.ProductId,
                                BuyDate = DateOnly.FromDateTime(DateTime.Now),
                                Status = true 
                            };

                            await _unitOfWork.CustomerProducts.CreateAsync(customerProduct);
                        }
                    }
                }
                else if (status == PaymentStatusTypeEnum.Failed || status == PaymentStatusTypeEnum.Expired)
                {
                    order.OrderStatus = OrderStatusTypeEnum.Failed;

                    var orderDetails = await _unitOfWork.OrderDetails.FindAllAsync(od => od.OrderId == order.Id);
                    foreach (var orderDetail in orderDetails)
                    {
                        var product = await _unitOfWork.Products.FindAsync(p => p.Id == orderDetail.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += orderDetail.Quantity;
                            await _unitOfWork.Products.UpdateAsync(product);
                        }
                    }
                }

                await _unitOfWork.Orders.UpdateAsync(order);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        #endregion

        #region get orders with filter
        public async Task<List<OrderModel>> GetAllOrdersWithFilterAsync(OrderFilterModel filter)
        {
            var query = _unitOfWork.Orders.FindAll();
                

            if (filter.CustomerId.HasValue)
                query = query.Where(o => o.CustomerId == filter.CustomerId.Value);

            if (filter.OrderDate.HasValue)
                query = query.Where(o => o.OrderDate == filter.OrderDate.Value);

            if (filter.MinTotalAmount.HasValue)
                query = query.Where(o => o.TotalAmount >= filter.MinTotalAmount.Value);

            if (filter.MaxTotalAmount.HasValue)
                query = query.Where(o => o.TotalAmount <= filter.MaxTotalAmount.Value);

            if (!string.IsNullOrEmpty(filter.OrderStatus))
                query = query.Where(o => o.OrderStatus.Contains(filter.OrderStatus));

            // Áp dụng sắp xếp
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                switch (filter.SortBy)
                {
                    case nameof(OrderModel.OrderDate):
                        query = filter.IsAscending ? query.OrderBy(o => o.OrderDate) : query.OrderByDescending(o => o.OrderDate);
                        break;
                    case nameof(OrderModel.TotalAmount):
                        query = filter.IsAscending ? query.OrderBy(o => o.TotalAmount) : query.OrderByDescending(o => o.TotalAmount);
                        break;
                    case nameof(OrderModel.OrderStatus):
                        query = filter.IsAscending ? query.OrderBy(o => o.OrderStatus) : query.OrderByDescending(o => o.OrderStatus);
                        break;
                        // Thêm các trường khác nếu cần
                }
            }

            query = query.Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .Include(o => o.OrderHistories);

            var listOrders = await query.ToListAsync();

            if (listOrders != null)
            {
                var listOrderModel = _mapper.Map<List<OrderModel>>(listOrders);
                return listOrderModel;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
