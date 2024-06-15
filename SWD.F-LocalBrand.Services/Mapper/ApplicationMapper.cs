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
            CreateMap<Product, ProductModel>().ReverseMap();

            CreateMap<Category, CategoryModel>().ReverseMap()
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products));
            CreateMap<Compapility, CompapilityModel>();

            CreateMap<Compapility, CompapilityModel>()
            .ForMember(dest => dest.RecommendedProduct, opt => opt.MapFrom(src => src.RecommendedProduct));
            CreateMap<Product, ProductModel>()
                .ForMember(dest => dest.Recommendations, opt => opt.MapFrom(src => src.CompapilityProducts.Select(cp => cp.RecommendedProduct)));
            CreateMap<Collection, CollectionModel>()
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.CollectionProducts.Select(cp => cp.Product)));
            CreateMap<Campaign, CampaignModel>()
                .ForMember(dest => dest.Collections, opt => opt.MapFrom(src => src.Collections));

            CreateMap<OrderHistory, OrderHistoryModel>();
            CreateMap<Order, OrderModel>()
                .ForMember(dest => dest.OrderHistories, opt => opt.MapFrom(src => src.OrderHistories));

            //CreateMap<Customer, CustomerModel>()
            //    .ForMember(dest => dest.CustomerProducts, opt => opt.MapFrom(src => src.CustomerProducts));

            CreateMap<CustomerProduct, CustomerProductModel>()
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product));
        }
    }
}
