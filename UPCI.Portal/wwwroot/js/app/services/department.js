
var DepartmentService = {};

DepartmentService.All = () => { 
    return axios.get(appUrl + '/Maintenance/Department/Index?handler=All');
};

DepartmentService.Search = (filter, sortColumn, descending, pageNum, pageSize) => {
    var searchOption = { Filters: filter, SortColumn: sortColumn, Descending: descending, PageNum: pageNum, PageSize: pageSize };
    return axios.post(appUrl + '/Maintenance/Department/Index?handler=Filter',
        JSON.stringify(searchOption),
        { headers: headers });
};
   
DepartmentService.Delete = (item) => {
    return axios.post(appUrl + '/Maintenance/Department/Index?handler=Delete',
        JSON.stringify(item),
        { headers: headers });
};

DepartmentService.Save = (item) => {
    return axios.post(appUrl + '/Maintenance/Department/Index?handler=Save',
        JSON.stringify(item),
        { headers: headers });
};
    