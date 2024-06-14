using AutoMapper;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Data.Common.Interfaces;
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
        public List<CategoryModel> GetAll()
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
            var categoryModels = _mapper.Map<List<CategoryModel>>(categories);
            return categoryModels;
        }
        
    }
}
