const { createApp, reactive, ref, computed, onMounted, filter, nextTick, watch } = Vue;
const cellController = createApp({
    setup() {
        let isFormValid = ref(false);
        onMounted(() => {
        });
        const actionMode = ref('');
        const selectedCellCode = ref(''); 
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
        const formDataMember = reactive({
        }); 
        const search = reactive({});

        const checkBoxes = reactive({
            mainCheckBox: false,
            childCheckBox: []
        });
        const items = ref([]); 
        const members = ref([]);
        const memberPositions = ref([]);
        const departments = ref([]);
        const selectedMembers = ref([]); 

        const Search = async () => {
           
            datatable.filter = [];
            if ($('#searchDescription').val().trim() !== "")
                datatable.filter.push({ "Property": "Description", "Value": search.description, "Operator": "Contains" });
            await GetCell();
            
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
        const GetMemberCodeAndName = async (list) => {
            const result = await MemberService.GetCodeAndName(list);

            if (result.data != null) {
                members.value = result.data;
            }
            else {
                members.value = [];
            }
            nextTick(() => {
                try {
                    $('.selectpicker').selectpicker('refresh');
                }
                catch (ex) {

                }
            });

        };
        const GetPosition = async () => {
            const result = await PositionCellService.All();

            if (result.data != null) {
                memberPositions.value = result.data;
            }
            else {
                memberPositions.value = [];
            }
            nextTick(() => {
                try {
                    $('.selectpicker').selectpicker('refresh');
                }
                catch (ex) {

                }
            });

        };
        const GetCell = async () => {

            var filterDeleted = { "Property": "Deleted", "Value": true, "Operator": "NOTEQUALS" };
            addFilterIfNotExists(datatable.filter, filterDeleted);
            $(".preloader").show(); 
            const result = await CellService.Search(datatable.filter, datatable.sortColumn, datatable.descending, datatable.pageNum, datatable.pageSize)

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
        const ConstructCells = (cellCode) => {
            var memberList = [];
            for (let x = 0; x < selectedMembers.value.length; x++) {
                memberList.push(
                    {
                        cellCode: cellCode,
                        memberCode: selectedMembers.value[x].memberCode,
                        positionCellCode: selectedMembers.value[x].positionCode 
                    }
                );
            }
            return memberList;
        }
        const Save = async () => {
            $('#form').parsley().validate();
            if ($('#form').parsley().isValid()) {
                $(".preloader").show();
                const result = await CellService.Save(formData);
                if (result.data.status === 'SUCCESS') {
                    $('#formModal').modal('hide');
                    swal.fire({
                        text: "Cell successfully saved!",
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
        };
        
        const SaveMembers = async () => {
            $('#member').parsley().validate();
            if ($('#member').parsley().isValid()) {
                $(".preloader").show();
                var memberList = ConstructCells(selectedCellCode.value);
                var data = {
                    cellCode: selectedCellCode.value,
                    cellMembers: memberList,
                    isMembersChanged: formDataMember.memberChanged,
                };
                const result = await CellService.SaveMembers(data);
                if (result.data.status === 'SUCCESS') {
                    $('#member').modal('hide');
                    swal.fire({
                        text: "Member successfully saved!",
                        icon: "success"
                    });
                    $('#addMemberModal').modal('hide');
                    $('#memberModal').modal('hide');

                    Search();
                }
                else if (result.data.status === 'INFO') {
                    $('#member').modal('hide');
                    swal.fire({
                        text: "No changes made",
                        icon: "info"
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
                    CellService.Delete(item)
                        .then((result) => {
                            if (result.data.status === 'SUCCESS') {
                                swal.fire({
                                    text: "Cell successfully deleted!",
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
                    GetCell();
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
            formData.code = '';
            formData.name = '';
            formData.description = '';
            //formData = {};
        };
        const MemberModal = (item) => {
            //get ministry members 
            selectedMembers.value = [];
            selectedCellCode.value = item.code; 
            if (item.memberCell.length > 0) {
                selectedMembers.value = item.memberCell.map(member => (
                    {
                        memberCode: member.memberCode,
                        memberDesc: member.memberDesc || '',
                        positionCode: member.position || '',
                        positionDesc: member.positionDesc || '',

                    }));
            }

        };
        const Edit = (item) => {
            $('#form').parsley().reset();
            actionMode.value = 'Modify' 
            disableControl.code = true; 
            formData.id = item.id; 
            formData.code = item.code; 
            formData.description = item.description;
             
        };

        const AddMemberModal = () => {
            //addmember modal
            formDataMember.memberChanged = false;
            disableControl.selectedMember = false;

        } 
        const AddMember = () => {
            //addmember to Temporary
            $('#addMember').parsley().validate();
            if ($('#addMember').parsley().isValid()) {
                var memberCode = formDataMember.member.trim();
                var positioncode = formDataMember.position.trim();

                var member = {};
                var position = {};

                var selectedMember = {
                    "memberCode": '',
                    "memberDesc": '',
                    "positionCode": '',
                    "positionDesc": '',
                };

                for (let x = 0; x < members.value.length; x++) {
                    if (memberCode == members.value[x].code)
                        member = members.value[x];
                }

                for (let x = 0; x < memberPositions.value.length; x++) {
                    if (positioncode == memberPositions.value[x].code)
                        position = memberPositions.value[x];
                }

                if (disableControl.selectedMember) {
                    var index = selectedMembers.value.findIndex(c => c.memberCode === memberCode);

                    if (index !== -1) {
                        selectedMembers.value[index].positionCode = position.code;
                        selectedMembers.value[index].positionDesc = position.description;

                        swal.fire({
                            text: "Member modified!",
                            icon: "success"
                        });
                        formDataMember.memberChanged = true;
                        nextTick(() => {
                            // Trigger Parsley validation manually
                            $('.selectpicker').selectpicker('refresh');
                        });
                        $('#addMemberModal').modal('hide');
                    }

                }
                else {
                    if (!selectedMembers.value.find(c => c.memberCode === memberCode)) {

                        selectedMember.memberCode = member.code;
                        selectedMember.memberDesc = member.firstName + " " + member.middleName + " " + member.lastName;
                        selectedMember.positionCode = position.code;
                        selectedMember.positionDesc = position.description;
                        selectedMembers.value.push(selectedMember);
                        swal.fire({
                            text: "Member added!",
                            icon: "success"
                        });
                        formDataMember.member = "";
                        formDataMember.position = "";
                        formDataMember.memberChanged = true;
                        nextTick(() => {
                            // Trigger Parsley validation manually
                            $('.selectpicker').selectpicker('refresh');
                        });
                    } else {
                        swal.fire({
                            text: "Member is already selected!",
                            icon: "error"
                        });
                    }
                }
            }
            else {
                swal.fire({
                    text: "Fill out the required fields!",
                    icon: "warning"
                });
            }

        } 
        const EditMember = (member) => {
            //disableControl.selectedMember = true;
            formDataMember.member = member.memberCode;
            formDataMember.position = member.positionCode;
            nextTick(() => {
                // Trigger Parsley validation manually
                $('.selectpicker').selectpicker('refresh');
            });
        }
        const RemoveMember = (member) => {
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

                    const index = selectedMembers.value.findIndex(m => m.memberCode === member.memberCode);
                    if (index !== -1) {
                        // If the member is found, remove it
                        selectedMembers.value.splice(index, 1);
                        formDataMember.memberChanged = true;
                    } else {
                        // Optionally, you can add the member if it's not found
                        // this.selectedMembers.push(member);
                        // this.formDataMember.memberChanged = true;
                    }

                     
                    swal.fire({
                        text: "Member removed!",
                        icon: "success"
                    });
                }
            });

        } 
        const HandleSelectChange = (event) => {
            $('#addMember').parsley().validate();
        };


        // Execute function when Vue instance is created  
        GetPosition();
        GetMemberCodeAndName(""); 
        GetCell();
        const returnProps = {
            actionMode,
            isFormValid,
            datatable,
            disableControl,
            search,
            formData, 
            formDataMember,
            items,
            checkBoxes,  
            members,
            memberPositions,
            selectedMembers,
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
            Sort,
            SortClass,
            MemberModal,
            AddMemberModal,
            AddMember,
            HandleSelectChange,
            SaveMembers,
            EditMember,
            RemoveMember,
        };
        return {
            ...returnProps,
            ...returnMethod

        };
    }
});

cellController.mount('#CellController');