
var RouteService = {};

RouteService.Search = (filter, sortColumn, descending, pageNum, pageSize) => {
    var searchOption = { Filters: filter, SortColumn: sortColumn, Descending: descending, PageNum: pageNum, PageSize: pageSize };
    return axios.post(appUrl + '/Application/Route/Index?handler=Filter',
        JSON.stringify(searchOption),
        { headers: headers });
};

RouteService.Summary = (filter, sortColumn, descending, pageNum, pageSize) => {
    var searchOption = { Filters: filter, SortColumn: sortColumn, Descending: descending, PageNum: pageNum, PageSize: pageSize };
    return axios.post(appUrl + '/Application/Route/Index?handler=FilterRouteSummary',
        JSON.stringify(searchOption),
        { headers: headers });
};
   
RouteService.Delete = (encryptedId) => {
    return axios.post(appUrl + '/Application/Route/Index?encryptedId=' + encryptedId + '&handler=Delete',
        JSON.stringify(encryptedId),
        { headers: headers });
};

RouteService.Save = (route) => {
    return axios.post(appUrl + '/Application/Route/Index?handler=Save',
        JSON.stringify(route),
        { headers: headers });
};

RouteService.PublishOcelot = () => {
    return axios.get(appUrl + '/Application/Route/Index?handler=PublishOcelot');
};


 