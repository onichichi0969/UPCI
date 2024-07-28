﻿namespace UPCI.BLL.Services.IService
{
    public interface IMinistryService
    {
        Task<List<UPCI.DAL.DTO.Response.Ministry>> Get();
        Task<UPCI.DAL.DTO.Response.Ministry> ById(string id);
        Task<UPCI.DAL.DTO.Response.VMinistry> Filter(UPCI.DAL.DTO.Request.FParam model);
        Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.Ministry model);
        Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.Ministry model);
        Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.Ministry model);
    }
}