using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWD.F_LocalBrand.Business.DTO;
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
    public class ProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }




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

        //get prodcut by category id
        public async Task<List<ProductModel>> GetProductsByCategoryIdAsync(int categoryId)
        {
            var listProducts = await _unitOfWork.Products.FindAll().Where(x => x.CategoryId == categoryId).ToListAsync();
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

        //get product by category id and have paging
        public async Task<List<ProductModel>> GetProductsByCategoryIdPagingAsync(int categoryId, int pageIndex, int pageSize)
        {
            var listProducts = await _unitOfWork.Products.FindAll().Where(x => x.CategoryId == categoryId).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
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
            var product = await _unitOfWork.Products.FindByCondition(
                p => p.Id == productId,
                trackChanges: false,
                includeProperties: p => p.CompapilityProducts)
                .Include(p => p.CompapilityProducts)
                    .ThenInclude(cp => cp.RecommendedProduct)
                .FirstOrDefaultAsync();

            if (product == null) return null;

            var visitedProducts = new HashSet<int>();
            var recommendations = new List<Product>();

            GetRecommendations(product, visitedProducts, recommendations);

            var productModel = _mapper.Map<ProductModel>(product);
            productModel.Recommendations = _mapper.Map<List<ProductModel>>(recommendations);

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

        
    }
}
