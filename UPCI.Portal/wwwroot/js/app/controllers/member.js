const { createApp, reactive, ref, computed, onMounted, filter, nextTick } = Vue;
const membershipController = createApp({
    setup() {
        let isFormValid = ref(false);
        const search = reactive({
            dateFrom: '',
            dateTo: ''
        });
        const formData = reactive({
        });
        const initializeDatepickerForm = (selector, modelKey) => {
            $(selector).datepicker({
                changeMonth: true,
                changeYear: true,
                dateFormat: "yy-mm-dd",
                yearRange: "1900:2100",
                onSelect: (dateText) => {
                    formData[modelKey] = dateText; // Update Vue.js model
                    nextTick(() => {
                        // Trigger Parsley validation manually
                        $('#form').parsley().validate();
                    });
                }
            });
        };
        
        onMounted(() => {
            initializeDatepickerForm('#dateFirstAttend', 'firstAttend');
            initializeDatepickerForm('#dateBaptized', 'baptismDate');
            initializeDatepickerForm('#dateBday', 'birthday');
            // Initialize Parsley validation
            $('#form').parsley({
                trigger: 'change focusout'
            });
        });
        const actionMode = ref('');
        const datatable = reactive({
            pageNum: 1,
            pageSize: 10,
            sortColumn: 'CreatedDate',
            descending: true,
            filter: [],
            show_table: null,
        });

        const disableControl = reactive({});

       
        const formDataSecurity = reactive({
        });
        const checkBoxes = reactive({
            mainCheckBox: false,
            childCheckBox: []
        });
        const items = ref([]);  
        const cell = ref([]);
        const ministry = ref([]);


        const Search = () => {
            $(".preloader").show();
            datatable.filter = [];
            if ($('#searchCode').val().trim() !== "")
                datatable.filter.push({ "Property": "Code", "Value": search.code, "Operator": "Contains" });
            GetMember();
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
        const GetMember = async () => {

            var filterDeleted = { "Property": "Deleted", "Value": true, "Operator": "NOTEQUALS" };
            addFilterIfNotExists(datatable.filter, filterDeleted);
            const result = await MemberService.Search(datatable.filter, datatable.sortColumn, datatable.descending, datatable.pageNum, datatable.pageSize)

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
       
        const GetCell = async () => { 
            const result = await CellService.All() 
            if (result.data != null) {
                cell.value = result.data;
            }
            else {
                cell.value = [];
            }

        };
        const GetMinistry = async () => {
            const result = await MinistryService.All()
            if (result.data != null) {
                ministry.value = result.data;
            }
            else {
                ministry.value = [];
            }

        };
        const Save = async () => {
            $('#form').parsley().validate();
            if ($('#form').parsley().isValid()) {
                $(".preloader").show();
                const result = await MemberService.Save(formData);
                if (result.data.status === 'SUCCESS') {
                    $('#formModal').modal('hide');
                    swal.fire({
                        text: "Member successfully saved!",
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
                    MemberService.Delete(item)
                        .then((result) => {
                            if (result.data.status === 'SUCCESS') {
                                swal.fire({
                                    text: "Member successfully deleted!",
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

            if ($('#formFilter').parsley().isValid()) {
                $('#filterModal').modal('hide');
                Search();

            }
        };
        const Add = () => {
            $('#form').parsley().reset();
            actionMode.value = 'Add'
            disableControl.code = false;
            formData.id = '';
            formData.activeMember = false;
            formData.involvedToCell = false;
            formData.baptized = false;
            formData.memberType = 'NEW';
            formData.pepsol = '';
            formData.baptismDate = '';
            formData.firstAttend = '';
            formData.gender = '';
            formData.civilStatus = '';
            formData.firstName = '';
            formData.middleName = '';
            formData.lastName = '';
            formData.birthday = '';
            formData.email = '';
            formData.contactNo = '';
            formData.address = '';
            //formData = {};
        };

        const Edit = (item) => {
            $('#form').parsley().reset();
            actionMode.value = 'Modify'
            disableControl.code = true;
            formData.id = item.id; 
            formData.code = item.code;
            formData.activeMember = item.activeMember;
            formData.involvedToCell = item.involvedToCell;
            formData.baptized = item.baptized;
            formData.memberType = item.memberType;
            formData.pepsol = item.pepsol;
            formData.baptismDate = item.baptismDate;
            formData.firstAttend = item.firstAttend;
            formData.gender = item.gender;
            formData.civilStatus = item.civilStatus;
            formData.firstName = item.firstName;
            formData.middleName = item.middleName;
            formData.lastName = item.lastName;
            formData.birthday = item.birthday;
            formData.email = item.email;
            formData.contactNo = item.contactNo;
            formData.address = item.address;

        };
        const FormatDate = (dateString) => {
            if (!dateString) return ''; // Check for blank date value
            try {
                // Convert the string to a Date object
                const date = new Date(dateString);

                // Check if the date is valid
                if (isNaN(date)) throw new Error('Invalid date');

                // Format the date
                const options = { year: 'numeric', month: 'long', day: '2-digit' };
                const formattedDate = date.toLocaleDateString('en-US', options); 

                return `${formattedDate}`;
            } catch (error) {
                console.error('Error formatting date:', error);
                return 'Invalid date '; // Return empty string if an error occurs
            }
        }
        const FormatDateBirthday = (dateString) =>{
            if (!dateString) return ''; // Check for blank date value
            try
            {
                // Convert the string to a Date object
                const date = new Date(dateString);

                // Check if the date is valid
                if (isNaN(date)) throw new Error('Invalid date');

                // Format the date
                const options = { year: 'numeric', month: 'long', day: '2-digit' };
                const formattedDate = date.toLocaleDateString('en-US', options);

                // Calculate the age
                const today = new Date();
                let age = today.getFullYear() - date.getFullYear();
                const monthDiff = today.getMonth() - date.getMonth();

                if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < date.getDate())) {
                    age--;
                }

                return `${formattedDate} (Age: ${age})`;
            } catch (error) {
                console.error('Error formatting date:', error);
                return 'Invalid date '; // Return empty string if an error occurs
            }
        }
        GetCell();
        GetMinistry();
        GetMember();
        const returnProps = {
            actionMode,
            isFormValid,
            datatable,
            disableControl,
            search,
            formData, 
            items,
            cell,
            ministry,

        };

        // Return methods
        const returnMethod = {
            Search,
            ActionModeIcon,
            itemCountChange,
            ActionModeIcon,
            Sort,
            SortClass, 
            Filter,
            ApplyFilter,
            Save,
            Delete,
            Add,
            Edit,
            FormatDate,
            FormatDateBirthday,
        };
        return {
            ...returnProps,
            ...returnMethod

        };
    }
});
membershipController.mount('#MembersController');