const { createApp, reactive, ref, computed, onMounted, filter, nextTick } = Vue;
const controller = createApp({
    setup() {
        let isFormValid = ref(false);
        const search = reactive({
            dateFrom: '',
            dateTo:''
        });

        const initializeDatepicker = (selector, modelKey) => {
            $(selector).datepicker({
                changeMonth: true,
                changeYear: true,
                dateFormat: "yy-mm-dd",
                yearRange: "1900:2100",
                onSelect: (dateText) => {
                    search[modelKey] = dateText; // Update Vue.js model
                    nextTick(() => {
                        // Trigger Parsley validation manually
                        $('#formFilter').parsley().validate();
                    });
                }
            });
        };
        onMounted(() => {
            initializeDatepicker('#dateFrom', 'dateFrom');
            initializeDatepicker('#dateTo', 'dateTo');

            // Initialize Parsley validation
            $('#formFilter').parsley({
                trigger: 'change focusout'
            });
        });
        const actionMode = ref(''); 
        const datatable = reactive({
            pageNum: 1,
            pageSize: 10,
            sortColumn: 'RequestDate',
            descending: true,
            filter: [],
            show_table: null,
        });
         
        const disableControl = reactive({});

        const formData = reactive({
        });
        const formDataSecurity = reactive({
        });
       

        const checkBoxes = reactive({
            mainCheckBox: false,
            childCheckBox: []
        });
        const items = ref([]);  

        const Search = () => {
            $(".preloader").show(); 
            datatable.filter = [];
            if ($('#searchDescription').val().trim() !== "")
                datatable.filter.push({ "Property": "TraceId", "Value": search.description, "Operator": "Contains" });
            if ($('#dateFrom').val().trim() !== "" && $('#dateTo').val().trim() !== "")
                datatable.filter.push({ "Property": "RequestDate", "Value": search.dateFrom, "Value2": search.dateTo, "Operator": "Between", IsDate: true });
            if ($('#apiUser').val().trim() !== "")
                datatable.filter.push({ "Property": "Client", "Value": search.apiUser, "Operator": "Contains" });
            if ($('#apiName').val().trim() !== "")
                datatable.filter.push({ "Property": "Name", "Value": search.apiName, "Operator": "Contains" });
            datatable.pageNum = 1;


            GetItems();
            $('.preloader').fadeOut('slow');
        };
        const Download = async () => {
            const result = await ReportService.LogHTTPDownload(datatable.filter, datatable.sortColumn, datatable.descending, datatable.pageNum, datatable.pageSize);
            const blob = new Blob([result.data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
            const contentDisposition = result.headers['content-disposition'];
            let filename = 'Logs_HTTP.xlsx'; // Default filename in case the header is missing
            if (contentDisposition) {
                const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                const matches = filenameRegex.exec(contentDisposition);
                if (matches != null && matches[1]) {
                    filename = matches[1].replace(/['"]/g, '');
                }
            }

            // Use the saveAs function from FileSaver.js
            saveAs(blob, filename);
        };
        const addFilterIfNotExists = (filters, newFilter) => {
            if (!filters.some(filter =>
                filter.Property === newFilter.Property &&
                filter.Value === newFilter.Value &&
                filter.Operator === newFilter.Operator
            )) {
                filters.push(newFilter);
            }
        };
        const GetItems = async () => {

            //var filterDeleted = { "Property": "Deleted", "Value": true, "Operator": "NOTEQUALS" };
            //addFilterIfNotExists(datatable.filter, filterDeleted);
            const result = await ReportService.LogHTTP(datatable.filter, datatable.sortColumn, datatable.descending, datatable.pageNum, datatable.pageSize)

            if (result.data != null && result.data.data.length != 0) {
                items.value = result.data.data;
                datatable.show_table = true;
                if (datatable.pageNum > result.data.totalPage) {
                    datatable.pageNum = result.data.totalPage;
                    initPages(result.data.totalPage);
                } else if (result.data.totalPage > 1) {
                    initPages(result.data.totalPage);
                } else {
                    try {
                        $('#sync-pagination').twbsPagination('destroy');
                    }
                    catch (error) { }
                }
            }
            else {
                items.value = [];
                datatable.show_table = false;
                try {
                    $('#sync-pagination').twbsPagination('destroy');
                }
                catch (error) { }
            }
             
        };
        
        // Table Events
        const itemCountChange = () => {
            Search();
        }
        const initPages = (tp) => { 
            $('#sync-pagination').twbsPagination('destroy');
            $('#sync-pagination').twbsPagination({
                totalPages: tp,
                initiateStartPageClick: false,
                hideOnlyOnePage: true,
                startPage: datatable.pageNum,
                onPageClick: (evt, page) => {
                    datatable.pageNum = page;
                    GetItems();
                }
            });
        };

        const Sort = (col) => {
            datatable.sortColumn = col;

            if (datatable.descending) {
                datatable.descending = false;
            } else {
                datatable.descending = true;
            }

            Search();
        };

        const SortClass = (col) => {
            if (datatable.sortColumn === col) {
                if (datatable.descending) {
                    return 'fa-sort-up';
                } else {
                    return 'fa-sort-down';
                }
            }
            return 'fa fa-sort';
        };
        const ActionModeIcon = () => {

            if (actionMode.value == 'Add') {
                return 'fa-plus-circle';
            }
            else {
                return 'fa-edit';
            }

        };
        const KeyPress_Search = (e) => {
            if (e.which == 13) {
                Search();
            }
        };

        const Filter = () => {
            datatable.filter = [];
        };
        const ApplyFilter = () => {
            $('#formFilter').parsley().validate();
             
            if ($('#formFilter').parsley().isValid())
            {
                $('#filterModal').modal('hide');
                Search();

            }
        };
        // Execute function when Vue instance is created  
        GetItems(); 
        const returnProps = {
            actionMode,
            isFormValid,
            datatable,
            disableControl,
            search,
            formData,
            formDataSecurity,
            items, 
            
        };

        // Return methods
        const returnMethod = {
            Search,
            ActionModeIcon,
            itemCountChange,
            ActionModeIcon, 
            Sort,
            SortClass,
            Download,
            Filter, 
            ApplyFilter,
        };
        return {
            ...returnProps,
            ...returnMethod

        };
    }
});

controller.mount('#LogHTTPController');