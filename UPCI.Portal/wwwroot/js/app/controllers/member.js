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
        const formDataGroups = reactive({
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
         
        
        const checkBoxes = reactive({
            mainCheckBox: false,
            childCheckBox: []
        });
        const items = ref([]);  
        const departments = ref([]);
        const cells = ref([]);
        const positionCells = ref([]);
        const ministries = ref([]);
        const positionMinistries = ref([]);
        const selectedMinistries = ref([]); 
        const selectedCells = ref([]);
        const imageData = ref(''); 
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
        const GetDepartment = async () => {
            const result = await DepartmentService.All()

            if (result.data != null) {
                departments.value = result.data;
            }
            else {
                departments.value = [];
            }

        };
        const GetCell = async () => { 
            const result = await CellService.All() 
            if (result.data != null) {
                cells.value = result.data;
            }
            else {
                cells.value = [];
            }

        };
        const GetPositionCell = async () => {
            const result = await PositionCellService.All()
            if (result.data != null) {
                positionCells.value = result.data;
            }
            else {
                positionCells.value = [];
            }

        };
        const GetMinistry = async (departmentCode) => {
            const result = await MinistryService.All(departmentCode)
            if (result.data != null) {
                ministries.value = result.data;
            }
            else {
                ministries.value = [];
            }

        };
        const GetPositionMinistries = async () => {
            const result = await PositionMinistryService.All()
            if (result.data != null) {
                positionMinistries.value = result.data;
            }
            else {
                positionMinistries.value = [];
            }

        };
        const ConstructCells = () => {
            var cellList = [];
            for (let x = 0; x < selectedCells.value.length; x++) {
                cellList.push(
                    {
                        cellCode: selectedCells.value[x].cellCode,
                        positionCellCode: selectedCells.value[x].positionCellCode
                    }
                );
            }
            return cellList;
        }
        const ConstructMinistries = () => {
            var ministryList = [];
            for (let x = 0; x < selectedMinistries.value.length; x++) {
                ministryList.push(
                    {
                        ministryCode: selectedMinistries.value[x].ministryCode,
                        positionMinistryCode: selectedMinistries.value[x].positionMinistryCode
                    }
                );
            }
            return ministryList;
        }
        
        const blobToBase64 = (blob) => {
            return new Promise((resolve, reject) => {
                const reader = new FileReader();
                reader.onloadend = () => resolve(reader.result);
                reader.onerror = reject;
                reader.readAsDataURL(blob); // Convert Blob to base64
            });
        };
        const GetMemberProfileImage = async (id) =>
        {
            var response = await MemberService.GetMemberProfileImage(id);
            const base64Image = await blobToBase64(response.data); // Convert Blob to base64
            imageData.value = base64Image; 

        } 
        
        const SetMemberImage = () => {
            $(".preloader").show();
            var imageElement = document.getElementById('imageMember');
            var cropper = $(imageElement).data('cropper');
            var canvas = cropper.getCroppedCanvas();
            if (typeof canvas.toBlob !== "undefined") {
                canvas.toBlob(function (blob) { 
                    $('.preloader').fadeOut('slow');
                     blobToBase64(blob)
                        .then(base64String => {
                            imageData.value = base64String; 
                            formData.imageChanged = true;
                            $('#memberProfileModal').modal('hide');
                    })
                    .catch(error => {
                        console.error('Error converting Blob to Base64:', error);
                    });
                }, "image/jpeg", 1);
            }
            else if (typeof canvas.msToBlob !== "undefined") {
                var blob = canvas.msToBlob();
                $('.preloader').fadeOut('slow');
                return blob; 
            }
            else {
                $('.preloader').fadeOut('slow');
                return null;
            }
            $('.preloader').fadeOut('slow');
        };
        const getImageTypeFromBase64 = (base64String)=> {
            // Extract the MIME type from the base64 string
            const matches = base64String.match(/^data:(image\/[a-zA-Z]*);base64,/);
            if (matches && matches.length > 1) {
                return matches[1]; // MIME type (e.g., image/jpeg)
            }
            return null; // Return null if no MIME type is found
        }
        const Save = async () => {
            var cellList = ConstructCells();
            var ministryList = ConstructMinistries();
            formData.cells = cellList;
            formData.ministries = ministryList;
            $('#form').parsley().validate();
            if ($('#form').parsley().isValid()) {
                $(".preloader").show();

                if (formData.imageChanged)
                {
                    formData.imageContent = imageData.value;
                    formData.imageType = getImageTypeFromBase64(imageData.value);
                }
                
                const result = await MemberService.Save(formData);
                if (result.data.status === 'SUCCESS') {
                    $('#formModal').modal('hide');
                    formData.imageChanged = false;
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
            formData.imageChanged = false;
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
        const SaveCell = () =>
        {
            $('#cellForm').parsley().validate();
            if ($('#cellForm').parsley().isValid()) {
                var cellcode = formDataGroups.cell.trim();
                var positioncode = formDataGroups.positionCell.trim();
                var cell = {};
                var positionCell = {};
                var selectedCell = {
                    "cellCode": '',
                    "cellDesc": '',
                    "positionCellCode": '',
                    "positionCellDesc": '',
                };

                for (let x = 0; x < cells.value.length; x++) {
                    if (cellcode == cells.value[x].code)
                        cell = cells.value[x];
                }

                for (let x = 0; x < positionCells.value.length; x++) {
                    if (positioncode == positionCells.value[x].code)
                        positionCell = positionCells.value[x];
                }
                if (disableControl.selectedCell) {
                    var index = selectedCells.value.findIndex(c => c.cellCode === cellcode);

                    if (index !== -1) {
                        // Update the cell at the found index
                        selectedCells.value[index].positionCellCode = positionCell.code;
                        selectedCells.value[index].positionCellDesc = positionCell.description;

                        swal.fire({
                            text: "Cell modified!",
                            icon: "success"
                        });
                        $('#cellFormModal').modal('hide');
                        formData.cellChanged = true;
                    }

                }
                else {
                    if (!selectedCells.value.find(c => c.cellCode === cellcode)) {

                        selectedCell.cellCode = cell.code;
                        selectedCell.cellDesc = cell.description;
                        selectedCell.positionCellCode = positionCell.code;
                        selectedCell.positionCellDesc = positionCell.description;

                        selectedCells.value.push(selectedCell);
                        swal.fire({
                            text: "Cell added!",
                            icon: "success"
                        });
                        formDataGroups.cell = "";
                        formDataGroups.positionCell = "";
                        formData.cellChanged = true;
                    } else {
                        swal.fire({
                            text: "Cell is already selected!",
                            icon: "error"
                        });
                    }
                }
            }
            else
            {
                swal.fire({
                    text: "Fill out the required fields!",
                    icon: "warning"
                });
            }
            
        }
        const SaveMinistry = () => {
            $('#ministryForm').parsley().validate();
            if ($('#ministryForm').parsley().isValid()) {
                var ministrycode = formDataGroups.ministry.trim();
                var positioncode = formDataGroups.positionMinistry.trim();
                var departmentCode = formDataGroups.departmentCode.trim();
                var ministry = {};
                var positionMinistry = {};
                var department = {};
                var selectedMinistry = {
                    "ministryCode": '',
                    "ministryDesc": '',
                    "positionMinistryCode": '',
                    "positionMinistryDesc": '',
                    "departmentCode": '',
                    "departmentDesc":''
                };

                for (let x = 0; x < ministries.value.length; x++) {
                    if (ministrycode == ministries.value[x].code)
                        ministry = ministries.value[x];
                }

                for (let x = 0; x < positionMinistries.value.length; x++) {
                    if (positioncode == positionMinistries.value[x].code)
                        positionMinistry = positionMinistries.value[x];
                }
                for (let x = 0; x < departments.value.length; x++) {
                    if (departmentCode == departments.value[x].code)
                        department = departments.value[x];
                }

                if (disableControl.selectedMinistry) {
                    var index = selectedMinistries.value.findIndex(c => c.ministryCode === ministrycode);

                    if (index !== -1) { 
                        selectedMinistries.value[index].positionMinistryCode = positionMinistry.code;
                        selectedMinistries.value[index].positionMinistryDesc = positionMinistry.description; 

                        selectedMinistries.value[index].departmentCode = department.code;
                        selectedMinistries.value[index].departmentDesc = department.description;

                        swal.fire({
                            text: "Ministry modified!",
                            icon: "success"
                        }); 
                        formData.ministryChanged = true;
                        $('#ministryFormModal').modal('hide');
                    }

                }
                else {
                    if (!selectedMinistries.value.find(c => c.ministryCode === ministrycode)) {

                        selectedMinistry.ministryCode = ministry.code;
                        selectedMinistry.ministryDesc = ministry.description;
                        selectedMinistry.positionMinistryCode = positionMinistry.code;
                        selectedMinistry.positionMinistryDesc = positionMinistry.description;
                        selectedMinistry.departmentCode = department.code;
                        selectedMinistry.departmentDesc = department.description;
                        selectedMinistries.value.push(selectedMinistry);
                        swal.fire({
                            text: "Ministry added!",
                            icon: "success"
                        });
                        formDataGroups.ministry = "";
                        formDataGroups.positionMinistry = ""; 
                        formDataGroups.departmentCode = "";
                        formData.ministryChanged = true;
                    } else {
                        swal.fire({
                            text: "Ministry is already selected!",
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
                    GetMember();
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
       
        const Add = () => {
            $('#form').parsley().reset();
            actionMode.value = 'Add'
            disableControl.code = false;
            selectedCells.value = [];
            selectedMinistries.value = [];
            GetMemberProfileImage("");
            formData.cellChanged = false;
            formData.ministryChanged = false;

            formData.imageChanged = false;
            formData.id = '';
            formData.code = '';
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
            formDataGroups.cell = '';
            formDataGroups.positionCell = '';
            formDataGroups.ministry = '';
            formDataGroups.positionMinistry = '';

             
            //formData = {};
        };

        const Edit = (item) => {
            $('#form').parsley().reset();
            GetMemberProfileImage(item.id);
            actionMode.value = 'Modify'
            disableControl.code = true;
            formDataGroups.cell = '';
            formDataGroups.positionCell = '';
            formDataGroups.ministry = '';
            formDataGroups.positionMinistry = '';
            formDataGroups.departmentCode = '';
            formData.imageChanged = false;
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
            formData.cellChanged = false;
            formData.ministryChanged = false;

            LoadMemberCell(item);
            LoadMemberMinistry(item);


        };
        const AddCell = () => {
            disableControl.selectedCell = false;
            formDataGroups.cell = '';
            formDataGroups.positionCell = '';
        };
        const AddMinistry = () => {
            disableControl.selectedMinistry = false;
            disableControl.selectedDepartment = false;
            formDataGroups.ministry = '';
            formDataGroups.positionMinistry = '';
            formDataGroups.departmentCode = '';
        };
        const EditCell = (cell) => {
            disableControl.selectedCell = true;
            formDataGroups.cell = cell.cellCode;
            formDataGroups.positionCell = cell.positionCellCode;

        }
        const EditMinistry = async (ministry) => {
            disableControl.selectedMinistry = true;
            disableControl.selectedDepartment = true;

            formDataGroups.positionMinistry = ministry.positionMinistryCode;
            formDataGroups.departmentCode = ministry.departmentCode;

            await DepartmentChange(ministry.departmentCode);

            if (ministries.value.find(min => min.code === ministry.ministryCode)) {
                formDataGroups.ministry = ministry.ministryCode;
            }
            else {
                formDataGroups.ministry = '';
            }

        }
        const RemoveCell = (cell) => {
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
                    if (selectedCells.value.includes(cell)) {
                        const index = selectedCells.value.indexOf(cell);
                        selectedCells.value.splice(index, 1);
                        formData.cellChanged = true;
                    }
                    swal.fire({
                        text: "Cell removed!",
                        icon: "success"
                    });
                }
            });
           
        }
        const RemoveMinistry = (ministry) => {
            swal.fire({
                title: "Are you sure?",
                text: "Once delete, this will not be accessible on the system!",
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, delete it!'
            }).then((result) => {
                if (result.isConfirmed)
                {
                    if (selectedMinistries.value.includes(ministry)) {
                        const index = selectedMinistries.value.indexOf(ministry);
                        selectedMinistries.value.splice(index, 1);
                        formData.ministryChanged = true;
                    }
                    swal.fire({
                        text: "Ministry removed!",
                        icon: "success"
                    });
                }
            });
           
        } 
        const DepartmentChange = async (ministryCode) => {
            await GetMinistry(ministryCode);
        }
        const LoadMemberCell = (item) =>
        {
            selectedCells.value = [];
            item.memberCell.forEach(cell => {
                selectedCells.value.push({
                    cellCode: cell.cellCode,
                    cellDesc: cell.cellDesc,
                    positionCellCode: cell.position,
                    positionCellDesc: cell.positionDesc
                });
            });
        }
        const LoadMemberMinistry = (item) => {
            selectedMinistries.value = [];
            item.memberMinistry.forEach(ministry => {
                selectedMinistries.value.push({
                    ministryCode: ministry.ministryCode,
                    ministryDesc: ministry.ministryDesc,
                    positionMinistryCode: ministry.position,
                    positionMinistryDesc: ministry.positionDesc,
                    departmentCode : ministry.departmentCode,
                    departmentDesc : ministry.departmentDesc,
                });
            });
        }
        const DestroyCropperMember = () => {
            $('#imageMember').cropper('destroy');

        }
        const ChangeMemberProfileImage = () => {

            $('#memberProfileModal').modal('show');
           // $('#imageMember').attr('src', '');
            setTimeout(function () {
               //$('#imageMember').attr('src', imageData);
                loadCropper('#imageMember', '.img-preview-member', '#inputImageMember');
            }, 1000); // 3000 milliseconds delay (3 seconds)

            //document.getElementById('aspectRatio2').click();

        };
        GetDepartment(); 
        GetCell();
        GetPositionCell(); 
        GetPositionMinistries();
        GetMember();
        const returnProps = {
            actionMode,
            isFormValid,
            datatable,
            disableControl,
            search,
            formData, 
            formDataGroups,
            items,
            cells,
            ministries,
            selectedMinistries,
            selectedCells,
            positionCells,
            positionMinistries,
            departments,
            imageData,
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
            AddCell,
            AddMinistry,
            SaveCell,
            SaveMinistry,
            RemoveCell,
            RemoveMinistry,
            EditCell,
            EditMinistry,
            DepartmentChange,
            DestroyCropperMember,
            ChangeMemberProfileImage,
            SetMemberImage,
        };
        return {
            ...returnProps,
            ...returnMethod

        };
    }
});
membershipController.mount('#MembersController');