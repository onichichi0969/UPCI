using AutoMapper;
using UPCI.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UPCI.DAL
{
    public static class DependencyInjection
    {
        public static void RegisterDAL(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("Default")!));
            var mapperConfiguration = new MapperConfiguration(config =>
            {
                //config.CreateMap<ApiClient, UPCI.DAL.DTO.Response.ApiClient>()
                //    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                //    .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId));
                //config.CreateMap<ApiClient, UPCI.DAL.DTO.Response.FApiClient>()
                //    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                //    .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId));

                //config.CreateMap<Company, UPCI.DAL.DTO.Response.Company>()
                //.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
                //config.CreateMap<Company, UPCI.DAL.DTO.Response.FCompany>()
                //    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

                //config.CreateMap<Route, UPCI.DAL.DTO.Response.Route>()
                // .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
                //config.CreateMap<Route, UPCI.DAL.DTO.Response.FRoute>()
                //    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

                config.CreateMap<Cell, UPCI.DAL.DTO.Response.Cell>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

                config.CreateMap<Ministry, UPCI.DAL.DTO.Response.Ministry>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

                config.CreateMap<Department, UPCI.DAL.DTO.Response.Department>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

                config.CreateMap<Member, Member>()
                .ForMember(dest => dest.FullName, opt =>
                    opt.MapFrom(src => src.FirstName + " " + src.MiddleName + " " + src.LastName));

                config.CreateMap<MemberMinistry, UPCI.DAL.DTO.Response.MemberMinistry>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

                config.CreateMap<MemberCell, UPCI.DAL.DTO.Response.MemberCell>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

               

                //For getting count and putting it to a int variable, another workaround is, user model.Count in sorting
                //config.CreateMap<Ministry, Ministry>()
                // .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.MemberMinistry.Count));

                config.CreateMap<PositionCell, UPCI.DAL.DTO.Response.PositionCell>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)); 

                config.CreateMap<PositionMinistry, UPCI.DAL.DTO.Response.PositionMinistry>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)); 

                config.CreateMap<Module, UPCI.DAL.DTO.Response.Module>()
                     .ForMember(dest => dest.Id, opt => opt.MapFrom(src => UPCI.DAL.Helpers.StringManipulation.Encrypt(src.Id.ToString(), configuration["AppContext:EncryptionKey"]!)))
                     .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId.ToString()));
                config.CreateMap<Module, UPCI.DAL.DTO.Response.FModule>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => UPCI.DAL.Helpers.StringManipulation.Encrypt(src.Id.ToString(), configuration["AppContext:EncryptionKey"]!)))
                    .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId.ToString()));

                config.CreateMap<Role, UPCI.DAL.DTO.Response.Role>()
                    .ForMember(dest => dest.EncryptedId, opt => opt.MapFrom(src => UPCI.DAL.Helpers.StringManipulation.Encrypt(src.Id.ToString(), configuration["AppContext:EncryptionKey"]!)));
                config.CreateMap<Role, UPCI.DAL.DTO.Response.FRole>()
                    .ForMember(dest => dest.EncryptedId, opt => opt.MapFrom(src => UPCI.DAL.Helpers.StringManipulation.Encrypt(src.Id.ToString(), configuration["AppContext:EncryptionKey"]!)));

             

                config.CreateMap<User, UPCI.DAL.DTO.Response.User>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.EncryptedRoleId, opt => opt.MapFrom(src => UPCI.DAL.Helpers.StringManipulation.Encrypt(src.RoleId.ToString(), configuration["AppContext:EncryptionKey"]!)));
                config.CreateMap<User, UPCI.DAL.DTO.Response.FUser>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.EncryptedRoleId, opt => opt.MapFrom(src => UPCI.DAL.Helpers.StringManipulation.Encrypt(src.RoleId.ToString(), configuration["AppContext:EncryptionKey"]!)));

            });
            var mapper = mapperConfiguration.CreateMapper();
            services.AddSingleton(mapper);
        }
    }
}
