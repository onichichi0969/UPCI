namespace UPCI.BLL.Services.IService
{
    public interface IDepartmentService
    {
        Task<List<UPCI.DAL.DTO.Response.Department>> Get();
        Task<UPCI.DAL.DTO.Response.Department> ById(string id);
        Task<UPCI.DAL.DTO.Response.VDepartment> Filter(UPCI.DAL.DTO.Request.FParam model);
        Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.Department model);
        Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.Department model);
        Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.Department model);
    }
}
