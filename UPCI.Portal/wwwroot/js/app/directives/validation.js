$(function () {
    var pathRegex = /^\/[a-zA-Z0-9-_.]+(\/[a-zA-Z0-9-_.]+)*(\/\{[a-zA-Z0-9-_]+\})*\/?$/;
    var ipRegex = /^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$|^::1$/;
    var ipPortRegex = /^((?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)|([a-zA-Z0-9](?:[a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}|::1)(?::\d{1,5})?$/;

    var hostRegex = /^((?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)|([a-zA-Z0-9](?:[a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,})(?::\d{1,5})?$/;

    var multiHostRegex = /^((?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)|(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}|localhost):\d{1,5}$/;


    window.Parsley.addValidator('restrictNumericOnly', {
        validateString: function (value) {
            return /^\d+$/.test(value);
        },
        messages: {
            en: 'This field can only contain digits (0-9).'
        }
    });
    window.Parsley.addValidator('restrictAlphaOnly', {
        validateString: function (value) {
            return /^[A-Za-z- ]*$/.test(value);
        },
        messages: {
            en: 'This field cannot contain special characters.'
        }
    }); 
    window.Parsley.addValidator('restrictAlphanumericOnly', {
        validateString: function (value) {
            return /^$|^[a-zA-Z0-9]+$/.test(value);
        },
        messages: {
            en: 'This field cannot contain special characters.'
        }
    });
    window.Parsley.addValidator('restrictSpecialCharacters', {
        validateString: function (value) {
            return new RegExp(scRegex).test(value);
        },
        messages: {
            en: 'This field cannot contain restricted special characters.'
        }
    });
    window.Parsley.addValidator('restrictPath', {
        validateString: function (value) {
            return pathRegex.test(value);
        },
        messages: {
            en: 'This field must be a valid API path like "/api/auth/authenticate".'
        }
    });
    window.Parsley.addValidator('ip', {
        validateString: function (value) {
            // Regular expression to match IP address or domain followed by a port number (e.g., 10.20.0.24)
            
            return ipRegex.test(value);
        },
        messages: {
            en: 'This field must be in the format of an IP address (e.g. 10.20.0.24).'
        }
    });
    window.Parsley.addValidator('ipPort', {
        validateString: function (value) {
            // Regular expression to match IP address or domain followed by a port number (e.g., 10.20.0.24:9999)
           
            return ipPortRegex.test(value);
        },
        messages: {
            en: 'This field must be in the format of an IP address (e.g. 10.20.0.24:9999).'
        }
    });
    window.Parsley.addValidator('host', {
        validateString: function (value) {
            // Regular expression to match IP address or domain followed by a port number (e.g., 10.20.0.24:9999)
            
            return hostRegex.test(value);
        },
        messages: {
            en: 'This field must be in the format of an IP address or domain followed by a port number (e.g. google.com, 10.20.0.24:9999).'
        }
    });
    window.Parsley.addValidator('multihost', {
        validateString: function (value) {
            // Regular expression to match a single IP address or domain followed by a port number 
            // Split the input value by | and validate each part
            var hosts = value.split('|');
            for (var i = 0; i < hosts.length; i++) {
                if (!multiHostRegex.test(hosts[i].trim())) {
                    return false;
                }
            }
            return true;
        },
        messages: {
            en: 'This field must contain hosts in the format of IP addresses, domains, or "localhost" followed by a port number, separated by "|".'
        }
    });
    window.Parsley.addValidator('atLeastOneChecked', {
        validate: function (value, requirement, parsleyInstance) {
            var $checkboxes = $(parsleyInstance.$element).find('input[name="httpMethods"]:checked');
            return $checkboxes.length > 0;
        },
        messages: {
            en: 'At least one checkbox must be checked.'
        }
    });
    window.Parsley.addValidator('durationFormat', {
        validateString: function (value) {
            // Regular expression to match the format
            var durationRegex = /^[1-9][0-9]*[smhd]$/;
            return durationRegex.test(value);
        },
        messages: {
            en: 'This field must be in the format of an integer followed by "s" (seconds), "m" (minutes), "h" (hours), or "d" (days).'
        }
    });
    window.Parsley.addValidator('militaryTime', {
        validateString: function (value) {
            // Regular expression to match military time format (HH:mm)
            var militaryTimeRegex = /^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$/;
            return militaryTimeRegex.test(value);
        },
        messages: {
            en: 'Please enter a valid military time (e.g., 08:00, 17:01, 23:59).'
        } 
    });
    window.Parsley.addValidator('emailValidator', {
        validateString: function (value) {
            // Regular expression to validate email format
            var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return emailRegex.test(value);
        },
        messages: {
            en: 'Please enter a valid email address.'
        }
    });
    window.Parsley.addValidator('dateFormat', {
        validateString: function (value) {
            // Check if the date matches the format yyyy-MM-dd
            const regex = /^\d{4}-\d{2}-\d{2}$/;

            if (!regex.test(value)) {
                return false;
            }

            // Parse the date
            const [year, month, day] = value.split('-').map(Number);

            // Create date and check if valid
            const date = new Date(year, month - 1, day);
            return date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day;
        },
        messages: {
            en: 'Please enter a valid date in the format YYYY-MM-DD.'
        }
    });
    window.Parsley.addValidator('requiredField', { 
        validateString: function (value) {
            return value.trim() !== '';
        },
        messages: {
            en: 'Required field.'
        } 
    });
});