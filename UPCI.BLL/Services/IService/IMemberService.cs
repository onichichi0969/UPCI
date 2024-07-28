﻿namespace UPCI.BLL.Services.IService
{
    public interface IMemberService
    {
        Task<List<UPCI.DAL.DTO.Response.Member>> Get();
        Task<UPCI.DAL.DTO.Response.Member> ById(string id);
        Task<UPCI.DAL.DTO.Response.VMember> Filter(UPCI.DAL.DTO.Request.FParam model);
        Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.Member model);
        Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.Member model);
        Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.Member model);
    }
}