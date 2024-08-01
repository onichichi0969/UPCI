using UPCI.BLL.Services;
using UPCI.BLL.Services.IService;
using Microsoft.Extensions.DependencyInjection;

namespace UPCI.BLL
{
    public static class DependencyInjection
    {
        public static void RegisterBLL(this IServiceCollection services)
        {
         
            services.AddScoped<IApiClientService, ApiClientService>();
            
            services.AddScoped<ICompanyService, CompanyService>();


            services.AddScoped<ICellService, CellService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<IMailerService, MailerService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IMinistryService, MinistryService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IPositionService, PositionService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IRouteService, RouteService>(); 
            services.AddScoped<IUserService, UserService>();


            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));









        }
    }
}
