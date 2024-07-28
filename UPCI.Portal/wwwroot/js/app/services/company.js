
var CompanyService = {};

CompanyService.All = () => { 
    return axios.get(appUrl + '/Maintenance/Company/Index?handler=All');
};

CompanyService.Search = (filter, sortColumn, descending, pageNum, pageSize) => {
    var searchOption = { Filters: filter, SortColumn: sortColumn, Descending: descending, PageNum: pageNum, PageSize: pageSize };
    return axios.post(appUrl + '/Maintenance/Company/Index?handler=Filter',
        JSON.stringify(searchOption),
        { headers: headers });
};
   
CompanyService.Delete = (item) => {
    return axios.post(appUrl + '/Maintenance/Company/Index?handler=Delete',
        JSON.stringify(item),
        { headers: headers });
};

CompanyService.Save = (item) => {
    return axios.post(appUrl + '/Maintenance/Company/Index?handler=Save',
        JSON.stringify(item),
        { headers: headers });
};
    