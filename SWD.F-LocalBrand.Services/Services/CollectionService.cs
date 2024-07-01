using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Business.DTO.Collection;
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
        #region create collection

        public async Task<string> CreateCollectionAsync(CollectionCreateModel model)
        {
            if (await _unitOfWork.Collections.CollectionNameExistsAsync(model.CollectionName))
            {
                throw new ArgumentException("Collection name already exists.");
            }
            var collection = new Collection
            {
                CollectionName = model.CollectionName
            };

            await _unitOfWork.Collections.CreateAsync(collection);
            await _unitOfWork.CommitAsync();

            return collection.CollectionName;
        }
        #endregion

        #region Update collection

        public async Task<Collection?> UpdateCollectionAsync(CollectionUpdateModel model)
        {
            var collection = await _unitOfWork.Collections.GetByIdAsync(model.Id);
            if (collection == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(model.CollectionName))
            {
                collection.CollectionName = model.CollectionName;
            }

            if (model.CampaignId.HasValue)
            {
                collection.CampaignId = model.CampaignId;
            }

            if (model.CollectionProductIds != null && model.CollectionProductIds.Any())
            {
                var collectionProducts = model.CollectionProductIds.Select(id => new CollectionProduct
                {
                    CollectionId = collection.Id,
                    ProductId = id
                }).ToList();

                collection.CollectionProducts = collectionProducts;
            }

            await _unitOfWork.Collections.UpdateAsync(collection);
            await _unitOfWork.CommitAsync();

            return collection;
        }
        #endregion

        #region get all collection
        public async Task<List<CollectionModel>> GetCollections()
        {
            var collection = await _unitOfWork.Collections.FindAll().ToListAsync();
            return _mapper.Map<List<CollectionModel>>(collection);
        }
        #endregion
    }
}
