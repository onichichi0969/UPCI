
var UserService = {};

UserService.All = () => { 
    return axios.get(appUrl + '/Security/User/Index?handler=All');
};

UserService.Search = (filter, sortColumn, descending, pageNum, pageSize) => {
    var searchOption = { Filters: filter, SortColumn: sortColumn, Descending: descending, PageNum: pageNum, PageSize: pageSize };
    return axios.post(appUrl + '/Security/User/Index?handler=Filter',
        JSON.stringify(searchOption),
        { headers: headers });
};

UserService.CheckAD = (item) => {
    return axios.post(appUrl + '/Security/User/Index?handler=CheckAD',
        JSON.stringify(item),
        { headers: headers });
};

UserService.Delete = (item) => {
    return axios.post(appUrl + '/Security/User/Index?handler=Delete',
        JSON.stringify(item),
        { headers: headers });
};

UserService.Save = (item) => {
    return axios.post(appUrl + '/Security/User/Index?handler=Save',
        JSON.stringify(item),
        { headers: headers });
};
UserService.ResetPassword = (item) => {
    return axios.post(appUrl + '/Security/User/Index?handler=ResetPassword',
        JSON.stringify(item),
        { headers: headers });
};
UserService.UnlockUser = (item) => {
    return axios.post(appUrl + '/Security/User/Index?handler=UnlockUser',
        JSON.stringify(item),
        { headers: headers });
};
UserService.RecentActivity = () => {
    return axios.get(appUrl + '/Security/User/Index?handler=ActivityLog'); 
};