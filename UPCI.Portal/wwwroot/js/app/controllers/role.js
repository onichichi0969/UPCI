const { createApp, reactive, ref, computed, onMounted, filter } = Vue;
const roleController = createApp({
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

        var ctrAccess = 0;
        var ctrModule = 0;
        var modules = []; 
        var moduleActions = [];
        var roleModules = [];

        const Search = () => {
            $(".preloader").show(); 
            datatable.filter = [];
            if ($('#searchDescription').val().trim() !== "")
                datatable.filter.push({ "Property": "Description", "Value": search.description, "Operator": "Contains" });
            GetRole();
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
        const GetRole = async () => {

            var filterDeleted = { "Property": "Deleted", "Value": true, "Operator": "NOTEQUALS" };
            addFilterIfNotExists(datatable.filter, filterDeleted);
            const result = await RoleService.Search(datatable.filter, datatable.sortColumn, datatable.descending, datatable.pageNum, datatable.pageSize)

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
                GetAllModuleActions();
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
        const GetModules = async () =>
        {
            const result = await ModuleService.AllParent();
            clearTableRows();
            if (result.data != null) {
                modules = result.data;
                ctrModule = modules.length;
                modules.map((value, key) => {
                    $("<tr></tr>").addClass("treegrid-" + value.id + (value.parentId != 0 ? " treegrid-parent-" + value.parentId : ""))
                        .appendTo($('.tree')).html("<td>" + value.name + (value.show == false ? ' <i class="fas fa-eye-slash"></i>' : '') + "</td><td class='text-center'><input type='checkbox' onclick='ActionEnable(this," + value.id + "," + value.parentId + ");' class='chkData' name='chk_"
                        + value.parentId + "' value='" + value.code + "' id='chk_" + value.id + "'/></td><td id='tdAction" + value.id + "'>" + GetAction(value.id, value.action) + "</td>");

                }); 
            }
            else {
                modules.value = [];
            }

        }
        const GetRoleModule = async (roleCode) => {
            const result = await RoleService.ByCode(roleCode);
            var x = '';
            if (result.data != null) {
                if (result.data.length > 0) {
                    roleModules = result.data;

                    ctrAccess = roleModules.length;

                    if (ctrModule == ctrAccess)
                        $('#checkAll').prop('checked', true);
                    else
                        $('#checkAll').prop('checked', false);

                    roleModules.map((value, key) => {
                        $("input:checkbox[id='chk_" + value.id + "']").prop('checked', true);
                        $("#chk_" + value.id + "").prop('checked', true);
                        $(".chkAction_" + value.id).prop('disabled', false);

                        $('.chkAction_' + value.id).each(function (index, obj) {
                            if (value.action.includes(this.value)) {
                                $(this).prop('checked', true);
                            }
                        });
                    });
                }
                else
                {
                    roleModules = [];
                    ctrAccess = 0;

                    $('.chkData').prop('checked', false);
                    $(".chkAction").prop('disabled', true);
                    $(".chkAction").prop('checked', false);
                    $('#checkAll').prop('checked', false);
                    ctrAccess = 0;
                }
                 
            }
            else {
                userGroupModules = [];
            }


        };
        const GetAllModuleActions = async () => {
            const result = await ModuleService.AllModuleAction();
            if (result.data !== null) {
                moduleActions = result.data;

                GetModules();
            }

        };
        const GetAction = (encryptedId, action) => {
            var html = "";

            if (action !== '') {
                action.split('|').map((value, key) => {
                    html += "<input type='checkbox' class='chkAction_" + encryptedId + " chkAction' value='" + value + "' disabled/>"
                        + " <label style='margin-right:20px;'  class='font-weight-light'> " + GetActionName(value) + '</label>';
                }); 
            }

            return html;

        }
        const GetActionName = (code) => {

            var name = '';

            for (var i = 0; i < moduleActions.length; i++) {
                if (code === moduleActions[i].code) {
                    return moduleActions[i].description;
                }
            };

            return name;
        };
        ActionEnable = (cb, id, parentId) => {

            if (cb.checked) {
                //$("input:checkbox[name='chk" + parentId + "']").prop('checked', true);
                $(".chkAction_" + id).prop('disabled', false);
                $(".chkAction_" + id).prop('checked', true);
                ctrAccess++;

                //check all parent
                while (parentId != 0) {
                    //check parent
                    $("#chk_" + parentId).prop('checked', true)
                    $(".chkAction_" + parentId).prop('disabled', false);
                    $(".chkAction_" + parentId).prop('checked', true);
                    //get next parent
                    var splittedParentID = $("#chk_" + parentId).attr('name').split('_');
                    if (splittedParentID == null)
                        parentId = 0;
                    if (splittedParentID.length > 0)
                        parentId = splittedParentID[1];

                }

                CheckChild("chk_" + id); 
            }
            else {
                $(".chkAction_" + id).prop('disabled', true);
                $(".chkAction_" + id).prop('checked', false);

                UncheckChild("chk_" + id);
                 
                ctrAccess--;
            }


            if (ctrModule == ctrAccess)
                $('#checkAll').prop('checked', true);
            else
                $('#checkAll').prop('checked', false);
        }
        const CheckChild = (parentCheckBoxName) => {


            var childElements = $("input:checkbox[name='chk_" + parentCheckBoxName.split('_')[1] + "']");
            childElements = GetUniqueIdValuesFromArray(childElements);
            $("#" + parentCheckBoxName).prop('checked', true);
            $(".chkAction_" + parentCheckBoxName.split('_')[1]).prop('checked', true);
            $(".chkAction_" + parentCheckBoxName.split('_')[1]).prop('disabled', false);
            //loop thru first level child
            for (let x = 0; x < childElements.length; x++) {
                $("#" + childElements[x]).prop('checked', true);
                var childID = childElements[x].split('_')[1];
                $(".chkAction_" + childID).prop('checked', true);
                $(".chkAction_" + childID).prop('disabled', false);
                var nextChildElements = $("input:checkbox[name='chk_" + childID + "']");
                nextChildElements = GetUniqueIdValuesFromArray(nextChildElements);
                if (nextChildElements != undefined || nextChildElements != null) {
                    if (nextChildElements.length > 0) {
                        for (let z = 0; z < nextChildElements.length; z++) {
                            CheckChild(nextChildElements[z]);
                        }
                    }
                }
                var y = "";
            }
        }
        const UncheckChild = (parentCheckBoxName) => {

            //uncheck child
            if (parentCheckBoxName == 'chk_6') {
                var zzz = "";
            }
            var childElements = $("input:checkbox[name='chk_" + parentCheckBoxName.split('_')[1] + "']");
            childElements = GetUniqueIdValuesFromArray(childElements);
            $("#" + parentCheckBoxName).prop('checked', false);
            $(".chkAction_" + parentCheckBoxName.split('_')[1]).prop('checked', false);
            $(".chkAction_" + parentCheckBoxName.split('_')[1]).prop('disabled', true);
            //loop thru first level child
            for (let x = 0; x < childElements.length; x++) {
                $("#" + childElements[x]).prop('checked', false);
                var childID = childElements[x].split('_')[1];
                $(".chkAction_" + childID).prop('checked', false);
                $(".chkAction_" + childID).prop('disabled', true);
                var nextChildElements = $("input:checkbox[name='chk_" + childID + "']");
                nextChildElements = GetUniqueIdValuesFromArray(nextChildElements);
                if (nextChildElements != undefined || nextChildElements != null) {
                    if (nextChildElements.length > 0) {
                        for (let z = 0; z < nextChildElements.length; z++) {
                            UncheckChild(nextChildElements[z]);
                        }
                    }
                }
                var y = "";
                //$("input:checkbox[name='" + childName + "']").prop('checked', false);
                //$(".chkAction_" + splittedChildId).prop('disabled', true);
                //$(".chkAction_" + splittedChildId).prop('checked', false);
            }
        }
        const GetUniqueIdValuesFromArray = (array) => {
            var stringArray = [];
            var cleanArray = [];
            for (let x = 0; x < array.length; x++) {
                stringArray.push(array[x].id);
            }
            cleanArray = [...new Set(stringArray)];

            return cleanArray;
        }
        const CheckAll = () => {
            var checkboxAll = $('#checkAll');

            if (checkboxAll[0].checked) {
                $('.chkData').prop('checked', true);
                $(".chkAction").prop('disabled', false);
                $(".chkAction").prop('checked', true);
                ctrAccess = ctrModule;
            }
            else {
                $('.chkData').prop('checked', false);
                $(".chkAction").prop('disabled', true);
                $(".chkAction").prop('checked', false);
                ctrAccess = 0;
            }


        }
        const clearTableRows = () => {
            const table = document.getElementById('modulesTable');
            const rows = table.getElementsByTagName('tr');

            // Skip the first row (header row)
            while (rows.length > 1) {
                table.deleteRow(1);
            }
        }
        const Save = async () => {
            $('#form').parsley().validate();
            if ($('#form').parsley().isValid()) { 
                var moduleCode = '';
                var moduleActions = '';
                roleModules = [];

                modules.map((value, key) => {
                    var checkbox = $("#chk_" + value.id + "");
                    if (checkbox.is(':checked')) {

                        moduleCode = $("#chk_" + value.id + "").val();

                        moduleActions = '';

                        $('.chkAction_' + value.id).each(function (index, obj) {
                            if ($(this).prop('checked')) {
                                moduleActions += (moduleActions === '' ? '' : '|') + $(this).val();
                            };

                        });

                        roleModules.push({
                            "Code": moduleCode
                            , "Action": moduleActions
                        });

                    }
                });
                if (roleModules.length == 0)
                {
                    swal.fire({
                        icon: 'error',
                        title: 'Failed',
                        text: 'You must select atleast 1 module'
                    });
                    return;
                }
                formData.modules = roleModules;
                $(".preloader").show(); 
                const result = await RoleService.Save(formData);
                if (result.data.status === 'SUCCESS') {
                    $('#formModal').modal('hide');
                    swal.fire({
                        text: "Role successfully saved!",
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
                    RoleService.Delete(item)
                        .then((result) => {
                            if (result.data.status === 'SUCCESS') {
                                swal.fire({
                                    text: "Role successfully deleted!",
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
                    GetRole();
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
            formData.encryptedId = ''; 
            formData.code = '';
            formData.name = '';
            formData.description = '';
            //formData = {};
        };

        const Edit = (item) => {
            $('#form').parsley().reset();
            actionMode.value = 'Modify' 
            GetRoleModule(item.code);
            disableControl.code = true; 
            formData.encryptedId = item.encryptedId; 
            formData.code = item.code; 
            formData.name = item.name; 
            formData.description = item.description;
             
        };



        // Execute function when Vue instance is created  
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
            CheckAll,
        };
        return {
            ...returnProps,
            ...returnMethod

        };
    }
});

roleController.mount('#RoleController');