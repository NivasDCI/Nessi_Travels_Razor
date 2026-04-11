 
//function ErrorAlert(strmessage) {
//    $.alert({
//        title: Alert_ErrorMessage, content: Alert_ErrorMessage_Content.replace("{0}", strmessage), animation: 'zoom', closeAnimation: 'zoom',
//        buttons: { okay: { text: Alert_OKMessage, btnClass: Alert_Button_Style } }
//    });
//}
//function SucessAlert(strmessage) {
//    var AlertSucess = $.alert({
//        title: Alert_SuccessMessage, content: Alert_SuccessMessage_Content.replace("{0}", strmessage), animation: 'zoom', closeAnimation: 'zoom',
//        buttons: { okay: { text: Alert_OKMessage, btnClass: Alert_Button_Style } }
//    });
//    setTimeout(function () { AlertSucess.close(); }, 1000);
//}

function GetConvertTime(currentTime) {
    
    var hours = parseInt(currentTime.split(":")[0]);
    var minutes =parseInt( currentTime.split(":")[1]);
     
    if (minutes < 10) {
        minutes = "0" + minutes;
    }
    var suffix = "AM";
    if (hours >= 12) {
        suffix = "PM";
        hours = hours - 12;
    }
    if (hours == 0) {
        hours = 12;
    }
    return hours + ":" + minutes + " " + suffix;
}

function isValidEmail(EmailAddress) {
    var userinput = EmailAddress;
    var pattern = /^\b[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b$/i

    if (!pattern.test(userinput)) {
        return false;
    }
    return true;
}

function CommonSetCookie(c_name, value, expiredays) {
    var exdate = new Date(); exdate.setDate(exdate.getDate() + expiredays);
    document.cookie = c_name + "=" + escape(value) +
    ((expiredays == null) ? "" : ";expires=" + exdate.toGMTString());
}

function CommonRemoveCookies(Name, Value) {
    var expires = new Date();
    expires.setUTCFullYear(expires.getUTCFullYear() - 1);
    document.cookie = Name + '=' + Value + '; expires=' + expires.toUTCString() + '; path=/';

}

function setCookie(c_name, value, exdays) {
    var exdate = new Date();
    exdate.setDate(exdate.getDate() + exdays);
    var c_value = escape(value) + ((exdays == null) ? "" : "; expires=" + exdate.toUTCString());
    document.cookie = c_name + "=" + c_value;
}

function PageAccess() {
    $.ajax({
        url: BaseURL + "Common/GetPageAccessControl", type: "GET",
        success: PageAccessSuccessCallBack,
        data: { HeaderViewID: GetParamValue("HeaderViewID"), DetailViewID: GetParamValue("DetailViewID") }
    });
}

function GridPageAccess() { 
    return $.get(BaseURL + "Common/GetPageAccessControl", {
        HeaderViewID: GetParamValue("HeaderViewID"),
        DetailViewID: GetParamValue("DetailViewID")
    });
}


function ValidatePhoneNumber(PhoneNumberLength, Maxdigitphone) {

    var IsValid = false;
    if (PhoneNumberLength == Maxdigitphone) {
        IsValid = true;
    }
    return IsValid;
}


function IsPhoneNumber(strVal) {
    var validStr = '0123456789';
    var temp;
    for (var i = 0; i < strVal.length; i++) {
        temp = strVal.substring(i, i + 1);
        if (validStr.indexOf(temp) == -1) return false;
    }
    if (isNaN(strVal))
        return false;
    return true;
}


function isText(strVal) {
    //var validStr = '0123456789-abcdefghijklmnopqrstuvwxyz `~!%&@#$^&*()-_=+/{}[],.:;"<>?\\ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    var validStr = '0123456789-abcdefghijklmnopqrstuvwxyz _ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    var temp;
    for (var i = 0; i < strVal.length; i++) {
        temp = strVal.substring(i, i + 1);
        if (strVal.charCodeAt(i) != 13 && strVal.charCodeAt(i) != 10) {
            if (validStr.indexOf(temp) == -1) return false;
        }
    }
    return true;
}


function isPasswordText(strVal) {
    //var validStr = '0123456789-abcdefghijklmnopqrstuvwxyz `~!%&@#$^&*()-_=+/{}[],.:;"<>?\\ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    var validStr = '0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ~!%&@#$^&*()-_=+/{}[],.:;<>?';
    var temp;
    for (var i = 0; i < strVal.length; i++) {
        temp = strVal.substring(i, i + 1);
        if (strVal.charCodeAt(i) != 13 && strVal.charCodeAt(i) != 10) {
            if (validStr.indexOf(temp) == -1) return false;
        }
    }
    return true;
}

function isPasswordValidSpecialchar(strVal) {
    var validStr = '~!%&#$^&*()-_=+/{}[],.:;<>?@';
    var strresult = false;
    for (var i = 0; i < strVal.length; i++) {
        if (strVal.charCodeAt(i) != 13 && strVal.charCodeAt(i) != 10) {
            if (validStr.indexOf(strVal.charAt(i)) != -1) {
                strresult = true;
                break;
            }

        }
    }
    return strresult;
}


function AllowNumeric(event) {

    //$(this).val($(this).val().replace(/[^0-9\.]/g, ''));
    //if ((event.which != 46 || $(this).val().indexOf('.') != -1) && (event.which < 48 || event.which > 57)) {
    //    event.preventDefault();
    //}

    var regex = new RegExp("^[0-9]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        event.preventDefault();
        return false;
    }
}

function GetParamValue(ParameterName) {
  
    var PageURL = $("#txtSessionURL").val(); 
    var SplitOne = PageURL.split("?")[1];
    var Parametervalue = "";
    var SplitResult = SplitOne.split("&");
    for (i = 0; i < SplitResult.length; i++) {
        if (SplitResult[i].split('=')[0] == ParameterName) {
            Parametervalue = SplitResult[i].split('=')[1];
            break;
        }
    }
    return Parametervalue;
}


function isNumberKey(evt) {
    
    var charCode = (evt.which) ? evt.which : event.keyCode;
    if (charCode != 46 && charCode > 31
        && (charCode < 48 || charCode > 57))
        return false;

    return true;
}

function isNumberKeyonly(e) {
    
    if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) {
        return false;
    }

    return true;
}


function GetProjectParameterValues(param) {
    var url = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < url.length; i++) {
        var urlparam = url[i].split('=');
        if (urlparam[0] == param) {
            return urlparam[1];
        }
    }
} 
function GetParseJsonDate(jsonDateString) {

    var monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    // var date = new Date(parseInt(jsonDateString.replace('/Date(', ''))); 
    var date = new Date(Number(jsonDateString.replace(/\D/g, '')));
    return (date.getDate() < 10 ? "0" + date.getDate() : date.getDate()) + "-" + monthNames[date.getMonth()] + "-" + date.getFullYear();

}
function ValidateDate(strDate) {
    var dtValue = strDate;
    var dtRegex = new RegExp("^([0]?[1-9]|[1-2]\\d|3[0-1])-(JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC)-[1-2]\\d{3}$", 'i');
    return dtRegex.test(dtValue);
}

