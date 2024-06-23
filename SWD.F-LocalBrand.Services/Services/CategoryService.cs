using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Business.DTO.Category;
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
    public class CategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        //Get categories with all product of them
        public List<CategoryModel> GetAllProductsWithCategories()
        {
            // Lấy danh sách Category kèm theo Products
            var categories =  _unitOfWork.Categories
                .FindAll(trackChanges: false, includeProperties: c => c.Products)
                .ToList();

            // Ánh xạ từ entities sang DTO models
            var categoryModels = _mapper.Map<List<CategoryModel>>(categories);
            return categoryModels;
        }

        //Get all categories
        public List<CategoryModel> GetAllCategories()
        {
            var categories = _unitOfWork.Categories.FindAll().ToList();
            if (categories == null)
            {
                return null;
            }
            var categoryModels = _mapper.Map<List<CategoryModel>>(categories);
            return categoryModels;
        }

        //get category with categpry id with products of them 
        public CategoryModel GetCategoryWithProducts(int categoryId)
        {
            var category = _unitOfWork.Categories.FindByCondition(c => c.Id == categoryId, trackChanges: false, includeProperties: c => c.Products).FirstOrDefault();
            if (category == null)
            {
                return null;
            }
            var categoryModel = _mapper.Map<CategoryModel>(category);
            return categoryModel;
        }

        #region create category

        public async Task<Category> CreateCategoryAsync(CategoryCreateModel model)
        {
            var category = new Category
            {
                CategoryName = model.CategoryName,
                Description = model.Description
            };

            await _unitOfWork.Categories.CreateAsync(category);
            await _unitOfWork.CommitAsync();

            return category;
        }
        #endregion
        #region update category
        public async Task<CategoryUpdateModel?> UpdateCategoryAsync(CategoryUpdateModel model)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(model.Id);
            if (category == null)
            {
                return null; // Return null if category is not found
            }

            category.CategoryName = model.CategoryName;
            category.Description = model.Description;

            await _unitOfWork.Categories.UpdateAsync(category);
            await _unitOfWork.CommitAsync();

            return model;
        }
        #endregion

        #region deleted category

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null)
            {
                return false; // Category not found
            }

            // Update status of all related products to "Inactive"
            var products = await _unitOfWork.Products.FindByCondition(x=>x.CategoryId==categoryId).ToListAsync();
            foreach (var product in products)
            {
                product.Status = "Inactive";
                await _unitOfWork.Products.UpdateAsync(product);
            }

            // Update category status to "Deleted"
            category.Status = "Deleted";
            await _unitOfWork.Categories.UpdateAsync(category);
            await _unitOfWork.CommitAsync();

            return true;
        }

        #endregion
    }
}
