const { createApp, reactive, ref, computed, onMounted, filter } = Vue;
const routeController = createApp({
    setup() {
        let isFormValid = ref(false);
        onMounted(() => {

        });
        const httpMethodValid = ref(false);
        const actionMode = ref('');
        //const datatable = reactive({
        //    pageNum: 1,
        //    pageSize: 10,
        //    column: '',
        //    reverse: false,
        //    show_table: null,
        //    sortOrder: null,
        //    filter: [],
        //    fields: '*',
        //});
        const datatable = reactive({
            pageNum: 1,
            pageSize: 10,
            sortColumn: 'CreatedDate',
            descending: false,
            filter: [],
            show_table: null,
        });

        const upstreamHTTPMethod = ref([]);
         
        const disableControl = reactive({});

        const formData = reactive({
        });
        const formDataSecurity = reactive({
        });
        const search = reactive({});

        const checkBoxes = reactive({
            mainCheckBox: false,
            childCheckBox: []
        });
        const items = ref([]);
        const allClients = ref([]);
        const selectedClients = ref([]);

        const selectedIPallowed = ref([]);

        const application = ref([]);
 
        const Search = () => {
            $(".preloader").show(); 
            datatable.filter = [];
            if ($('#searchDescription').val().trim() !== "")
                datatable.filter.push({ "Property": "Name", "Value": search.description, "Operator": "Contains" });
            GetRoutes();
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
        const GetRoutes = async () => {

            //var filterDeleted = { "Property": "Deleted", "Value": true, "Operator": "NOTEQUALS" };
            //addFilterIfNotExists(datatable.filter, filterDeleted);
            const result = await RouteService.Search(datatable.filter, datatable.sortColumn, datatable.descending, datatable.pageNum, datatable.pageSize)

            if (result.data != null && result.data.data.length != 0) {
                items.value = result.data.data;
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
        const GetClients = async () => {
            const result = await APIClientService.AllWithDeleted()

            if (result.data != null) {
                allClients.value = result.data; 
            }
            else {
                allClients.value = []; 
            }
             
        };
        const Publish = async () => {
            swal.fire({
                title: "Are you sure?",
                text: "Once published, this will be immediately in effect",
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, publish it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    RouteService.PublishOcelot()
                        .then((result) => {
                            if (result.data.status === 'SUCCESS') {
                                swal.fire({
                                    text: "Route successfully published!",
                                    icon: "success"
                                });

                                Search();
                            }
                            else if (result.data.status === 'FAILED') {
                                swal.fire({
                                    icon: 'error',
                                    text: result.data.message
                                });
                            }
                            else {
                                swal.fire({
                                    icon: 'error',
                                    title: 'Oops...',
                                    text: 'Error encountered'
                                });
                            }
                        });
                }
            }); 
        }
        const Save = async () => {
            $('#form').parsley().validate();
            validateCheckboxes();
            if ($('#form').parsley().isValid()) {
                if (!httpMethodValid.value)
                {
                    swal.fire({
                        text: "Fill out the required fields!",
                        icon: "warning"
                    });
                    return;
                }
                if (selectedClients.value.length == 0)
                {
                    swal.fire({
                        text: "You must add clients to Client Whitelist",
                        icon: "warning"
                    });
                    return;
                }
                if (selectedIPallowed.value.length == 0) {
                    swal.fire({
                        text: "You must add IP's to IP Whitelist",
                        icon: "warning"
                    });
                    return;
                }
                var httpMethods = ConstructHTTPMethod();
                var clientList = ConstructClientList();
                var ipAllowedList = ConstructIPAllowedList();
                formData.upstreamHttpMethod = httpMethods;
                formData.clientWhitelist = clientList;
                formData.IPAllowedList = ipAllowedList;
                $(".preloader").show(); 
                const result = await RouteService.Save(formData);
                if (result.data.status === 'SUCCESS') {
                    $('#formModal').modal('hide');
                    swal.fire({
                        text: "Route successfully saved!",
                        icon: "success"
                    });
                    Search();
                }
                else if (result.data.status === 'FAILED') {
                    swal.fire({
                        text: result.data.message,
                        icon: "error"
                    });
                }
                else {
                    swal.fire({
                        icon: 'error',
                        title: 'Oops...',
                        text: 'Error encountered'
                    });
                }
            } 
            else 
            {
                swal.fire({
                    text: "Fill out the required fields!",
                    icon: "warning"
                });
            }
            $('.preloader').fadeOut('slow');
        }
        const Delete = (item) => {
            swal.fire({
                title: "Are you sure?",
                text: "Once delete, this will not be accessible on the system!",
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, delete it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    RouteService.Delete(item)
                        .then((result) => {
                            if (result.data.status === 'SUCCESS') {
                                swal.fire({
                                    text: "Route successfully deleted!",
                                    icon: "success"
                                });

                                Search();
                            }
                            else if (result.data.status === 'FAILED') {
                                swal.fire({
                                    icon: 'error',
                                    text: result.data.message
                                });
                            }
                            else {
                                swal.fire({
                                    icon: 'error',
                                    title: 'Oops...',
                                    text: 'Error encountered'
                                });
                            }
                        });
                }
            });
        }
        const validateCheckboxes = () => {
            var isChecked = false;
            httpMethodValid.value = false;
            const checkboxes = document.querySelectorAll('input[type="checkbox"][name="httpMethods"]');
            checkboxes.forEach(function (checkbox) {
                if (checkbox.checked) {
                    isChecked = true;
                    httpMethodValid.value = true;
                }
            });
            return isChecked;
        }
        const ConstructHTTPMethod = () => {
            var httpMethods = [];

            if (formData.get)
                httpMethods.push("GET");
            if (formData.post)
                httpMethods.push("POST");
            if (formData.put)
                httpMethods.push("PUT");
            if (formData.patch)
                httpMethods.push("PATCH");
            if (formData.delete)
                httpMethods.push("DELETE");

            return httpMethods.join('|');
        }
        const ConstructClientList = () => {
            var clientList = [];
            for (let x = 0; x < selectedClients.value.length; x++)
            {
                clientList.push(selectedClients.value[x].id);
            }
            return clientList.join('|');
        }
        const ConstructIPAllowedList = () => {
            var allowedIP = [];
            for (let x = 0; x < selectedIPallowed.value.length; x++) {
                allowedIP.push(selectedIPallowed.value[x]);
            }
            return allowedIP.join('|');
        }
        const DeconstructIPAllowedList = (ipAllowedList) => {
            selectedIPallowed.value = ipAllowedList.split('|');
        }
        const DeconstructClientList = (clientList) => {
            var clientArray = clientList.split('|');
            selectedClients.value = [];
            for (let x = 0; x < allClients.value.length; x++) { 
                if (clientArray.includes(allClients.value[x].id))
                    selectedClients.value.push(allClients.value[x]) 
            } 
        }
        const EnableUpstreamMethods = (httpmethods) =>
        {
            if (httpmethods.includes("GET"))
                formData.get = true;
            if (httpmethods.includes("POST"))
                formData.post = true;
            if (httpmethods.includes("PUT"))
                formData.put = true;
            if (httpmethods.includes("PATCH"))
                formData.patch = true;
            if (httpmethods.includes("DELETE"))
                formData.delete = true; 
        }
        const AddClient = () => {
            formDataSecurity.client = '';
        }
        const SaveClient = () => { 
            var id = formDataSecurity.client.trim();  
            var client = {};

            for (let x = 0; x < allClients.value.length; x++)
            { 
                if (id == allClients.value[x].id) 
                    client = allClients.value[x]; 
            }

            if (!selectedClients.value.find(c => c.id === id)) {
                selectedClients.value.push(client);
                swal.fire({
                    text: "Client added!",
                    icon: "success"
                });
                formDataSecurity.client = "";
            } else {
                swal.fire({
                    text: "Client is already exists!",
                    icon: "error"
                });
            }  

        }
        

        const SaveIP = () => { 
            $('#formModalIPWhitelist').parsley().validate();
            if ($('#formModalIPWhitelist').parsley().isValid()) {
                var ip = formDataSecurity.ip.trim();

                if (!selectedIPallowed.value.includes(ip)) {
                    selectedIPallowed.value.push(ip);
                    swal.fire({
                        text: "IP Address added!",
                        icon: "success"
                    });
                    formDataSecurity.ip = "";
                } else {
                    swal.fire({
                        text: "IP Address is already exists!",
                        icon: "error"
                    });
                } 
            } 
        }
 
        const RemoveIP = (ip) => {
            if (selectedIPallowed.value.includes(ip)) {
                const index = selectedIPallowed.value.indexOf(ip);
                selectedIPallowed.value.splice(index, 1);
            } 
        }
        const RemoveClient = (client) => {
            if (selectedClients.value.find(c => c.id === client.id)) {
                const index = selectedClients.value.findIndex(c => c.id === client.id);
                selectedClients.value.splice(index, 1);
            }
        }
        
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

        const Add = () => {
            $('#form').parsley().reset();
            httpMethodValid.value = true;
            actionMode.value = 'Add'
            //disableControl.code = false; 
            formData.id = '';
            formData.get = false;
            formData.post = false;
            formData.put = false;
            formData.patch = false;
            formData.delete = false;
            formData.name = '';
            formData.authenticationProviderKey = '';
            formData.downstreamHostAndPorts = '';
            formData.downstreamPathTemplate = '';
            formData.downstreamScheme = '';
            formData.upstreamPathTemplate = '';
            formData.enableRateLimiting = false;
            formData.ratePeriod = '';
            formData.rateLimit = '';
            formData.ratePeriodTimespan = '';
            formData.enableTimeLimit = false;
            formData.timeFrom = '';
            formData.timeTo = '';
            selectedClients.value = [];
            selectedIPallowed.value = [];
            /*formData = {};*/
        };

        const Edit = (item) => {
            $('#form').parsley().reset();
            httpMethodValid.value = true;
            actionMode.value = 'Modify' 
            //disableControl.code = true; 
            formData.id = item.id;
            EnableUpstreamMethods(item.upstreamHttpMethod);
            DeconstructIPAllowedList(item.ipAllowedList);
            DeconstructClientList(item.clientWhitelistId);
            
            formData.name = item.name; 
            formData.authenticationProviderKey = item.authenticationProviderKey;
            formData.downstreamHostAndPorts = item.downstreamHostAndPorts;
            formData.downstreamPathTemplate = item.downstreamPathTemplate;
            formData.downstreamScheme = item.downstreamScheme;
            formData.upstreamPathTemplate = item.upstreamPathTemplate;
            formData.enableRateLimiting = item.enableRateLimiting;
            formData.ratePeriod = item.ratePeriod;
            formData.rateLimit = item.rateLimit;
            formData.ratePeriodTimespan = item.ratePeriodTimespan;
            formData.enableTimeLimit = item.enableTimeLimit;
            formData.timeFrom = item.timeFrom;
            formData.timeTo = item.timeTo; 
        };



        // Execute function when Vue instance is created 
        GetRoutes(); 
        GetClients();
        const returnProps = {
            actionMode,
            isFormValid,
            datatable,
            disableControl,
            search,
            formData,
            formDataSecurity,
            items,
            checkBoxes, 
            allClients,
            selectedClients,
            selectedIPallowed,
            httpMethodValid,
            
        };

        // Return methods
        const returnMethod = {
            Search,
            ActionModeIcon,
            itemCountChange,
            ActionModeIcon,
            Add,
            Edit,
            Delete,
            SaveIP,
            AddClient,
            SaveClient,
            RemoveIP,
            RemoveClient,
            Publish,
            Save,
            Sort,
            SortClass,
            validateCheckboxes,
        };
        return {
            ...returnProps,
            ...returnMethod

        };
    }
});

routeController.mount('#RouteController');