using AutoMapper;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Data.Models;


namespace SWD.F_LocalBrand.Business.Mapper
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper() 
        {
            CreateMap<User, UserModel>().ReverseMap();
//            CreateMap<Materials, MaterialsModel>().ReverseMap();
        }
    }
}
