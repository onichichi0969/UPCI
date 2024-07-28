using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPCI.BLL.Services.IService
{
    public interface IModuleService
    {
        Task<List<UPCI.DAL.DTO.Response.Module>> Get(); 
        Task<List<UPCI.DAL.DTO.Response.ModuleAccess>> GetUserModuleAccess(string roleModuleCode);
        Task<List<UPCI.DAL.DTO.Response.ModuleAccess>> GetAllParent();
        Task<List<UPCI.DAL.DTO.Response.ModuleAction>> GetAllModuleAction();
        Task<UPCI.DAL.DTO.Response.Module> ById(string id);
        Task<UPCI.DAL.DTO.Response.VModule> Filter(UPCI.DAL.DTO.Request.FParam model);
        Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.Module model);
        Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.Module model);
        Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.Module model);
    }
}
