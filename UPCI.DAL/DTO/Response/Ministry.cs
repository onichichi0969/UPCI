﻿namespace UPCI.DAL.DTO.Response
{
    public class VMinistry: ListBase
    {
        public List<FMinistry> Data { get; set; } = [];
    }

    public class FMinistry : Ministry
    {
        public bool Deleted { get; set; }
    }
    public class Ministry : Base
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string DepartmentCode { get; set; } = string.Empty;
        public string DepartmentDesc { get; set; } = string.Empty; 
        public List<MemberMinistry> MemberMinistry { get; set; } = new List<MemberMinistry>();
        //public int MemberCount{ get; set; } = 0;
    }
}
