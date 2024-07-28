
using UPCI.DAL.Models;

namespace UPCI.BLL.Services.IService
{
    public interface IRouteService
    {
        Task<List<UPCI.DAL.DTO.Response.Route>> GetAll();
        Task<UPCI.DAL.DTO.Response.Route> ById(string id);
        Task<UPCI.DAL.DTO.Response.VRoute> Filter(UPCI.DAL.DTO.Request.FParam model);
        Task<List<DAL.Models.RouteLogSummary>> RouteSummary();
        Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.Route model);
        Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.Route model);
        Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.Route model);
        Task<UPCI.DAL.DTO.Response.OcelotConfig> Config(); 
    }
}
