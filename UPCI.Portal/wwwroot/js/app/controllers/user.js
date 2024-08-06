const { createApp, reactive, ref, computed, onMounted, filter } = Vue;
const userController = createApp({
    setup() {
        let isFormValid = ref(false);
        onMounted(() => {

        });
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
        const roles = ref([]);  

        const Search = async () => {
            
            datatable.filter = [];
            if ($('#searchDescription').val().trim() !== "")
                datatable.filter.push({ "Property": "FirstName", "Value": search.description, "Operator": "Contains" });
            await GetUser();
            
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
        const GetUser = async () => {

            //var filterDeleted = { "Property": "Deleted", "Value": true, "Operator": "NOTEQUALS" };
            //addFilterIfNotExists(datatable.filter, filterDeleted);
            $(".preloader").show(); 
            const result = await UserService.Search(datatable.filter, datatable.sortColumn, datatable.descending, datatable.pageNum, datatable.pageSize)

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
        const GetRole = async () => {
            const result = await RoleService.All()

            if (result.data != null) {
                roles.value = result.data;
            }
            else {
                roles.value = [];
            }

        }; 
        const Sync = async () => {
            if (formData.username.trim() == "")
            {
                swal.fire({
                    text: "Fill out the USERNAME field!",
                    icon: "error"
                });
                return;
            }
            const result = await UserService.CheckAD(formData);
            if (result.data != null) {
                if (result.data.username != "") {
                    formData.firstName = result.data.firstName;
                    formData.middleName = result.data.middleName;
                    formData.lastName = result.data.lastName;
                    formData.email = result.data.email;
                    swal.fire({
                        text: "AD User latest details successfully retrieved!",
                        icon: "success"
                    });
                }
                else
                {
                    formData.firstName = result.data.firstName;
                    formData.middleName = result.data.middleName;
                    formData.lastName = result.data.lastName;
                    formData.email = result.data.email;
                    swal.fire({
                        text: "AD User not exists!",
                        icon: "error"
                    });

                }
                
            }
            else {
                
                swal.fire({
                    text: "Error occured!",
                    icon: "error"
                });
            }
        }
        const Save = async () => {
            $('#form').parsley().validate();
            if ($('#form').parsley().isValid()) { 
                $(".preloader").show(); 
                const result = await UserService.Save(formData);
                if (result.data.status === 'SUCCESS') {
                    $('#formModal').modal('hide');
                
                    swal.fire({
                        html: "User successfully created! <br> User's Password <br>" + "<b class='text-danger'>" + result.data.message + "</b>",
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
                else
                {
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
                    UserService.Delete(item)
                        .then((result) => {
                            if (result.data.status === 'SUCCESS') {
                                swal.fire({
                                    text: "User successfully deleted!",
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
        const ResetPassword = (item) => {
            swal.fire({
                title: "Are you sure?",
                text: "This will reset the user's password",
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes'
            }).then((result) => {
                if (result.isConfirmed) {
                    UserService.ResetPassword(item)
                        .then((result) => {
                            if (result.data.status === 'SUCCESS') {
                                swal.fire({
                                    html: "User password successfully reset! <br> New Password <br>" + "<b class='text-danger'>" +result.data.message +"</b>",
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
        const UnlockUser = (item) => {
            swal.fire({
                title: "Are you sure?",
                text: "This will unlock the user access",
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes'
            }).then((result) => {
                if (result.isConfirmed) {
                    UserService.UnlockUser(item)
                        .then((result) => {
                            if (result.data.status === 'SUCCESS') {
                                swal.fire({
                                    text: "User access successfully unlocked!",
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
                    GetUser();
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
            formData.encryptedRoleId = ''; 
            formData.username = '';
            formData.firstName = '';
            formData.middleName = '';
            formData.lastName = '';
            formData.email = '';
            //formData = {};
        };

        const Edit = (item) => {
            $('#form').parsley().reset();
            actionMode.value = 'Modify' 
            disableControl.code = true;
            disableControl.firstName = true;
            disableControl.middleName = true;
            disableControl.lastName = true;
            disableControl.email = true;
            formData.id = item.id; 
            formData.encryptedRoleId = item.encryptedRoleId; 
            formData.username = item.username; 
            formData.firstName = item.firstName;
            formData.middleName = item.middleName;
            formData.lastName = item.lastName;
            formData.email = item.email;
        };



        // Execute function when Vue instance is created  
        GetUser();
        GetRole();
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
            roles, 
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
            ResetPassword,
            UnlockUser,
            Sort,
            SortClass,
            Sync,
        };
        return {
            ...returnProps,
            ...returnMethod

        };
    }
});

userController.mount('#UserController');