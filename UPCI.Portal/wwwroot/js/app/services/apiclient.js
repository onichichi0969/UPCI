
var APIClientService = {};

APIClientService.AllNoDeleted = () => { 
    return axios.get(appUrl + '/Maintenance/APIClient/Index?handler=AllNoDeleted');
};

APIClientService.AllWithDeleted = () => {
    return axios.get(appUrl + '/Maintenance/APIClient/Index?handler=AllWithDeleted');
};

APIClientService.Search = (filter, sortColumn, descending, pageNum, pageSize) => {
    var searchOption = { Filters: filter, SortColumn: sortColumn, Descending: descending, PageNum: pageNum, PageSize: pageSize };
    return axios.post(appUrl + '/Maintenance/APIClient/Index?handler=Filter',
        JSON.stringify(searchOption),
        { headers: headers });
};
   
APIClientService.Delete = (item) => {
    return axios.post(appUrl + '/Maintenance/APIClient/Index?handler=Delete',
        JSON.stringify(item),
        { headers: headers });
};

APIClientService.Save = (item) => {
    return axios.post(appUrl + '/Maintenance/APIClient/Index?handler=Save',
        JSON.stringify(item),
        { headers: headers });
};
APIClientService.Reset = (item) => {
    return axios.post(appUrl + '/Maintenance/APIClient/Index?handler=Reset',
        JSON.stringify(item),
        { headers: headers });
};
    