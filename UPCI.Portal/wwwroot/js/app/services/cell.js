
var CellService = {};

CellService.All = () => { 
    return axios.get(appUrl + '/Maintenance/Cell/Index?handler=All');
};

CellService.Search = (filter, sortColumn, descending, pageNum, pageSize) => {
    var searchOption = { Filters: filter, SortColumn: sortColumn, Descending: descending, PageNum: pageNum, PageSize: pageSize };
    return axios.post(appUrl + '/Maintenance/Cell/Index?handler=Filter',
        JSON.stringify(searchOption),
        { headers: headers });
};
   
CellService.Delete = (item) => {
    return axios.post(appUrl + '/Maintenance/Cell/Index?handler=Delete',
        JSON.stringify(item),
        { headers: headers });
};

CellService.Save = (item) => {
    return axios.post(appUrl + '/Maintenance/Cell/Index?handler=Save',
        JSON.stringify(item),
        { headers: headers });
};
    