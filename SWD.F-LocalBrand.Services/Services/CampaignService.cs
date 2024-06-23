using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

        #region create campaign
        public async Task<Campaign?> CreateCampaignAsync(CampaignCreateModel model)
        {
            var campaign = new Campaign
            {
                CampaignName = model.CampaignName
            };

            await _unitOfWork.Campaigns.CreateAsync(campaign);
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
    }
}
