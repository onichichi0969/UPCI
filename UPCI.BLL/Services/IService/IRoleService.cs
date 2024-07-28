namespace UPCI.BLL.Services.IService
{
    public interface IRoleService
    {
        Task<List<UPCI.DAL.DTO.Response.Role>> Get();
        Task<UPCI.DAL.DTO.Response.Role> ById(string id);
        Task<List<UPCI.DAL.DTO.Response.Module>> ByCode(string code);
        Task<UPCI.DAL.DTO.Response.VRole> Filter(UPCI.DAL.DTO.Request.FParam model);
        Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.Role model);
        Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.Role model);
        Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.Role model);
    }
}
