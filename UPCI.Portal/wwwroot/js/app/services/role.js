
var RoleService = {};

RoleService.All = () => { 
    return axios.get(appUrl + '/Security/Role/Index?handler=All');
};

RoleService.ByCode = (roleCode) => {
    return axios.get(appUrl + '/Security/Role/Index?roleCode=' + roleCode + '&handler=ModuleByRoleCode');
};

RoleService.Search = (filter, sortColumn, descending, pageNum, pageSize) => {
    var searchOption = { Filters: filter, SortColumn: sortColumn, Descending: descending, PageNum: pageNum, PageSize: pageSize };
    return axios.post(appUrl + '/Security/Role/Index?handler=Filter',
        JSON.stringify(searchOption),
        { headers: headers });
};
   
RoleService.Delete = (item) => {
    return axios.post(appUrl + '/Security/Role/Index?handler=Delete',
        JSON.stringify(item),
        { headers: headers });
};

RoleService.Save = (item) => {
    return axios.post(appUrl + '/Security/Role/Index?handler=Save',
        JSON.stringify(item),
        { headers: headers });
};
    