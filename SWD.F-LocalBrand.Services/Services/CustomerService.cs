using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SWD.F_LocalBrand.Business.Helpers;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Data.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.Services
{
    public class CustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }



        //Get customer by username
        public async Task<bool?> GetCustomerByUsername(string email)
        {
            var customer = await _unitOfWork.Customers.FindByCondition(c => c.Email == email).FirstOrDefaultAsync();
            if (customer != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> UpdatePass(string email, string newPassword)
        {
            var user = await _unitOfWork.Customers.FindByCondition(c => c.Email == email, true).FirstOrDefaultAsync();

            if (user != null)
            {
                user.Password = SecurityUtil.Hash(newPassword);

                var result = await _unitOfWork.CommitAsync();
                if (result > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
