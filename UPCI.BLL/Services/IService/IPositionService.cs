namespace UPCI.BLL.Services.IService
{
    public interface IPositionService
    {
        Task<List<UPCI.DAL.DTO.Response.PositionCell>> GetCell();
        Task<List<UPCI.DAL.DTO.Response.PositionMinistry>> GetMinistry();

        Task<UPCI.DAL.DTO.Response.PositionCell> ByIdCell(string id);
        Task<UPCI.DAL.DTO.Response.PositionMinistry> ByIdMinistry(string id);

        Task<UPCI.DAL.DTO.Response.VPositionCell> FilterCell(UPCI.DAL.DTO.Request.FParam model);
        Task<UPCI.DAL.DTO.Response.VPositionMinistry> FilterMinistry(UPCI.DAL.DTO.Request.FParam model);

        Task<UPCI.DAL.DTO.Response.Result> CreateCell(UPCI.DAL.DTO.Request.PositionCell model);
        Task<UPCI.DAL.DTO.Response.Result> CreateMinistry(UPCI.DAL.DTO.Request.PositionMinistry model);

        Task<UPCI.DAL.DTO.Response.Result> UpdateCell(UPCI.DAL.DTO.Request.PositionCell model);
        Task<UPCI.DAL.DTO.Response.Result> UpdateMinistry(UPCI.DAL.DTO.Request.PositionMinistry model);

        Task<UPCI.DAL.DTO.Response.Result> DeleteCell(UPCI.DAL.DTO.Request.PositionCell model);
        Task<UPCI.DAL.DTO.Response.Result> DeleteMinistry(UPCI.DAL.DTO.Request.PositionMinistry model);
    }
}
