
var ModuleService = {};

ModuleService.AllModule = () => {
    return axios.get(appUrl + '/Maintenance/Module/Index?handler=All');
};

ModuleService.AllModuleAction = () => {
    return axios.get(appUrl + '/Maintenance/Module/Index?handler=AllModuleAction');
};
ModuleService.AllParent = () => {
    return axios.get(appUrl + '/Maintenance/Module/Index?handler=AllParent');
};

ModuleService.Search = (filter, sortColumn, descending, pageNum, pageSize) => {
    var searchOption = { Filters: filter, SortColumn: sortColumn, Descending: descending, PageNum: pageNum, PageSize: pageSize };
    return axios.post(appUrl + '/Maintenance/Module/Index?handler=Filter',
        JSON.stringify(searchOption),
        { headers: headers });
};

ModuleService.Delete = (item) => {
    return axios.post(appUrl + '/Maintenance/Module/Index?handler=Delete',
        JSON.stringify(item),
        { headers: headers });
};

ModuleService.Save = (item) => {
    return axios.post(appUrl + '/Maintenance/Module/Index?handler=Save',
        JSON.stringify(item),
        { headers: headers });
};
