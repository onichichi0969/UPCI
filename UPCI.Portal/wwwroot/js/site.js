var csrfToken = $('input:hidden[name="__RequestVerificationToken"]').val();
var contentType = 'application/json';
var headers = {
    "XSRF-TOKEN": csrfToken,
    "Content-Type": contentType
};
$(window).on('load', function () {
    $(".pagePreloader").show();
    $(".pagePreloader").fadeOut("slow");
    setInterval(function () { $(".contentBody").css("visibility", "visible"); }, 500);

});

window.FontAwesomeConfig = { autoReplaceSvg: false }
$(function () {


    toastr.options = {
        "closeButton": true,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "showDuration": "300",
        "hideDuration": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };

   setInterval(function () { CheckSession(); }, 5000);

    //$('.dates').daterangepicker({
    //    autoUpdateInput: false,
    //    locale: {
    //        cancelLabel: 'Clear'
    //    }
    //});

    //$('.dates').on('apply.daterangepicker', function (ev, picker) {
    //    $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
    //});

    //$('.dates').on('cancel.daterangepicker', function (ev, picker) {
    //    $(this).val('');
    //});
});

GetUrlVariables = function () {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
};

GetDateToday = function () {
    var today = new Date();
    var dd = String(today.getDate()).padStart(2, '0');
    var mm = String(today.getMonth() + 1).padStart(2, '0');
    var yyyy = today.getFullYear();
    today = mm + '/' + dd + '/' + yyyy;
    return today;
};

GoBack = function () {
    window.history.back();
};

UpdateActivity = function () {
    $.ajax({
        url: appUrl + "/Home?handler=UpdateActivity",
        type: 'get'
    });
};

CheckSession = function () {
    $.ajax({
        url: appUrl + "/Home?handler=SessionExpired",
        type: 'get',
        success: function (data) {
            if (data === 'true') {
                Swal.fire({
                    toast: true,
                    titleText: "You have forcefuly logged out due to session timeout.",
                    type: "success",
                    icon: 'success',
                    position: "top",
                    showConfirmButton: false,
                    timer: 5000
                }).then(() => {
                    window.location.href = appUrl;
                });
            }
        },
        error: function () {
            console.log('Error!');
        }
    });
};

Logout = function () {
    $.ajax({
        url: appUrl + "/Home?handler=Logout",
        type: 'get',
        success: function (data) {
            if (data === 'logout') {
                Swal.fire({
                    toast: true,
                    titleText: "You have logged out successfully and will be redirect to login shortly",
                    type: "success",
                    icon: 'success',
                    position: "top",
                    showConfirmButton: false,
                    timer: 3000
                }).then(() => {
                    window.location.href = appUrl;
                });
            }
        },
        error: function () {
            console.log('Error!');
        }
    });
};

ChangePassword = function () {
    window.location.href = appUrl + '/ChangePassword';
};

ActivityLog = function () {
    window.location.href = appUrl + '/ActivityLog';
};
DestroyCropper = function () {
    $('#image').cropper('destroy');

}
ChangeProfileImage = function () {

    $('#profileModal').modal('show');
    $('#image').attr('src', '');
    setTimeout(function () {
        $('#image').attr('src', appUrl + "/Home?handler=UserProfileImage");
        loadCropper();
    }, 1000); // 3000 milliseconds delay (3 seconds)






    //document.getElementById('aspectRatio2').click();

};


var validFileTypes = [".jpg", ".jpeg", ".gif", ".png"];

ValidateImage = function (input) {

    if (input.type == "file") {
        var sFileName = input.value;

        if (sFileName.length > 0) {
            var blnValid = false;
            for (var j = 0; j < validFileTypes.length; j++) {
                var sCurExtension = validFileTypes[j];

                if (sFileName.substr(sFileName.length - sCurExtension.length, sCurExtension.length).toLowerCase() == sCurExtension.toLowerCase()) {
                    blnValid = true;


                    if (input.files && input.files[0]) {
                        var reader = new FileReader();

                        reader.onload = function (e) {
                            $('#image').attr('src', e.target.result);
                        };

                        reader.readAsDataURL(input.files[0]);
                    }

                    break;
                }
            }

            if (!blnValid) {
                toastr.warning("Sorry, " + sFileName + " is invalid, allowed extensions are: " + validFileTypes.join(", ", 'Warning'));
                input.value = "";
                $('#image').attr('src', appUrl + "/Home?handler=ProfileImage");
                return false;
            }
        }

        return false;
    }

};

UploadProfileImage = function () {
    $(".preloader").show();
    var imageElement = document.getElementById('image');
    var cropper = $(imageElement).data('cropper');
    var canvas = cropper.getCroppedCanvas();
    if (typeof canvas.toBlob !== "undefined") {
        canvas.toBlob(function (blob) {
            // send the blob to server etc.
            var fdata = new FormData();
            fdata.append("images", blob);
            $.ajax({
                type: "POST",
                url: appUrl + '/Home?handler=ChangeProfileImage',
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("XSRF-TOKEN", csrfToken);
                },
                data: fdata,
                dataType: "json",
                cache: false,
                contentType: false,
                processData: false,
                async: true,
                success: function (result) {
                    $(".preloader").fadeOut('fast');
                    if (result.status === 'error') {
                        swal.fire({
                            title: 'Failed',
                            text: "Failed to save image!",
                            icon: "error"
                        });
                    }
                    else {
                        $('#profileModal').modal('hide');
                        swal.fire({
                            title: 'Success',
                            text: "Profile image successfully updated!",
                            icon: "success",
                            showCancelButton: false,
                            confirmButtonText: 'OK'
                        }).then((result) => {
                            location.reload();
                        });


                    }
                }
            });
            $('#image').attr('src', appUrl + "/Home?handler=ProfileImage");
        }, "image/jpeg", 1);
    }
    else if (typeof canvas.msToBlob !== "undefined") {
        var blob = canvas.msToBlob()
        // send blob
    }
    else {
        // manually convert Data-URI to Blob (if no polyfill)
    }

    //old upload
    //if ($('#imageUpload')[0].files.length > 0) {
    //    var fdata = new FormData();
    //    fdata.append("images", $('#imageUpload')[0].files[0]);

    //    $.ajax({
    //        type: "POST",
    //        url: appUrl + '/Home?handler=ChangeProfileImage',
    //        beforeSend: function (xhr) {
    //            xhr.setRequestHeader("XSRF-TOKEN", csrfToken);
    //        },
    //        data: fdata,
    //        dataType: "json",
    //        cache: false,
    //        contentType: false,
    //        processData: false,
    //        async: true,
    //        success: function (result) {
    //            if (result.status === 'error') {
    //                toastr.error("Sorry, Failed to upload image!", 'Error');
    //            }
    //            else {
    //                toastr.success("Profile image successfully updated!", 'Success');
    //                $('#profileModal').modal('hide');
    //                location.reload();
    //            }
    //        }
    //    });

    //}
    //else {
    //    toastr.error("Sorry, Please select image to upload!", 'Error');
    //}

};

//Night Mode function
document.addEventListener('DOMContentLoaded', function () {
    var lastState = localStorage.getItem('toggleState');
    if (lastState === 'sun') {
        document.getElementById('moonIcon').classList.remove('far');
        document.getElementById('moonIcon').classList.add('fas');
        document.getElementById('moonIcon').classList.remove('fa-moon');
        document.getElementById('moonIcon').classList.add('fa-sun');
        document.body.classList.remove('dark-mode');
    }

    // Add click event listener to toggle between moon and sun icons and save state in local storage
    document.getElementById('toggleButton').addEventListener('click', function () {
        var moonIcon = document.getElementById('moonIcon');
        if (moonIcon.classList.contains('far')) {
            moonIcon.classList.remove('far');
            moonIcon.classList.add('fas');
            moonIcon.classList.remove('fa-moon');
            moonIcon.classList.add('fa-sun');
            document.body.classList.remove('dark-mode');
            // Save the state in local storage
            localStorage.setItem('toggleState', 'sun');
        } else {
            moonIcon.classList.remove('fas');
            moonIcon.classList.add('far');
            moonIcon.classList.remove('fa-sun');
            moonIcon.classList.add('fa-moon');
            document.body.classList.add('dark-mode');
            // Save the state in local storage
            localStorage.setItem('toggleState', 'moon');
        }
    });

  
    //const chk = document.getElementById('chk');
    //var nightModeState = localStorage.getItem('nightModeState');
    //if (nightModeState === 'off') {
    //    document.body.classList.remove('dark-mode');
    //    chk.checked = true;
    //}
    //else {
    //    document.body.classList.add('dark-mode');
    //    chk.checked = false;
    //}

    //chk.addEventListener('change', () => {
    //    document.body.classList.toggle('dark-mode');
    //    if (document.body.classList.contains('dark-mode')) {
    //        localStorage.setItem('nightModeState', 'on');
    //    }
    //    else {
    //        localStorage.setItem('nightModeState', 'off');
    //    }
    //});
});

