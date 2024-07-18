using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SWD.F_LocalBrand.Business.Attributes;
using Microsoft.Extensions.Options;
using SWD.F_LocalBrand.Business.Common.Shared;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Business.DTO.Cart;
using SWD.F_LocalBrand.Business.DTO.Order;
using SWD.F_LocalBrand.Business.DTO.VNPay;
using SWD.F_LocalBrand.Business.Settings.VNPay;
using SWD.F_LocalBrand.Business.DTO.Report;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Data.Models;
using SWD.F_LocalBrand.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWD.F_LocalBrand.Business.DTO.Product;
using StackExchange.Redis;

namespace SWD.F_LocalBrand.Business.Services
{
    public class OrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ProductService _productService;
        private readonly RedisQueueService _queueService;
        private readonly IResponseCacheService _responseCacheService;
        private readonly VNPaySettings _vnPaySettings;
        private readonly VNPayService _vnPayService;
        private readonly NotificationService _notificationService;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ProductService productService, RedisQueueService queueService, IResponseCacheService responseCacheService,IOptions<VNPaySettings> vnPaySettings, VNPayService vnPayService, NotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _productService = productService;
            _queueService = queueService;
            _responseCacheService = responseCacheService;
            _vnPaySettings = vnPaySettings.Value;
            _vnPayService = vnPayService;
            _notificationService = notificationService;
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
        //public async Task<string> CreateOrderAsync(int customerId, List<CartProductModel> products, string paymentMethod)
        //{
        //    await _unitOfWork.BeginTransactionAsync();
        //    try
        //    {
        //        var order = new Order
        //        {
        //            CustomerId = customerId,
        //            OrderDate = DateOnly.FromDateTime(DateTime.Now),
        //            TotalAmount = 0m,
        //            OrderStatus = OrderStatusTypeEnum.Pending
        //        };

        //        await _unitOfWork.Orders.CreateAsync(order);
        //        await _unitOfWork.CommitAsync();

        //        decimal totalAmount = 0m;

        //        foreach (var product in products)
        //        {
        //            var productEntity = await _unitOfWork.Products.FindAsync(p => p.Id == product.ProductId);
        //            if (productEntity == null)
        //            {
        //                throw new Exception($"Product with ID {product.ProductId} not found");
        //            }

        //            var orderDetail = new OrderDetail
        //            {
        //                OrderId = order.Id,
        //                ProductId = product.ProductId,
        //                Quantity = product.Quantity,
        //                Price = productEntity.Price
        //            };

        //            totalAmount += product.Quantity * productEntity.Price;
        //            productEntity.StockQuantity -= product.Quantity;

        //            await _unitOfWork.OrderDetails.CreateAsync(orderDetail);
        //            await _unitOfWork.Products.UpdateAsync(productEntity);
        //        }

        //        order.TotalAmount = totalAmount;
        //        await _unitOfWork.Orders.UpdateAsync(order);

        //        var payment = new Payment
        //        {
        //            OrderId = order.Id,
        //            PaymentDate = DateOnly.FromDateTime(DateTime.Now),
        //            PaymentMethod = paymentMethod,
        //            PaymentStatus = PaymentStatusTypeEnum.Pending
        //        };

        //        await _unitOfWork.Payments.CreateAsync(payment);
        //        var res = await _unitOfWork.CommitAsync();

        //        var paymentUrl = string.Empty;
        //        if (res > 0)
        //        {
        //            var vnPayRequest = new CreateVNPayModel(_vnPaySettings.Version,
        //                _vnPaySettings.TmnCode, DateTime.Now, "127.0.0.1" ?? string.Empty, order.TotalAmount ?? 0, "VNĐ",
        //                "other", $"Thanh toan don hang {order.Id}", _vnPaySettings.ReturnUrl, order.Id!.ToString() ?? string.Empty);
        //            paymentUrl = _vnPayService.GetLink(_vnPaySettings.PaymentUrl, _vnPaySettings.HashSecret, vnPayRequest);
        //            return paymentUrl;
        //        }
        //        return "Something wrong!";
        //    }
        //    catch
        //    {
        //        await _unitOfWork.RollbackAsync();
        //        throw;
        //    }
        //}
        #endregion


        #region update payment 
        public async Task<string> UpdatePaymentStatusAsync(UpdateVNPayModel updateVNPayModel)
        {
            if (updateVNPayModel == null)
            {
                return "Input data required"; // "RspCode":"99"
            }

            if (!_vnPayService.IsValidSignature(_vnPaySettings.HashSecret, updateVNPayModel))
            {
                return "Invalid signature"; // "RspCode":"97"
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await _unitOfWork.Orders.FindByCondition(o => o.Id == int.Parse(updateVNPayModel.vnp_TxnRef)).FirstOrDefaultAsync();
                if (order == null)
                {
                    throw new Exception("Order not found");
                }

                var paymentCheck = await _unitOfWork.Payments.FindByCondition(p => p.OrderId == order.Id).FirstOrDefaultAsync();
                if (paymentCheck == null)
                {
                    return "Payment not found"; // "RspCode":"01"
                }

                if (order.TotalAmount != (updateVNPayModel.vnp_Amount / 100))
                {
                    paymentCheck.Vnd_CardType = updateVNPayModel.vnp_CardType;
                    paymentCheck.Vnp_BankCode = updateVNPayModel.vnp_BankCode;
                    paymentCheck.Vnp_BankTranNo = updateVNPayModel.vnp_BankTranNo;
                    paymentCheck.Vnp_ResponseCode = updateVNPayModel.vnp_ResponseCode;
                    paymentCheck.Vnp_TransactionStatus = updateVNPayModel.vnp_TransactionStatus;
                    paymentCheck.Vnp_TxnRef = updateVNPayModel.vnp_TxnRef;
                    paymentCheck.PaymentStatus = PaymentStatusTypeEnum.Failed;
                    await _unitOfWork.Payments.UpdateAsync(paymentCheck);
                    await _unitOfWork.CommitAsync(); // Commit changes including the payment update
                    return "Amount mismatch"; // "RspCode":"04"
                }

                if (order.OrderStatus != PaymentStatusTypeEnum.Pending)
                {
                    return "Payment in order already confirmed"; // "RspCode":"02"
                }

                // Update order status based on VNPay response
                if (updateVNPayModel.vnp_ResponseCode == "00" && updateVNPayModel.vnp_TransactionStatus == "00")
                {
                    order.OrderStatus = OrderStatusTypeEnum.Completed;
                    paymentCheck.Vnd_CardType = updateVNPayModel.vnp_CardType;
                    paymentCheck.Vnp_BankCode = updateVNPayModel.vnp_BankCode;
                    paymentCheck.Vnp_BankTranNo = updateVNPayModel.vnp_BankTranNo;
                    paymentCheck.Vnp_ResponseCode = updateVNPayModel.vnp_ResponseCode;
                    paymentCheck.Vnp_TransactionStatus = updateVNPayModel.vnp_TransactionStatus;
                    paymentCheck.Vnp_TxnRef = updateVNPayModel.vnp_TxnRef;
                    paymentCheck.PaymentStatus = PaymentStatusTypeEnum.Completed;
                    var orderHistory = new OrderHistory
                    {
                        OrderId = order.Id,
                        Status = OrderHistoryStatusTypeEnum.Preparing,
                        ChangeTime = DateTime.Now,
                        Description = "Order is being prepared",
                        IsCurrent = true
                    };

                    await _unitOfWork.OrderHistories.CreateAsync(orderHistory);

                    // Send notification to Admin
                    var role = await _unitOfWork.Roles.FindByCondition(r => r.RoleName == "Admin").FirstOrDefaultAsync();
                    var userList = await _unitOfWork.Users.FindAll().Where(u => u.RoleId == role.Id).ToListAsync();
                    foreach (var user in userList)
                    {
                        if (user.DeviceId != null)
                            await _notificationService.SendNotification(user.DeviceId, "F-LocalBrand", $"Have order by {order.CustomerId}, please check and prepare!");
                    }
                    var customer = await _unitOfWork.Customers.FindByCondition(c => c.Id == order.CustomerId.GetValueOrDefault()).FirstOrDefaultAsync();
                    await _notificationService.PushNotificationToRedis(order.CustomerId.GetValueOrDefault(), order.Id, $"Order {order.Id} is created", "Preparing", customer.FullName, customer.Image);
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
                    await _unitOfWork.Orders.UpdateAsync(order);
                    await _unitOfWork.Payments.UpdateAsync(paymentCheck);
                    await _unitOfWork.CommitAsync(); // Commit all changes
                    return "Confirm Success"; // "RspCode":"00"
                }
                else
                {
                    order.OrderStatus = OrderStatusTypeEnum.Failed;
                    paymentCheck.Vnd_CardType = updateVNPayModel.vnp_CardType;
                    paymentCheck.Vnp_BankCode = updateVNPayModel.vnp_BankCode;
                    paymentCheck.Vnp_BankTranNo = updateVNPayModel.vnp_BankTranNo;
                    paymentCheck.Vnp_ResponseCode = updateVNPayModel.vnp_ResponseCode;
                    paymentCheck.Vnp_TransactionStatus = updateVNPayModel.vnp_TransactionStatus;
                    paymentCheck.Vnp_TxnRef = updateVNPayModel.vnp_TxnRef;
                    paymentCheck.PaymentStatus = PaymentStatusTypeEnum.Failed;
                    var customer = await _unitOfWork.Customers.FindByCondition(c => c.Id == order.CustomerId.GetValueOrDefault()).FirstOrDefaultAsync();
                    await _unitOfWork.Orders.UpdateAsync(order);
                    await _unitOfWork.Payments.UpdateAsync(paymentCheck);
                    await _unitOfWork.CommitAsync(); // Commit changes including the order update

                    
                    await _notificationService.PushNotificationToRedis(order.CustomerId.GetValueOrDefault(), order.Id, $"Order {order.Id} is cancelled", "Cancelled", customer.FullName, customer.Image);
                    return "Có lỗi xảy ra trong quá trình xử lý"; // Error during payment processing
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                // Optionally log the exception here for debugging purposes
                throw; // Re-throwing the exception
            }
        }
        #endregion


        #region update status payment and order
        public async Task UpdateStatusPayymentAndOrder(int orderId)
        {
            var order = _unitOfWork.Orders.FindByCondition(o => o.Id == orderId).FirstOrDefault();
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


        #region get order filter for role shipper and customer
        public async Task<List<OrderModel>> GetOrdersWithFilterAsync(int? customerId, string statusOrderHistory)
        {

            var query = _unitOfWork.OrderHistories.FindByCondition(oh => oh.Status == statusOrderHistory);

            if (customerId.HasValue)
            {
                query = query.Where(oh => oh.Order.CustomerId == customerId.Value);
            }

            var orderIds = await query.Select(oh => oh.OrderId).Distinct().ToListAsync();
            var orders = _unitOfWork.Orders.FindByCondition(o => orderIds.Contains(o.Id))
                .Include(o => o.OrderHistories)
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .OrderByDescending(o => o.OrderDate);
            var listOrders = await orders.ToListAsync();
            var listOrderModel = _mapper.Map<List<OrderModel>>(listOrders);

            return listOrderModel;
        }
        #endregion


        #region create with payment with transaction and queue

        //public async Task<bool> CheckStockAvailabilityAsync(List<CartProductModel> products)
        //{
        //    foreach (var product in products)
        //    {
        //        var cacheKey = $"product:{product.ProductId}";
        //        var cachedProduct = await _responseCacheService.GetCachedResponseAsync(cacheKey);

        //        ProductModel productEntity;
        //        if (cachedProduct != null)
        //        {
        //            while (cachedProduct.StartsWith("\"") && cachedProduct.EndsWith("\""))
        //            {
        //                cachedProduct = JsonConvert.DeserializeObject<string>(cachedProduct);
        //            }
        //            productEntity = JsonConvert.DeserializeObject<ProductModel>(cachedProduct);
        //            if (productEntity == null)
        //            {
        //                throw new Exception($"Product with ID {product.ProductId} not found in cache");
        //            }
        //        }
        //        else
        //        {
        //            var productModel = await _unitOfWork.Products.FindAsync(p => p.Id == product.ProductId);
        //            if (productModel == null)
        //            {
        //                return false; // Product not found in database
        //            }
        //            productEntity = _mapper.Map<ProductModel>(productModel);
        //            await _responseCacheService.SetCacheResponseAsync(cacheKey, productEntity, TimeSpan.FromMinutes(30));
        //        }

        //        if (productEntity.StockQuantity < product.Quantity)
        //        {
        //            return false; // Insufficient stock
        //        }
        //    }

        //    return true;
        //}
        //public async Task CreateOrderQueueAsync(int customerId, List<CartProductModel> products, string paymentMethod)
        //{
        //    var orderQueueItem = new OrderQueueItem
        //    {
        //        CustomerId = customerId,
        //        Products = products,
        //        PaymentMethod = paymentMethod
        //    };

        //    await _queueService.EnqueueOrderAsync(orderQueueItem);
        //}
        //public async Task ProcessOrderAsync(OrderQueueItem orderQueueItem)
        //{
        //    await _unitOfWork.BeginTransactionAsync();
        //    try
        //    {
                
        //        // Tạo đối tượng Order
        //        var order = new Order
        //        {
        //            CustomerId = orderQueueItem.CustomerId,
        //            OrderDate = DateOnly.FromDateTime(DateTime.Now),
        //            TotalAmount = 0m,
        //            OrderStatus = OrderStatusTypeEnum.Pending
        //        };

        //        await _unitOfWork.Orders.CreateAsync(order);
        //        await _unitOfWork.CommitAsync();

        //        decimal totalAmount = 0m;

        //        // Xử lý từng sản phẩm trong đơn hàng
        //        foreach (var product in orderQueueItem.Products)
        //        {
        //            var cacheKey = $"product:{product.ProductId}";
        //            var cachedProduct = await _responseCacheService.GetCachedResponseAsync(cacheKey);

        //            ProductModel productEntity;
        //            if (cachedProduct != null)
        //            {
        //                while (cachedProduct.StartsWith("\"") && cachedProduct.EndsWith("\""))
        //                {
        //                    cachedProduct = JsonConvert.DeserializeObject<string>(cachedProduct);
        //                }
        //                productEntity = JsonConvert.DeserializeObject<ProductModel>(cachedProduct);
        //                if (productEntity == null)
        //                {
        //                    throw new Exception($"Product with ID {product.ProductId} not found in cache");
        //                }
        //            }
        //            else
        //            {
        //                var productModel = await _unitOfWork.Products.FindAsync(p => p.Id == product.ProductId);
        //                if (productModel == null)
        //                {
        //                    throw new Exception($"Product with ID {product.ProductId} not found in database");
        //                }
        //                productEntity = _mapper.Map<ProductModel>(productModel);
        //            }

        //            var orderDetail = new OrderDetail
        //            {
        //                OrderId = order.Id,
        //                ProductId = product.ProductId,
        //                Quantity = product.Quantity,
        //                Price = productEntity.Price.GetValueOrDefault()
        //            };

        //            totalAmount += product.Quantity * productEntity.Price.GetValueOrDefault();
        //            productEntity.StockQuantity -= product.Quantity;

        //            var productUpdate = new Product
        //            {
        //                Id = productEntity.Id, // Thêm ID vào Product để cập nhật chính xác
        //                ProductName = productEntity.ProductName,
        //                CategoryId = productEntity.CategoryId,
        //                CampaignId = productEntity.CampaignId,
        //                Gender = productEntity.Gender,
        //                Price = productEntity.Price.GetValueOrDefault(),
        //                Description = productEntity.Description,
        //                StockQuantity = productEntity.StockQuantity,
        //                ImageUrl = productEntity.ImageUrl,
        //                Size = productEntity.Size,
        //                Color = productEntity.Color,
        //                Status = productEntity.Status
        //            };

        //            await _unitOfWork.OrderDetails.CreateAsync(orderDetail);
        //            await _unitOfWork.Products.UpdateAsync(productUpdate);

        //            await _responseCacheService.SetCacheResponseAsync(cacheKey, productEntity, TimeSpan.FromMinutes(30));
        //        }

        //        order.TotalAmount = totalAmount;
        //        await _unitOfWork.Orders.UpdateAsync(order);

        //        var payment = new Payment
        //        {
        //            OrderId = order.Id,
        //            PaymentDate = DateOnly.FromDateTime(DateTime.Now),
        //            PaymentMethod = orderQueueItem.PaymentMethod,
        //            PaymentStatus = PaymentStatusTypeEnum.Pending
        //        };

        //        await _unitOfWork.Payments.CreateAsync(payment);
        //        await _unitOfWork.CommitAsync();
        //    }
        //    catch
        //    {
        //        await _unitOfWork.RollbackAsync();
        //        throw;
        //    }
        //}

        //public async Task ProcessOrderAsync()
        //{
        //    var orderQueueItem = await _queueService.DequeueOrderAsync();
        //    if (orderQueueItem == null) return;

        //    await _unitOfWork.BeginTransactionAsync();
        //    try
        //    {
        //        var order = new Order
        //        {
        //            CustomerId = orderQueueItem.CustomerId,
        //            OrderDate = DateOnly.FromDateTime(DateTime.Now),
        //            TotalAmount = 0m,
        //            OrderStatus = OrderStatusTypeEnum.Pending
        //        };

        //        await _unitOfWork.Orders.CreateAsync(order);
        //        await _unitOfWork.CommitAsync();

        //        var totalAmount = 0m;

        //        foreach (var product in orderQueueItem.Products)
        //        {
        //            var cacheKey = $"product:{product.ProductId}";
        //            var cachedProduct = await _responseCacheService.GetCachedResponseAsync(cacheKey);

        //            ProductModel productEntity;
        //            if (cachedProduct != null)
        //            {
        //                while (cachedProduct.StartsWith("\"") && cachedProduct.EndsWith("\""))
        //                {
        //                    cachedProduct = JsonConvert.DeserializeObject<string>(cachedProduct);
        //                }
        //                productEntity = JsonConvert.DeserializeObject<ProductModel>(cachedProduct);
        //                if (productEntity == null)
        //                {
        //                    throw new Exception($"Product with ID {product.ProductId} not found in cache");
        //                }
        //            }
        //            else
        //            {
        //                var  productModel = await _unitOfWork.Products.FindAsync(p => p.Id == product.ProductId);
        //                if (productModel == null)
        //                {
        //                    throw new Exception($"Product with ID {product.ProductId} not found in database");
        //                }
        //                productEntity = _mapper.Map<ProductModel>(productModel);
        //            }

        //            var orderDetail = new OrderDetail
        //            {
        //                OrderId = order.Id,
        //                ProductId = product.ProductId,
        //                Quantity = product.Quantity,
        //                Price = productEntity.Price
        //            };

        //            totalAmount += product.Quantity * productEntity.Price.GetValueOrDefault();
        //            productEntity.StockQuantity -= product.Quantity;

        //            var productUpdate = new Product
        //            {
        //                ProductName = productEntity.ProductName,
        //                CategoryId = productEntity.CategoryId,
        //                CampaignId = productEntity.CampaignId,
        //                Gender = productEntity.Gender,
        //                Price = productEntity.Price.GetValueOrDefault(),
        //                Description = productEntity.Description,
        //                StockQuantity = productEntity.StockQuantity,
        //                ImageUrl = productEntity.ImageUrl,
        //                Size = productEntity.Size,
        //                Color = productEntity.Color,
        //                Status = productEntity.Status
        //            };

        //            await _unitOfWork.OrderDetails.CreateAsync(orderDetail);
        //            await _unitOfWork.Products.UpdateAsync(productUpdate);

        //            await _responseCacheService.SetCacheResponseAsync(cacheKey, productEntity, TimeSpan.FromMinutes(30));
        //        }

        //        order.TotalAmount = totalAmount;
        //        await _unitOfWork.Orders.UpdateAsync(order);

        //        var payment = new Payment
        //        {
        //            OrderId = order.Id,
        //            PaymentDate = DateOnly.FromDateTime(DateTime.Now),
        //            PaymentMethod = orderQueueItem.PaymentMethod,
        //            PaymentStatus = PaymentStatusTypeEnum.Pending
        //        };

        //        await _unitOfWork.Payments.CreateAsync(payment);
        //        await _unitOfWork.CommitAsync();
        //    }
        //    catch
        //    {
        //        await _unitOfWork.RollbackAsync();
        //        throw;
        //    }
        //}
        #endregion


        #region create order with payment with transaction payment
        public async Task<(bool Success, string ErrorMessage, string PaymentUrl)> CreateOrderQueuePaymentAsync(int customerId, List<CartProductModel> products, string paymentMethod)
        {
            var orderQueueItem = new OrderQueueItem
            {
                CustomerId = customerId,
                Products = products,
                PaymentMethod = paymentMethod
            };

            await _queueService.EnqueueOrderAsync(orderQueueItem);
            return await _queueService.ProcessOrderPaymentAsync(orderQueueItem);
        }
        #endregion


        #region  get daily report data async

        public async Task<ReportData> GetDailyReportDataAsync(DateOnly reportDate)
        {
            var startDate = reportDate;
            var endDate = startDate.AddDays(1);

            var totalOrders = await GetTotalOrdersAsync(startDate, endDate);
            var totalRevenue = await GetTotalRevenueAsync(startDate, endDate);
            var orderStatusCounts = await GetOrderStatusCountsAsync(startDate, endDate);
            var paymentStatusCounts = await GetPaymentStatusCountsAsync(startDate, endDate);
            var shippingStatusCounts = await GetShippingStatusCountsAsync(DateTime.Now.Date, DateTime.Now.Date.AddDays(1));
            var topSellingProducts = await GetTopSellingProductsAsync(startDate, endDate);

            return new ReportData
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                OrderStatusCounts = orderStatusCounts,
                TopSellingProducts = topSellingProducts,
                PaymentStatusCounts = paymentStatusCounts,
                ShippingStatusCounts = shippingStatusCounts
            };
        }
        public async Task<int> GetTotalOrdersAsync(DateOnly startDate, DateOnly endDate)
        {
            return await _unitOfWork.Orders
                .FindByCondition(o => o.OrderDate >= startDate && o.OrderDate < endDate)
                .CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateOnly startDate, DateOnly endDate)
        {
            return await _unitOfWork.Orders
                .FindByCondition(o => o.OrderDate >= startDate && o.OrderDate < endDate && o.OrderStatus.Equals(OrderStatusTypeEnum.Completed))
                .SumAsync(o => o.TotalAmount ?? 0);
        }

        public async Task<Dictionary<string, int>> GetOrderStatusCountsAsync(DateOnly startDate, DateOnly endDate)
        {
            return await _unitOfWork.Orders
                .FindByCondition(o => o.OrderDate >= startDate && o.OrderDate < endDate)
                .GroupBy(o => o.OrderStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Status, g => g.Count);
        }

        public async Task<Dictionary<string, int>> GetPaymentStatusCountsAsync(DateOnly startDate, DateOnly endDate)
        {
            return await _unitOfWork.Payments
                .FindByCondition(p => p.PaymentDate >= startDate && p.PaymentDate < endDate)
                .GroupBy(p => p.PaymentStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Status, g => g.Count);
        }

        public async Task<Dictionary<string, int>> GetShippingStatusCountsAsync(DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.OrderHistories
                .FindByCondition(h => h.ChangeTime >= startDate && h.ChangeTime < endDate&&h.IsCurrent==true)
                .GroupBy(h => h.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Status, g => g.Count);
        }

        public async Task<List<string>> GetTopSellingProductsAsync(DateOnly startDate, DateOnly endDate)
        {
            return await _unitOfWork.OrderDetails
                .FindByCondition(d => d.Order.OrderDate >= startDate && d.Order.OrderDate < endDate)
                .GroupBy(d => d.Product.ProductName)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(5)
                .ToListAsync();
        }
        #endregion


        #region get order in progress
        public async Task<List<InProgressOrderResponseModel>> GetInProgressOrdersAsync(int customerId)
        {
            var inProgressStatuses = new List<string>
            {
                OrderHistoryStatusTypeEnum.Preparing,
                OrderHistoryStatusTypeEnum.Prepared,
                OrderHistoryStatusTypeEnum.ShipperReceived,
                OrderHistoryStatusTypeEnum.InTransit
            };

            
                var orders = await _unitOfWork.Orders
                    .FindOrderAsync(o => o.CustomerId == customerId && o.OrderStatus == OrderStatusTypeEnum.Completed);

                var inProgressOrders = orders
                    .Where(o => o.OrderHistories.Any(oh => oh.IsCurrent && inProgressStatuses.Contains(oh.Status)))
                    .Select(o => new InProgressOrderResponseModel
                    {
                        TotalItem=o.OrderDetails.Count,
                        OrderId = o.Id,
                        TotalAmount = o.TotalAmount ?? 0,
                        OrderStatus = o.OrderStatus,
                        OrderDate=(DateOnly)o.OrderDate,
                        CurrentStatus = o.OrderHistories
                            .Where(oh => oh.IsCurrent && inProgressStatuses.Contains(oh.Status))
                            .Select(oh => oh.Status)
                            .FirstOrDefault(),
                        ChangeTime =(DateTime) o.OrderHistories
                            .Where(oh => oh.IsCurrent && inProgressStatuses.Contains(oh.Status))
                            .Select(oh => oh.ChangeTime)
                            .FirstOrDefault()
                    })
                    .ToList();

                return inProgressOrders;
            
            
        }


        #endregion


        #region get detail of order

        public async Task<OrderResponseModel> GetOrderDetailsByOrderIdAsync(int orderId)
        {
            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with Id {orderId} not found.");
            }

            var statusHistory = new Dictionary<string, DateTime?>
    {
        { OrderHistoryStatusTypeEnum.Preparing, null },
        { OrderHistoryStatusTypeEnum.Prepared, null },
        { OrderHistoryStatusTypeEnum.ShipperReceived, null },
        { OrderHistoryStatusTypeEnum.InTransit, null },
        { OrderHistoryStatusTypeEnum.Delivered, null },
        { OrderHistoryStatusTypeEnum.Cancelled, null }
    };
            foreach (var history in order.OrderHistories)
            {
                if (statusHistory.ContainsKey(history.Status))
                {
                    statusHistory[history.Status] = history.ChangeTime;
                }
            }

            // Convert the dictionary to the desired string format
            var statusHistoryString = string.Join(",", statusHistory.Select(kvp =>
                $"{kvp.Key}:{(kvp.Value.HasValue ? kvp.Value.Value.ToString("yyyy-MM-ddTHH:mm:ss") : "N/A")}"));

        

            var orderResponse = new OrderResponseModel
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                StatusHistory = statusHistoryString,
                Details = order.OrderDetails.Select(od => new OrderDetailResponseModel
                {
                    Quantity = od.Quantity ?? 0,
                    Price = od.Price ?? 0,
                    Product = new ProductWithInfoModel
                    {
                        Id = od.Product.Id,
                        ProductName = od.Product.ProductName,
                        CategoryId = od.Product.CategoryId,
                        CampaignId = od.Product.CampaignId,
                        Gender = od.Product.Gender,
                        Price = od.Product.Price,
                        Description = od.Product.Description,
                        StockQuantity = od.Product.StockQuantity,
                        ImageUrl = od.Product.ImageUrl,
                        Size = od.Product.Size,
                        Color = od.Product.Color,
                        CreateDate = od.Product.CreateDate
                    }
                }).ToList()
            };


            return orderResponse;
        }

        #endregion


        #region get order by order history status
        public async Task<List<OrderModel>> GetOrderByOrderHistoryStatus(OrderHistoryStatusModel orderHistoryStatusModel)
        {
            try
            {
                var listOrderHistory = await _unitOfWork.OrderHistories.FindByCondition(oh => oh.IsCurrent && oh.Status == orderHistoryStatusModel.Status).ToListAsync();
                var listOrders = new List<OrderModel>();
                foreach (var orderHistory in listOrderHistory)
                {
                    var order = await _unitOfWork.Orders.FindByCondition(o => o.Id == orderHistory.OrderId.GetValueOrDefault()).FirstOrDefaultAsync();
                    if (order != null)
                    {
                        var orderMapper = _mapper.Map<OrderModel>(order);
                        listOrders.Add(orderMapper);
                    }
                }
                if (orderHistoryStatusModel.CustomerId != null)
                {
                    listOrders = listOrders.Where(lo => lo.CustomerId == orderHistoryStatusModel.CustomerId).ToList();
                }
                return listOrders;
            }
            catch (Exception ex)
            {
                return new List<OrderModel>();
            }
            
        }
        #endregion
    }
}
