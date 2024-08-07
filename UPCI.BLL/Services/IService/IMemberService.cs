namespace UPCI.BLL.Services.IService
{
    public interface IMemberService
    {
        Task<UPCI.DAL.DTO.Response.MemberStatistics> MemberStatistics();
        Task<List<UPCI.DAL.DTO.Response.Member>> Get();
        Task<List<UPCI.DAL.DTO.Response.Member>> GetCodeAndName(List<string> existing);
        Task<UPCI.DAL.DTO.Response.Member> ById(string id);
        Task<UPCI.DAL.DTO.Response.VMember> Filter(UPCI.DAL.DTO.Request.FParam model);
        Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.Member model);
        Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.Member model);
        Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.Member model); 
    }
}
