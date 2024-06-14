using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var campaign = await _unitOfWork.Campaigns.FindByCondition(
                               c => c.Id == id,
                                              trackChanges: false,
                                                             includeProperties: c => c.Collections)
                .Include(c => c.Collections)
                .ThenInclude(cp => cp.CollectionProducts)
                .ThenInclude(cp => cp.Product)
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

    }
}
