namespace UPCI.BLL.Services.IService
{
    public interface ICellService
    {
        Task<List<UPCI.DAL.DTO.Response.Cell>> Get();
        Task<UPCI.DAL.DTO.Response.Cell> ById(string id);
        Task<UPCI.DAL.DTO.Response.VCell> Filter(UPCI.DAL.DTO.Request.FParam model);
        Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.Cell model);
        Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.Cell model);
        Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.Cell model);
    }
}
