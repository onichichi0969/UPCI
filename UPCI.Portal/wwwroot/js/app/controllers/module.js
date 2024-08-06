const { createApp, reactive, ref, computed, onMounted, filter } = Vue;
const moduleController = createApp({
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
        const formDataSecurity = reactive({
        });
        const search = reactive({});

        const checkBoxes = reactive({
            mainCheckBox: false,
            childCheckBox: []
        });
        const items = ref([]); 
        const moduleActions = ref([]);
        const parentIds = ref([]);
        const selectedModuleActions = ref([]);

        const Search = async () => {
            
            datatable.filter = [];
            if ($('#searchDescription').val().trim() !== "")
                datatable.filter.push({ "Property": "Name", "Value": search.description, "Operator": "Contains" });
            await GetModule();
           
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
        const GetModule = async () => {
            var filterDeleted = { "Property": "Deleted", "Value": true, "Operator": "NOTEQUALS" };
            addFilterIfNotExists(datatable.filter, filterDeleted);
            $(".preloader").show(); 
            const result = await ModuleService.Search(datatable.filter, datatable.sortColumn, datatable.descending, datatable.pageNum, datatable.pageSize)

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
            $('.preloader').fadeOut('slow');
        };
        const GetModuleAction = async () => {
            const result = await ModuleService.AllModuleAction()
            if (result.data != null) {
                moduleActions.value = result.data;
            }
            else {
                moduleActions.value = [];
            }
        }; 
        const GetParent = async () => {
            const result = await ModuleService.AllParent()
            if (result.data != null) {
                parentIds.value = result.data;
            }
            else {
                parentIds.value = [];
            }
        }; 
        const Save = async () => {
            $('#form').parsley().validate();
            if ($('#form').parsley().isValid()) {

                var filtered = selectedModuleActions.value.filter(function (el) {
                    return el != '';
                });
                formData.action = filtered.sort().join("|");
                $(".preloader").show(); 
                const result = await ModuleService.Save(formData);
                if (result.data.status === 'SUCCESS') {
                    $('#formModal').modal('hide');
                    swal.fire({
                        text: "Module successfully saved!",
                        icon: "success"
                    });
                    Search();
                    GetParent();
                }
                else if (result.data.status === 'FAILED') {
                    swal.fire({
                        text: result.data.message,
                        icon: "error"
                    });
                }
                else
                {
                    swal.fire({
                        icon: 'error',
                        title: 'Oops...',
                        text: 'Error encountered'
                    });
                    } 
            }
            else {
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
                    ModuleService.Delete(item)
                        .then((result) => {
                            if (result.data.status === 'SUCCESS') {
                                swal.fire({
                                    text: "Module successfully deleted!",
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
                    GetModule();
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
            actionMode.value = 'Add'
            disableControl.code = false; 
            formData.id = ''; 
            formData.parentId = ''; 
            formData.code = ''; 
            formData.name = ''; 
            formData.moduleType = ''; 
            formData.displayOrder = ''; 
            formData.description = ''; 
            formData.url = ''; 
            formData.icon = ''; 
            formData.auditContent = '';
            formData.show = false;
            CheckModuleActionExists('');

            formData.action = '';
        };

        const Edit = (item) => {
            $('#form').parsley().reset();
            
            actionMode.value = 'Modify' 
            disableControl.code = true; 
            formData.id = item.id; 

            formData.parentId = '';
            for (let x = 0; x < parentIds.value.length; x++)
            {
                if (parentIds.value[x].id == item.parentId)
                {
                    formData.parentId = item.parentId; 
                }
            } 
                
            formData.code = item.code;
            formData.name = item.name;
            formData.moduleType = item.moduleType;
            formData.displayOrder = item.displayOrder;
            formData.description = item.description;
            formData.url = item.url;
            formData.icon = item.icon;
            formData.auditContent = item.auditContent;
            formData.show = item.show;
            CheckModuleActionExists(item.action); 
            formData.action = selectedModuleActions.value.join("|");
        };
        const toggleModuleCheckbox = (value) => {
            // Check if the string exists in the array
            var index = selectedModuleActions.value.indexOf(value);

            if (index !== -1) {
                // If the string exists, remove it
                selectedModuleActions.value.splice(index, 1);
            } else {
                // If the string doesn't exist, add it
                selectedModuleActions.value.push(value);
            }
        };

        const CheckModuleActionExists = (currentModuleActionList) => {
            const allModuleArray = moduleActions.value;
            const splittedCurrentArray = currentModuleActionList.split("|");
            selectedModuleActions.value = splittedCurrentArray;
            allModuleArray.forEach((item) => {
                const inputElement = document.querySelector(`input[value="${item.code}"]`);
                if (splittedCurrentArray.includes(item.code)) {
                    inputElement.checked = true;
                }
                else {
                    inputElement.checked = false;
                }
            });

        };

        
        // Execute function when Vue instance is created  
        GetModule(); 
        GetModuleAction();
        GetParent();
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
            moduleActions,
            selectedModuleActions,
            parentIds, 
            
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
            Save,
            toggleModuleCheckbox, 
            SortClass,
            Sort,
        };
        return {
            ...returnProps,
            ...returnMethod

        };
    }
});

moduleController.mount('#ModuleController');