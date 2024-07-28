namespace UPCI.BLL.Services.IService
{
    public interface IApiClientService
    {
        Task<UPCI.DAL.DTO.Response.ApiClient> Authenticate(string id, string password);
        Task<UPCI.DAL.DTO.Response.ApiClient> ByApiKey(string key);
        Task<List<UPCI.DAL.DTO.Response.ApiClient>> GetAll(bool includeDeleted);
        Task<UPCI.DAL.DTO.Response.ApiClient> ById(string id);
        Task<UPCI.DAL.DTO.Response.APISecurityResult> Reset(UPCI.DAL.DTO.Request.ApiClient model);
        Task<UPCI.DAL.DTO.Response.VApiClient> Filter(UPCI.DAL.DTO.Request.FParam model);
        Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.ApiClient model);
        Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.ApiClient model);
        Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.ApiClient model);

        //reset password

        //renew apikey

    }
}
