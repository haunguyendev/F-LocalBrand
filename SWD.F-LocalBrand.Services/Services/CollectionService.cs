using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWD.F_LocalBrand.Business.Common.Shared;
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
        public async Task<bool> IsCollectionInUseAsync(int collectionId)
        {
            return await _unitOfWork.CollectionProducts.AnyAsync(cp => cp.CollectionId == collectionId);
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
                CollectionName = model.CollectionName,
                Status = model.Status
            };
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
            collection.Status = model.Status;

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

        #region get collections with filter
        public async Task<List<CollectionModel>> GetAllCollectionsWithFilterAsync(CollectionFilterModel filter)
        {
            var query = _unitOfWork.Collections
                .FindAll(true);

            if (filter.CollectionName != null)
                query = query.Where(c => c.CollectionName.Contains(filter.CollectionName));

            if (filter.CampaignId.HasValue)
                query = query.Where(c => c.CampaignId == filter.CampaignId.Value);

            if (filter.Status != null)
                query = query.Where(c => c.Status == filter.Status);

            // Áp dụng sắp xếp
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                switch (filter.SortBy)
                {
                    case nameof(Collection.CollectionName):
                        query = filter.IsAscending ? query.OrderBy(c => c.CollectionName) : query.OrderByDescending(c => c.CollectionName);
                        break;
                    case nameof(Collection.CampaignId):
                        query = filter.IsAscending ? query.OrderBy(c => c.CampaignId) : query.OrderByDescending(c => c.CampaignId);
                        break;
                        // Thêm các trường khác nếu cần
                }
            }
            query = query.Include(c => c.CollectionProducts).ThenInclude(cp => cp.Product);
            var listCollections = await query.ToListAsync();

            if (listCollections != null)
            {
                var listCollectionModel = _mapper.Map<List<CollectionModel>>(listCollections);
                return listCollectionModel;
            }
            else
            {
                return null;
            }
        }

        #endregion
        #region update collection category
        public async Task<bool> UpdateCollectionStatusAsync(int collectionId, string status)
        {
            var collection = await _unitOfWork.Collections.FindAsync(c => c.Id == collectionId);

            if (collection == null)
            {
                return false;
            }

            collection.Status = status;
            await _unitOfWork.Collections.UpdateAsync(collection);
            await _unitOfWork.CommitAsync();

            return true;
        }
        #endregion
        #region delete collection 
        public async Task<bool> DeleteCollectionAsync(int collectionId)
        {
            var collection = await _unitOfWork.Collections.FindAsync(c => c.Id == collectionId);

            if (collection == null)
            {
                return false;
            }

            collection.Status = CollectionStatusTypeEnum.Deleted;
            await _unitOfWork.Collections.UpdateAsync(collection);
            await _unitOfWork.CommitAsync();

            return true;
        }
        #endregion
    }
}
