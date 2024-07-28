namespace UPCI.BLL.Services.IService
{
    public interface ICompanyService
    {
        Task<List<UPCI.DAL.DTO.Response.Company>> Get();
        Task<UPCI.DAL.DTO.Response.Company> ById(string id);
        Task<UPCI.DAL.DTO.Response.VCompany> Filter(UPCI.DAL.DTO.Request.FParam model);
        Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.Company model);
        Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.Company model);
        Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.Company model);
    }
}
