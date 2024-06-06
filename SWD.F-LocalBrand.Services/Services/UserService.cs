using AutoMapper;

using Microsoft.EntityFrameworkCore;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Data.Models;
namespace SWD.F_LocalBrand.Business.Services
{
    public class UserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<User?> GetUserById(int id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<UserModel?> GetUserByEmail(string email)
        {
            var user = await _unitOfWork.Users.FindByCondition(u=>u.Email==email).FirstOrDefaultAsync();
            return _mapper.Map<UserModel>(user);
        }


    }
}
