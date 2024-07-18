using Newtonsoft.Json;
using SWD.F_LocalBrand.Business.Common.Shared;
using SWD.F_LocalBrand.Business.DTO.VNPay;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Business.Settings.VNPay;
using SWD.F_LocalBrand.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SWD.F_LocalBrand.Business.Attributes;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using Sprache;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace SWD.F_LocalBrand.Business.Services
{
    public class ProcessingWorker
    {
        //private readonly IUnitOfWork _unitOfWork;
        //private readonly IResponseCacheService _responseCacheService;
        //private readonly IMapper _mapper;
        //private readonly VNPayService _vnPayService;
        //private readonly VNPaySettings _vnPaySettings;

        //public ProcessingWorker(IUnitOfWork unitOfWork, IResponseCacheService responseCacheService, IMapper mapper, VNPayService vnPayService, IOptions<VNPaySettings> vnPaySettings)
        //{
        //    _unitOfWork = unitOfWork;
        //    _responseCacheService = responseCacheService;
        //    _mapper = mapper;
        //    _vnPayService = vnPayService;
        //    _vnPaySettings = vnPaySettings.Value;
        //}
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly NotificationService _notificationService;

        public ProcessingWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public async Task<(bool Success, string ErrorMessage, string PaymentUrl)> ProcessOrderPaymentAsync(OrderQueueItem orderQueueItem)
        {
            var scope = _scopeFactory.CreateScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var _responseCacheService = scope.ServiceProvider.GetRequiredService<IResponseCacheService>();
            var _mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            var _vnPayService = scope.ServiceProvider.GetRequiredService<VNPayService>();
            var _vnPaySettings = scope.ServiceProvider.GetRequiredService<IOptions<VNPaySettings>>().Value;
            var _notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
            var _productService = scope.ServiceProvider.GetRequiredService<ProductService>();


            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var requiredCategories = new List<string> { "Quần", "Áo", "Giày" };
                var categoryCount = new Dictionary<string, int> { { "Quần", 0 }, { "Áo", 0 }, { "Giày", 0 } };

                foreach (var productCheck in orderQueueItem.Products)
                {
                    var productModelCheck = await _unitOfWork.Products.FindByCondition(p => p.Id == productCheck.ProductId && p.Status == ProductStatusEnum.Active)
                        .Include(p => p.Category)
                        .FirstOrDefaultAsync();

                    if (productModelCheck != null)
                    {
                        var categoryName = productModelCheck.Category.CategoryName;
                        if (categoryCount.ContainsKey(categoryName))
                        {
                            categoryCount[categoryName]++;
                        }
                    }
                    else
                    {
                        return (false, $"Product with ID {productCheck.ProductId} not found in database", null);
                    }
                }

                // Tạo danh sách các danh mục thiếu
                var missingCategories = requiredCategories.Where(c => categoryCount[c] == 0).ToList();

                if (missingCategories.Any())
                {
                    var missingCategoriesString = string.Join(", ", missingCategories);
                    return (false, $"The order does not contain all required categories. Missing: {missingCategoriesString}.", null);
                }
                decimal totalAmount = 0m;
                var productEntities = new List<ProductModel>();

                foreach (var product in orderQueueItem.Products)
                {
                    var cacheKey = $"product:{product.ProductId}";
                    var cachedProduct = await _responseCacheService.GetCachedResponseAsync(cacheKey);

                    ProductModel productEntity;
                    if (cachedProduct != null)
                    {
                        while (cachedProduct.StartsWith("\"") && cachedProduct.EndsWith("\""))
                        {
                            cachedProduct = JsonConvert.DeserializeObject<string>(cachedProduct);
                        }
                        productEntity = JsonConvert.DeserializeObject<ProductModel>(cachedProduct);
                        if (productEntity == null)
                        {
                            return (false, $"Product with ID {product.ProductId} not found in cache", null);
                        }
                    }
                    else
                    {
                        var productModel = await _unitOfWork.Products.FindByCondition(p => p.Id == product.ProductId)
                            .Include(p => p.CompapilityProducts)
                                .ThenInclude(cp => cp.RecommendedProduct)
                            .Include(p => p.CollectionProducts)
                                .ThenInclude(cp => cp.Collection)
                            .AsSplitQuery() // Tách truy vấn thành nhiều truy vấn nhỏ để tối ưu hiệu suất
                            .FirstOrDefaultAsync(); ;
                        if (productModel == null)
                        {
                            return (false, $"Product with ID {product.ProductId} not found in database", null);
                        }
                        var visitedProducts = new HashSet<int>();
                        var recommendations = new List<Product>();

                        // Gọi hàm đệ quy để lấy tất cả các sản phẩm được đề xuất
                        _productService.GetRecommendations(productModel, visitedProducts, recommendations);
                        productEntity = _mapper.Map<ProductModel>(productModel);
                        productEntity.Recommendations = _mapper.Map<List<ProductModel>>(recommendations.DistinctBy(p => p.Id).ToList());
                        productEntity.Collections = _mapper.Map<List<CollectionModel>>(productModel.CollectionProducts.Select(cp => cp.Collection).ToList());
                    }

                    if (productEntity.StockQuantity < product.Quantity)
                    {
                        return (false, $"Product with ID {product.ProductId} does not have enough stock", null);
                    }

                    totalAmount += product.Quantity * productEntity.Price.GetValueOrDefault();
                    productEntity.StockQuantity -= product.Quantity;

                    productEntities.Add(productEntity);

                    var productUpdate = new Product
                    {
                        Id = productEntity.Id, // Thêm ID vào Product để cập nhật chính xác
                        ProductName = productEntity.ProductName,
                        CategoryId = productEntity.CategoryId,
                        CampaignId = productEntity.CampaignId,
                        Gender = productEntity.Gender,
                        Price = productEntity.Price.GetValueOrDefault(),
                        Description = productEntity.Description,
                        StockQuantity = productEntity.StockQuantity,
                        ImageUrl = productEntity.ImageUrl,
                        Size = productEntity.Size,
                        Color = productEntity.Color,
                        Status = productEntity.Status
                    };

                    await _unitOfWork.Products.UpdateAsync(productUpdate);
                    await _responseCacheService.SetCacheResponseAsync(cacheKey, productEntity, TimeSpan.FromMinutes(30));
                }

                var order = new Order
                {
                    CustomerId = orderQueueItem.CustomerId,
                    OrderDate = DateOnly.FromDateTime(DateTime.Now),
                    TotalAmount = totalAmount,
                    OrderStatus = OrderStatusTypeEnum.Pending
                };

                await _unitOfWork.Orders.CreateAsync(order);
                await _unitOfWork.CommitAsync();


                for (int i = 0; i < orderQueueItem.Products.Count; i++)
                {
                    var product = orderQueueItem.Products[i];
                    var productEntity = productEntities[i];

                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = product.ProductId,
                        Quantity = product.Quantity,
                        Price = productEntity.Price.GetValueOrDefault()
                    };

                    await _unitOfWork.OrderDetails.CreateAsync(orderDetail);
                }

                order.TotalAmount = totalAmount;
                await _unitOfWork.Orders.UpdateAsync(order);

                var payment = new Payment
                {
                    OrderId = order.Id,
                    PaymentDate = DateOnly.FromDateTime(DateTime.Now),
                    PaymentMethod = orderQueueItem.PaymentMethod,
                    PaymentStatus = PaymentStatusTypeEnum.Pending
                };

                await _unitOfWork.Payments.CreateAsync(payment);
                var res = await _unitOfWork.CommitAsync();

                // create payment url
                var paymentUrl = string.Empty;
                if (res > 0)
                {
                    var vnPayRequest = new CreateVNPayModel(_vnPaySettings.Version,
                    _vnPaySettings.TmnCode, DateTime.Now, "127.0.0.1" ?? string.Empty, order.TotalAmount ?? 0, "VND",
                        "other", $"Thanh toan don hang {order.Id}", _vnPaySettings.ReturnUrl, order.Id!.ToString() ?? string.Empty);
                    paymentUrl = _vnPayService.GetLink(_vnPaySettings.PaymentUrl, _vnPaySettings.HashSecret, vnPayRequest);
                    Console.WriteLine(paymentUrl);
                }
                return (true, null, paymentUrl);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return (false, ex.Message, null);
            }
        }
    }
}
