
var PositionCellService = {};

var PositionMinistryService = {};

PositionCellService.All = () => { 
    return axios.get(appUrl + '/Maintenance/Position/PositionCell?handler=All');
};

PositionCellService.Search = (filter, sortColumn, descending, pageNum, pageSize) => {
    var searchOption = { Filters: filter, SortColumn: sortColumn, Descending: descending, PageNum: pageNum, PageSize: pageSize };
    return axios.post(appUrl + '/Maintenance/Position/PositionCell?handler=Filter',
        JSON.stringify(searchOption),
        { headers: headers });
};
   
PositionCellService.Delete = (item) => {
    return axios.post(appUrl + '/Maintenance/Position/PositionCell?handler=Delete',
        JSON.stringify(item),
        { headers: headers });
};

PositionCellService.Save = (item) => {
    return axios.post(appUrl + '/Maintenance/Position/PositionCell?handler=Save',
        JSON.stringify(item),
        { headers: headers });
};
PositionMinistryService.All = () => {
    return axios.get(appUrl + '/Maintenance/Position/PositionMinistry?handler=All');
};
PositionMinistryService.Search = (filter, sortColumn, descending, pageNum, pageSize) => {
    var searchOption = { Filters: filter, SortColumn: sortColumn, Descending: descending, PageNum: pageNum, PageSize: pageSize };
    return axios.post(appUrl + '/Maintenance/Position/PositionMinistry?handler=Filter',
        JSON.stringify(searchOption),
        { headers: headers });
};

PositionMinistryService.Delete = (item) => {
    return axios.post(appUrl + '/Maintenance/Position/PositionMinistry?handler=Delete',
        JSON.stringify(item),
        { headers: headers });
};

PositionMinistryService.Save = (item) => {
    return axios.post(appUrl + '/Maintenance/Position/PositionMinistry?handler=Save',
        JSON.stringify(item),
        { headers: headers });
};