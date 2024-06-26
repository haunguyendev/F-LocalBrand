using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Data.Models;
namespace SWD.F_LocalBrand.Business.Services
{
    public class UserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly FirebaseService _firebaseService;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, FirebaseService firebaseService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseService = firebaseService;
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

        //public async Task<string> CreateUrl(IFormFile formFile)
        //{
        //    if (formFile == null || formFile.Length == 0)
        //    {
        //        throw new ArgumentException("The file is empty");
        //    }

        //    var guidPath = Guid.NewGuid().ToString();
        //    var imagePath = "TRIP/" + $"{guidPath}";
        //    var downloadUrl = await _firebaseService.UploadFileToFirebase(formFile, imagePath);
        //    return downloadUrl;
        //}


    }
}
