const { createApp, reactive, ref, computed, onMounted, filter } = Vue;
const homeController = createApp({
    setup() {
        let isFormValid = ref(false);
        onMounted(() => {

        });
        const actionMode = ref(''); 
        const datatable = reactive({
            pageNum: 1,
            pageSize: 10,
            sortColumn: 'CreatedDate',
            descending: false,
            filter: [],
            show_table: null,
        });
         
        const disableControl = reactive({});

        const formData = reactive({
        }); 
        const search = reactive({});

        const checkBoxes = reactive({
            mainCheckBox: false,
            childCheckBox: []
        });
        const items = ref([]);  
        const recentActivities = ref([]);  
        const Search = () => {
            $(".preloader").show(); 
            datatable.filter = [];
            
            GetRouteSummary();
            $('.preloader').fadeOut('slow');
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
        const GetRouteSummary = async () => {
             
            const result = await RouteService.Summary(datatable.filter, datatable.sortColumn, datatable.descending, datatable.pageNum, datatable.pageSize)

            if (result.data != null && result.data.length != 0) {
                items.value = result.data;
                datatable.show_table = true;

                if (result.data.totalPage > 1)
                    initPages(result.data.totalPage);
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
        const GetRecentSummary = async () => {

            const result = await UserService.RecentActivity()

            if (result.data != null && result.data.length != 0) {
                recentActivities.value = result.data; 
            }
            else {
                recentActivities.value = [];  
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
                    GetRoutes();
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
            datatable.filter = [];
            datatable.filter = [
                { "Property": "RegionCode", "Value": search.description, "Operator": "Contains" }
            ];
            datatable.pageNum = 1;
            Search();
        };
         
        // Execute function when Vue instance is created  
        GetRouteSummary();
        GetRecentSummary();
        const returnProps = {
            actionMode,
            isFormValid,
            datatable,
            disableControl,
            search,
            formData, 
            items,
            checkBoxes,   
            recentActivities,
        };

        // Return methods
        const returnMethod = {
            Search,
            ActionModeIcon,
            itemCountChange,
            ActionModeIcon, 
            Sort,
            SortClass,
        };
        return {
            ...returnProps,
            ...returnMethod

        };
    }
});

homeController.mount('#HomeController');