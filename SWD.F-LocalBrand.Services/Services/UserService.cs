using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Business.DTO.User;
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

        #region Get all list user

        public async Task<List<UserResponseModel>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            var userDtos = users.Select(u => new UserResponseModel
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Phone = u.Phone,
                Address = u.Address,
                Status=u.Status,
                RoleName = u.Role?.RoleName 
            }).ToList();

            return userDtos;
        }
        #endregion
        #region Get user have filter
         public async Task<List<UserResponseModel>> GetAllUsersWithFilterAsync(UserFilterModel filter)
    {
        var query = _unitOfWork.Users.FindAll();

        if (!string.IsNullOrEmpty(filter.UserName))
            query = query.Where(u => u.UserName.Contains(filter.UserName));

        if (!string.IsNullOrEmpty(filter.Email))
            query = query.Where(u => u.Email.Contains(filter.Email));

        if (!string.IsNullOrEmpty(filter.Phone))
            query = query.Where(u => u.Phone.Contains(filter.Phone));

        if (!string.IsNullOrEmpty(filter.Address))
            query = query.Where(u => u.Address.Contains(filter.Address));

        if (filter.RegistrationDate.HasValue)
            query = query.Where(u => u.RegistrationDate == filter.RegistrationDate.Value);

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(u => u.Status == filter.Status);

        if (!string.IsNullOrEmpty(filter.RoleName))
            query = query.Where(u => u.Role.RoleName.Contains(filter.RoleName));

        if (!string.IsNullOrEmpty(filter.SortBy))
        {
            switch (filter.SortBy)
            {
                case nameof(User.UserName):
                    query = filter.IsAscending ? query.OrderBy(u => u.UserName) : query.OrderByDescending(u => u.UserName);
                    break;
                case nameof(User.Email):
                    query = filter.IsAscending ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email);
                    break;
                case nameof(User.Phone):
                    query = filter.IsAscending ? query.OrderBy(u => u.Phone) : query.OrderByDescending(u => u.Phone);
                    break;
                case nameof(User.RegistrationDate):
                    query = filter.IsAscending ? query.OrderBy(u => u.RegistrationDate) : query.OrderByDescending(u => u.RegistrationDate);
                    break;
                case nameof(User.Status):
                    query = filter.IsAscending ? query.OrderBy(u => u.Status) : query.OrderByDescending(u => u.Status);
                    break;
                case nameof(User.Role.RoleName):
                    query = filter.IsAscending ? query.OrderBy(u => u.Role.RoleName) : query.OrderByDescending(u => u.Role.RoleName);
                    break;
            }
        }

        var users = await query.ToListAsync();
        var userDtos = _mapper.Map<List<UserResponseModel>>(users);
        return userDtos;
    }
        #endregion

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

        #region update user detail
        public async Task<UserUpdateModel?> UpdateUserAsync(UserUpdateModel userUpdateModel)
        {
            var user = await _unitOfWork.Users.FindAsync(u => u.Id == userUpdateModel.Id);

            if (user == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(userUpdateModel.Email))
                user.Email = userUpdateModel.Email;

            if (!string.IsNullOrEmpty(userUpdateModel.Phone))
                user.Phone = userUpdateModel.Phone;

            if (!string.IsNullOrEmpty(userUpdateModel.Address))
                user.Address = userUpdateModel.Address;

            if (userUpdateModel.ImageUrl != null && userUpdateModel.ImageUrl.Length > 0)
            {
                if (!string.IsNullOrEmpty(user.Image))
                {
                    string url = $"USER/{user.Id}";
                    var deleteResult = await _firebaseService.DeleteFileFromFirebase(url);
                    if (!deleteResult)
                    {
                        throw new Exception("Delete image failed");
                    }
                }

                var imageUrl = $"USER/{user.Id}";
                var uploadResult = await _firebaseService.UploadFileToFirebase(userUpdateModel.ImageUrl, imageUrl);
                user.Image = uploadResult;
                
            }

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            return userUpdateModel;
        }
        #endregion

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _unitOfWork.Users.AnyAsync(u => u.Email == email);
        }


    }
}
