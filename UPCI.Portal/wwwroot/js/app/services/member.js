﻿
var MemberService = {};

MemberService.All = () => { 
    return axios.get(appUrl + '/Application/Membership/Index?handler=All');
};
MemberService.GetCodeAndName = (list) => { 
    return axios.post(appUrl + '/Application/Membership/Index?handler=GetCodeAndName',
        JSON.stringify(list),
        { headers: headers });
};
MemberService.Search = (filter, sortColumn, descending, pageNum, pageSize) => {
    var searchOption = { Filters: filter, SortColumn: sortColumn, Descending: descending, PageNum: pageNum, PageSize: pageSize };
    return axios.post(appUrl + '/Application/Membership/Index?handler=Filter',
        JSON.stringify(searchOption),
        { headers: headers });
};
   
MemberService.Delete = (item) => {
    return axios.post(appUrl + '/Application/Membership/Index?handler=Delete',
        JSON.stringify(item),
        { headers: headers });
};

MemberService.Save = (item) => {
    return axios.post(appUrl + '/Application/Membership/Index?handler=Save',
        JSON.stringify(item),
        { headers: headers });
};
//MemberService.Save = (formData) => {
//    return axios.post(appUrl + '/Application/Membership/Index?handler=Save', formData, {
//        headers: {
//            'XSRF-TOKEN': csrfToken,
//            'Content-Type': 'multipart/form-data'   
//        }
//    });
//};

MemberService.GetMemberProfileImage = (id) => {
    return axios.post(appUrl + '/Application/Membership/Index?handler=MemberProfileImage',
        JSON.stringify(id),
        {
            headers: headers,
            responseType: 'blob'
        });
};