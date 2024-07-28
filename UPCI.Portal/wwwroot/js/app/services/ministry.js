
var MinistryService = {};

MinistryService.All = () => { 
    return axios.get(appUrl + '/Maintenance/Ministry/Index?handler=All');
};

MinistryService.Search = (filter, sortColumn, descending, pageNum, pageSize) => {
    var searchOption = { Filters: filter, SortColumn: sortColumn, Descending: descending, PageNum: pageNum, PageSize: pageSize };
    return axios.post(appUrl + '/Maintenance/Ministry/Index?handler=Filter',
        JSON.stringify(searchOption),
        { headers: headers });
};
   
MinistryService.Delete = (item) => {
    return axios.post(appUrl + '/Maintenance/Ministry/Index?handler=Delete',
        JSON.stringify(item),
        { headers: headers });
};

MinistryService.Save = (item) => {
    return axios.post(appUrl + '/Maintenance/Ministry/Index?handler=Save',
        JSON.stringify(item),
        { headers: headers });
};
    