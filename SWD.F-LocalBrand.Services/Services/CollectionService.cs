using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.Services
{
    public class CollectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CollectionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        //get collection with product in collection by id
        public async Task<CollectionModel> GetCollectionById(int id)
        {
            var collection = await _unitOfWork.Collections.FindByCondition(
                c => c.Id == id, 
                trackChanges: false, 
                includeProperties: c => c.CollectionProducts)
                .Include(c => c.CollectionProducts)
                .ThenInclude(cp => cp.Product)
            .FirstOrDefaultAsync();
            if (collection != null)
            {
                var collectionModel = _mapper.Map<CollectionModel>(collection);
                return collectionModel;
            }
            else
            {
                return null;
            }
        }

    }
}
