using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SWD.F_LocalBrand.Business.Attributes;
using SWD.F_LocalBrand.Business.Common.Shared;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Business.DTO.Campaign;
using SWD.F_LocalBrand.Business.DTO.Category;
using SWD.F_LocalBrand.Business.DTO.Product;
using SWD.F_LocalBrand.Business.Utils;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Data.Models;


namespace SWD.F_LocalBrand.Business.Services
{
    public class ProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IResponseCacheService _cache;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, IResponseCacheService cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
        }


        #region Get all products have paging and include all table related

        public async Task<List<ProductWithAllRelatedModel>> GetAllProductsAsync(int pageNumber, int pageSize)
        {

            var listProducts = await _unitOfWork.Products
                                                .FindAll(true)
                                                .Include(x => x.Category)
                                                .Include(x => x.Campaign)
                                                .Include(x => x.CollectionProducts)                                               
                                                .Include(x => x.CompapilityProducts)                                              
                                                .ToListAsync();
            var listProductsPage =(List<Product>) PaginationUtils.Paginate(listProducts, pageNumber,pageSize);
            var listProductReturn= _mapper.Map<List<ProductWithAllRelatedModel>>(listProductsPage);
            for(int i = 0; i < listProductReturn.Count; i++)
            {
                var product = listProductsPage[i];
                var campaginReturn = _mapper.Map<CampaignWithInfoModel>(product.Campaign);
                var categoryReturn = _mapper.Map<CategoryWithInfoModel>(product.Category);
                var listProductRecommendationsReturn = _mapper.Map <List<ProductWithInfoModel>>(product.CompapilityProducts.Select(x=>x.RecommendedProduct));

                listProductReturn[i].Campaign=campaginReturn;
                listProductReturn[i].Category = categoryReturn;
                listProductReturn[i].ProductsRecommendation = listProductRecommendationsReturn;



            }

            return listProductReturn;

        }

        #endregion



        //get all product with entities relate
        public async Task<List<ProductModel>> GetAllProductsAsync()
        {
            var listProducts = await _unitOfWork.Products.FindAll().ToListAsync();
            if(listProducts != null)
            {
                var listProductModel = _mapper.Map<List<ProductModel>>(listProducts);
                return listProductModel;
            }
            else
            {
                return null;
            }
            
        }

        

        #region Get product by id and compapility of them ( only get product by id and recommend of them, do not have reverse)
        public async Task<ProductModel?> GetProductWithRecommendationsAsync(int productId)
        {
            var cacheKey = $"product:{productId}";
            var cachedProduct = await _cache.GetCachedResponseAsync(cacheKey);
            if (cachedProduct != null)
            {
                try
                {
                    // Check if the string needs multiple deserializations
                    while (cachedProduct.StartsWith("\"") && cachedProduct.EndsWith("\""))
                    {
                        cachedProduct = JsonConvert.DeserializeObject<string>(cachedProduct);
                    }

                    var deserializedProduct = JsonConvert.DeserializeObject<ProductModel>(cachedProduct);
                    if (deserializedProduct == null)
                    {
                        throw new Exception("Deserialization resulted in null.");
                    }
                    return deserializedProduct;
                }
                catch (JsonSerializationException ex)
                {
                    // Log or handle the exception
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
            // Tải sản phẩm cùng với các sản phẩm được đề xuất và các bộ sưu tập liên quan
            var product = await _unitOfWork.Products.FindByCondition(
                p => p.Id == productId,
                trackChanges: false,
                includeProperties: p => p.CompapilityProducts)
                .Include(p => p.CompapilityProducts)
                    .ThenInclude(cp => cp.RecommendedProduct)
                .Include(p => p.CollectionProducts)
                    .ThenInclude(cp => cp.Collection)
                .AsSplitQuery() // Tách truy vấn thành nhiều truy vấn nhỏ để tối ưu hiệu suất
                .FirstOrDefaultAsync();

            if (product == null) return null;

            // Sử dụng HashSet để tránh lặp lại các sản phẩm đã kiểm tra
            var visitedProducts = new HashSet<int>();
            var recommendations = new List<Product>();

            // Gọi hàm đệ quy để lấy tất cả các sản phẩm được đề xuất
            GetRecommendations(product, visitedProducts, recommendations);

            // Map các dữ liệu sản phẩm sang ProductModel
            var productModel = _mapper.Map<ProductModel>(product);
            productModel.Recommendations = _mapper.Map<List<ProductModel>>(recommendations.DistinctBy(p => p.Id).ToList());
            productModel.Collections = _mapper.Map<List<CollectionModel>>(product.CollectionProducts.Select(cp => cp.Collection).ToList());
            var serializedProduct = JsonConvert.SerializeObject(productModel);
            await _cache.SetCacheResponseAsync(cacheKey, serializedProduct, TimeSpan.FromMinutes(30));



            return productModel;
        }

        private void GetRecommendations(Product product, HashSet<int> visitedProducts, List<Product> recommendations)
        {
            if (product == null || visitedProducts.Contains(product.Id)) return;

            visitedProducts.Add(product.Id);

            foreach (var compapility in product.CompapilityProducts)
            {
                var recommendedProduct = compapility.RecommendedProduct;
                if (recommendedProduct != null && !visitedProducts.Contains(recommendedProduct.Id))
                {
                    recommendations.Add(recommendedProduct);
                    GetRecommendations(recommendedProduct, visitedProducts, recommendations);
                }
            }
        }
        #endregion

        
        #region create product

        public async Task<int> CreateProductAsync(ProductCreateModel model)
        {
            // Map ProductCreateModel to Product entity
            var product = new Product
            {
                ProductName = model.ProductName,
                CategoryId = model.CategoryId,
                CampaignId = model.CampaignId,
                Gender = model.Gender,
                Price = model.Price,
                Description = model.Description,
                StockQuantity = model.StockQuantity,
                ImageUrl = model.ImageUrl,
                Size = model.Size,
                Color = model.Color,
                Status = "Inactive"
            };

            // Create product using UnitOfWork
            await _unitOfWork.Products.CreateAsync(product);
            await _unitOfWork.CommitAsync();

            return product.Id;
        }

        #endregion


        #region update product detail
        public async Task<ProductUpdateModel?> UpdateProductAsync(ProductUpdateModel model)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(model.Id);
            if (product == null)
            {
                return null;
            }

            product.ProductName = model.ProductName;
            product.CategoryId = model.CategoryId;
            product.CampaignId = model.CampaignId;
            product.Gender = model.Gender;
            product.Price = model.Price;
            product.Description = model.Description;
            product.StockQuantity = model.StockQuantity;
            product.ImageUrl = model.ImageUrl;
            product.Size = model.Size;
            product.Color = model.Color;
            product.Status = model.Status;

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.CommitAsync();

            return model;
        }

        #endregion


        #region deleted product by changed status

        public async Task DeleteProductAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                throw new EntryPointNotFoundException("Product not found");
            }

            product.Status =ProductStatusEnum.Deleted;

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.CommitAsync();
        }
        #endregion


        #region add list product recommend for product 
        public async Task AddRecommendedProductsAsync(int productId, List<int> recommendedProductIds)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                throw new EntryPointNotFoundException("Product not found");
            }

            foreach (var recommendedProductId in recommendedProductIds)
            {
                var recommendedProduct = await _unitOfWork.Products.GetByIdAsync(recommendedProductId);
                if (recommendedProduct == null)
                {
                    throw new EntryPointNotFoundException($"Recommended product with ID {recommendedProductId} not found");
                }

                var compapility = new Compapility
                {
                    ProductId = productId,
                    RecommendedProductId = recommendedProductId
                };

                await _unitOfWork.Compapilities.CreateAsync(compapility);
            }

            await _unitOfWork.CommitAsync();
        }
        #endregion


        #region get list product which best seller
        public async Task<List<ProductModel>> GetBestSellerProductsAsync(int limit)
        {
            var bestSellerProductIds = await _unitOfWork.OrderDetails
                .FindAll()
                .GroupBy(od => od.ProductId)
                .OrderByDescending(g => g.Sum(od => od.Quantity ?? 0))
                .Select(g => g.Key)
                .Take(limit)
                .ToListAsync();
            Console.WriteLine("Best Seller Product IDs: " + string.Join(", ", bestSellerProductIds));
            var bestSellerProducts = await _unitOfWork.Products
                .FindAll()
                .Where(p => bestSellerProductIds.Contains(p.Id))
                .ToListAsync();

            //sort best seller products by bestSellerProductIds
            bestSellerProducts = bestSellerProducts
                .OrderBy(p => bestSellerProductIds.IndexOf(p.Id))
                .ToList();

            return _mapper.Map<List<ProductModel>>(bestSellerProducts);
        }
        #endregion


        #region get list product have lastest
        public async Task<List<ProductModel>> GetLatestProductsAsync(int limit)
        {
            var latestProducts = await _unitOfWork.Products
                .FindAll()
                .OrderByDescending(p => p.CreateDate)
                .Take(limit)
                .ToListAsync();

            return _mapper.Map<List<ProductModel>>(latestProducts);
        }
        #endregion


        #region get products with filter
        public async Task<List<ProductModel>> GetAllProductsWithFilterAsync(ProductFilterModel filter)
        {
            var query = _unitOfWork.Products.FindAll();

            if (filter.ProductName != null)
                query = query.Where(p => p.ProductName.Contains(filter.ProductName));

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

            if (filter.CampaignId.HasValue)
                query = query.Where(p => p.CampaignId == filter.CampaignId.Value);

            if (filter.Gender != null)
                query = query.Where(p => p.Gender == filter.Gender);

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            if (filter.StockQuantity.HasValue)
                query = query.Where(p => p.StockQuantity == filter.StockQuantity.Value);

            if (filter.ImageUrl != null)
                query = query.Where(p => p.ImageUrl == filter.ImageUrl);

            if (filter.Size.HasValue)
                query = query.Where(p => p.Size == filter.Size.Value);

            if (filter.Color != null)
                query = query.Where(p => p.Color == filter.Color);

            if (filter.Status != null)
                query = query.Where(p => p.Status == filter.Status);

            if (filter.CreateDate.HasValue)
                query = query.Where(p => p.CreateDate == filter.CreateDate.Value);

            if (filter.CollectionId.HasValue)
            {
                query = query.Where(p => p.CollectionProducts.Any(cp => cp.CollectionId == filter.CollectionId.Value));
            }

            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                switch (filter.SortBy)
                {
                    case nameof(Product.ProductName):
                        query = filter.IsAscending ? query.OrderBy(p => p.ProductName) : query.OrderByDescending(p => p.ProductName);
                        break;
                    case nameof(Product.Price):
                        query = filter.IsAscending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price);
                        break;
                    case nameof(Product.Size):
                        query = filter.IsAscending ? query.OrderBy(p => p.Size) : query.OrderByDescending(p => p.Size);
                        break;
                    case nameof(Product.Color):
                        query = filter.IsAscending ? query.OrderBy(p => p.Color) : query.OrderByDescending(p => p.Color);
                        break;
                    case nameof(Product.StockQuantity):
                        query = filter.IsAscending ? query.OrderBy(p => p.StockQuantity) : query.OrderByDescending(p => p.StockQuantity);
                        break;
                    case nameof(Product.CreateDate):
                        query = filter.IsAscending ? query.OrderBy(p => p.CreateDate) : query.OrderByDescending(p => p.CreateDate);
                        break;
                    case nameof(Product.Status):
                        query = filter.IsAscending ? query.OrderBy(p => p.Status) : query.OrderByDescending(p => p.Status);
                        break;
                        
                }
            }
            var listProducts = await query.ToListAsync();

            if (listProducts != null)
            {
                var listProductModel = _mapper.Map<List<ProductModel>>(listProducts);
                return listProductModel;
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}
    