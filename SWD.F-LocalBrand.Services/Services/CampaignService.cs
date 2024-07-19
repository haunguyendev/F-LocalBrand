using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWD.F_LocalBrand.Business.Common.Shared;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Business.DTO.Campaign;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Data.Models;
using SWD.F_LocalBrand.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.Services
{
    public class CampaignService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CampaignService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        //get campain with product in collection in campain by id
        public async Task<CampaignModel> GetCampaignById(int id)
        {
            //var campaign = await _unitOfWork.Campaigns.FindByCondition(
            //    c => c.Id == id, trackChanges: false,
            //                          includeProperties: c => c.Collections)
            //         .Include(c => c.Collections)
            //         .ThenInclude(col => col.CollectionProducts)
            //         .ThenInclude(cp => cp.Product)
            //         .Include(c => c.Products)
            //         .FirstOrDefaultAsync();

            var campaign = await _unitOfWork.Campaigns.FindByCondition(
                c => c.Id == id, false).Include(c => c.Collections).ThenInclude(col => col.CollectionProducts)
                     .ThenInclude(cp => cp.Product)
                     .Include(c => c.Products)
                     .FirstOrDefaultAsync();
            if (campaign != null)
            {
                var campaignModel = _mapper.Map<CampaignModel>(campaign);
                return campaignModel;
            }
            else
            {
                return null;
            }
        }
        #region Utils
        public async Task<bool> IsCampaignInUseAsync(int campaignId)
        {
            return await _unitOfWork.Collections.AnyAsync(c => c.CampaignId == campaignId);
        }
        #endregion


        #region create campaign
        public async Task<Campaign?> CreateCampaignAsync(CampaignCreateModel model)
        {
            var campaign = new Campaign
            {
                CampaignName = model.CampaignName,
                Status = model.Status
            };
            await _unitOfWork.Campaigns.CreateAsync(campaign);
            await _unitOfWork.CommitAsync();

            if (model.CollectionIds != null)
            {
                var collections = await _unitOfWork.Collections.GetByIdsAsync(model.CollectionIds);

                foreach (var collection in collections)
                {
                    if (collection == null)
                    {
                        throw new ArgumentException($"One or more collections do not exist.");
                    }
                    collection.Campaign = campaign;
                    collection.CampaignId = campaign.Id;
                    await _unitOfWork.Collections.UpdateAsync(collection);
                    campaign.Collections.Add(collection);
                }
            }

            await _unitOfWork.CommitAsync();
            return campaign;
        }


        #endregion
        #region update campaign
        public async Task<Campaign?> UpdateCampaignAsync(CampaignUpdateModel model)
        {
            var campaign = await _unitOfWork.Campaigns.GetByIdAsync(model.Id);
            if (campaign == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(model.CampaignName))
            {
                campaign.CampaignName = model.CampaignName;
            }

            if (model.CollectionIds != null)
            {
                campaign.Collections.Clear();
                foreach (var collectionId in model.CollectionIds)
                {
                    var collection = await _unitOfWork.Collections.GetByIdAsync(collectionId);
                    if (collection == null)
                    {
                        throw new ArgumentException($"Collection with ID {collectionId} does not exist.");
                    }
                    campaign.Collections.Add(collection);
                }
            }

            await _unitOfWork.Campaigns.UpdateAsync(campaign);
            await _unitOfWork.CommitAsync();

            return campaign;
        }

        #endregion

        public async Task<bool> IsCampaignNameExistAsync(string campaignName)
        {
            return await _unitOfWork.Campaigns.AnyAsync(c => c.CampaignName == campaignName);
        }

        #region get campaigns with filter
        public async Task<List<CampaignModel>> GetAllCampaignsWithFilterAsync(CampaignFilterModel filter)
        {
            var query = _unitOfWork.Campaigns.FindAll();

            if (filter.CampaignName != null)
                query = query.Where(c => c.CampaignName.Contains(filter.CampaignName));
            if(filter.Status != null)
                query = query.Where(c => c.Status == filter.Status);

            // Áp dụng sắp xếp
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                switch (filter.SortBy)
                {
                    case nameof(Campaign.CampaignName):
                        query = filter.IsAscending ? query.OrderBy(c => c.CampaignName) : query.OrderByDescending(c => c.CampaignName);
                        break;
                        // Thêm các trường khác nếu cần
                }
            }
            query = query
                .Include(c => c.Collections)
                    .ThenInclude(col => col.CollectionProducts)
                        .ThenInclude(cp => cp.Product)
                .Include(c => c.Products);

            var listCampaigns = await query.ToListAsync();

            if (listCampaigns != null)
            {
                var listCampaignModel = _mapper.Map<List<CampaignModel>>(listCampaigns);
                return listCampaignModel;
            }
            else
            {
                return null;
            }
        }

        #endregion
        #region update status campaign
        public async Task<bool> UpdateCampaignStatusAsync(int campaignId, string status)
        {
            var campaign = await _unitOfWork.Campaigns.FindAsync(c => c.Id == campaignId);

            if (campaign == null)
            {
                return false;
            }

            campaign.Status = status;
            await _unitOfWork.Campaigns.UpdateAsync(campaign);
            await _unitOfWork.CommitAsync();

            return true;
        }
        #endregion
        #region update stauts deleted campaign
        public async Task<bool> DeleteCampaignAsync(int campaignId)
        {
            var campaign = await _unitOfWork.Campaigns.FindAsync(c => c.Id == campaignId);

            if (campaign == null)
            {
                return false;
            }

            campaign.Status = CollectionStatusTypeEnum.Deleted;
            await _unitOfWork.Campaigns.UpdateAsync(campaign);
            await _unitOfWork.CommitAsync();

            return true;
        }
        #endregion

    }
}
