
var ReportService = {};

 
ReportService.LogHTTP = (filter, sortColumn, descending, pageNum, pageSize) => {
    var searchOption = { Filters: filter, SortColumn: sortColumn, Descending: descending, PageNum: pageNum, PageSize: pageSize };
    return axios.post(appUrl + '/Reports/LogHTTP?handler=Filter',
        JSON.stringify(searchOption),
        { headers: headers });
};

ReportService.LogHTTPDownload = (filter, sortColumn, descending, pageNum, pageSize) => {
    var searchOption = { Filters: filter, SortColumn: sortColumn, Descending: descending, PageNum: pageNum, PageSize: pageSize };
    return axios.post(appUrl + '/Reports/LogHTTP?handler=Download',
        JSON.stringify(searchOption),
        {
            headers: headers ,
            responseType: 'blob'
        });
   
};

ReportService.LogTransaction = (filter, sortColumn, descending, pageNum, pageSize) => {
    var searchOption = { Filters: filter, SortColumn: sortColumn, Descending: descending, PageNum: pageNum, PageSize: pageSize };
    return axios.post(appUrl + '/Reports/LogTransaction?handler=Filter',
        JSON.stringify(searchOption),
        { headers: headers });
};

ReportService.LogTransactionDownload = (filter, sortColumn, descending, pageNum, pageSize) => {
    var searchOption = { Filters: filter, SortColumn: sortColumn, Descending: descending, PageNum: pageNum, PageSize: pageSize };
    return axios.post(appUrl + '/Reports/LogTransaction?handler=Download',
        JSON.stringify(searchOption),
        {
            headers: headers,
            responseType: 'blob'
        });

};