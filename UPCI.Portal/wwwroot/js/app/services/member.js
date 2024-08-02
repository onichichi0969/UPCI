
var MemberService = {};

MemberService.All = () => { 
    return axios.get(appUrl + '/Application/Membership/Index?handler=All');
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

MemberService.GetMemberProfileImage = (id) => {
    return axios.post(appUrl + '/Application/Membership/Index?handler=MemberProfileImage',
        JSON.stringify(id),
        {
            headers: headers,
            responseType: 'blob'
        });
};