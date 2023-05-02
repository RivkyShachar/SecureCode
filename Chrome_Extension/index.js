{
    // http://www.websocket.org/echo.html
    const wsUri = "ws://127.0.0.1/";
    var websocket = "";
    var sub_case = 0;
    var cur_tab;
    var currentUrl = "";
    var op = 0;
    var response = new Uint8Array(100);

    function doSend(message) {
       // writeToScreen("SENT: " + message);
        websocket.send(message);
    }


    function bin2String(array) {
        var result = "";
        for (var i = 0; i < array.length; i++) {
            result += String.fromCharCode(parseInt(array[i], 2));
        }
        return result;
    }

    //function that sets the display of the chrome extension pop up
    function display_pop_up(tab) {
        cur_tab = tab;
        const url = JSON.stringify(cur_tab.url);

        if (url.toLowerCase().includes("login") || url.toLowerCase().includes("sign-in") || url.toLowerCase().includes("register") || url.toLowerCase().includes("log-in") || url.toLowerCase().includes("signin") || url.toLowerCase().includes("signup") || url.toLowerCase().includes("join")) {//if in a login page
            document.getElementById('main_Menu').style.display = 'block';
            currentUrl = url;
            websocket = new WebSocket(wsUri);
            websocket.onopen = (e) => {
                // writeToScreen("CONNECTED");
                //doSend("WebSocket rocks");
            };

            websocket.onclose = (e) => {
                //writeToScreen("DISCONNECTED");
            };

            websocket.onmessage = (e) => {
                //writeToScreen(e.data);
                var arr_res = e.data.split(",");
                switch (arr_res.slice(0, 1).at(0)) {
                    case "0"://CheckMainPassword(string password)
                        signInhelper(arr_res.slice(1, 2));
                        break;
                    case "1"://retrieve_password_user(string url)
                        if (sub_case == 1) handleAutoFillHelper(arr_res.slice(1, arr_res.length + 1));
                        else if (sub_case == 2) handleSave1ClickHelper(arr_res.slice(1, arr_res.length + 1));
                        else if (sub_case == 3) handleSave2ClickHelper(arr_res.slice(1, arr_res.length + 1));
                        break;
                    case "2"://save_password_user(Url,pwd,usr);
                        saveHelper(arr_res.slice(1, 2));
                        break;
                    case "3"://autoGenPassword();
                        displayGenHelper(arr_res.slice(1, 2));
                        break;
                    case "4": //new_user(pwd)
                        signUpHelper(arr_res.slice(1, 2));
                    default:
                    // code block
                }
            };

            websocket.onerror = (e) => {
                // writeToScreen("ERROR" + e.data);
            };
        }
        else// if the url doesn't contain "login"
            document.getElementById('non_login_message').style.display = 'block';

    }

    //function that sets the display of the Main Page
    function display_main_menu() {
        document.getElementById('main_Menu').style.display = 'block';
        document.getElementById('SignUp_Menu').style.display = 'none';
    }

    //function that sets the display of the sign in page
    function display_SignIn() {
        document.getElementById('signIn_Menu').style.display = 'block';
        document.getElementById('SignUp_Menu').style.display = 'none';
        document.getElementById('main_Menu').style.display = 'none';
        document.getElementById('menu').style.display = 'none';
        document.getElementById('save').style.display = 'none';
        document.getElementById('generate').style.display = 'none';
        document.getElementById('confirm_update').style.display = 'none';
    }

    //function that sets the display of the sign in page
    function display_SignUp() {
        document.getElementById('SignUp_Menu').style.display = 'block';
        document.getElementById('signIn_Menu').style.display = 'none';
        document.getElementById('main_Menu').style.display = 'none';
        document.getElementById('menu').style.display = 'none';
        document.getElementById('save').style.display = 'none';
        document.getElementById('generate').style.display = 'none';
        document.getElementById('confirm_update').style.display = 'none';
    }

    //function that displays the password recieved from the server
    function displayGenHelper(res) {
        if (res[0] == "0")
            alert("Error in auto generating a new password");
        else {
            op = 1;
            clearTextBoxes();
            document.getElementById('signIn_Menu').style.display = 'none';
            document.getElementById('menu').style.display = 'none';
            document.getElementById('save').style.display = 'none';
            document.getElementById('confirm_update').style.display = 'none';
            document.getElementById('generate').style.display = 'block';
            document.querySelector("#txtb_pwd1").value = res[0];
        }
    }

    //function that sets the display of the chrome extension pop up for saving a user name and password
    function display_Save() {
        op = 2;
        document.getElementById('signIn_Menu').style.display = 'none';
        document.getElementById('menu').style.display = 'none';
        document.getElementById('save').style.display = 'block';
        document.getElementById('generate').style.display = 'none';
        document.getElementById('confirm_update').style.display = 'none';
    }

    //functon that sets the display of  the chrome extension pop up for saving or autofilling a password
    function display_menu() {
        document.getElementById('signIn_Menu').style.display = 'none';
        document.getElementById('menu').style.display = 'block';
        document.getElementById('save').style.display = 'none';
        document.getElementById('generate').style.display = 'none';
        document.getElementById('confirm_update').style.display = 'none';
    }

    //functon that sets the display of the chrome extension pop up for confirming updating a password
    function display_confirm_update() {
        document.getElementById('signIn_Menu').style.display = 'none';
        document.getElementById('menu').style.display = 'none';
        document.getElementById('save').style.display = 'none';
        document.getElementById('generate').style.display = 'none';
        document.getElementById('confirm_update').style.display = 'block';
    }

    function clearTextBoxes() {
        document.querySelector("#txtb_usr1").value = "";
        document.querySelector("#txtb_pwd1").value = "";
        document.querySelector("#txtb_usr2").value = "";
        document.querySelector("#txtb_pwd2").value = "";
    }
   
    //function that will run apon loading the pop up
    function pageLoad() {

        chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) { //querys all open tabs and selects the current tab
            currentUrl = tabs[0].url;
            display_pop_up(tabs[0]);

        });

        document.getElementById("btn_signUp").addEventListener("click", handleSignUpClick);
        document.getElementById("btn_signIn").addEventListener("click", handleGoToSignInClick);
        document.getElementById("btn_sign_in").addEventListener("click", handleSignInClick);
        document.getElementById("btn_generate").addEventListener("click", handleGenerateClick);
        document.getElementById("btn_fill").addEventListener("click", handleAutoFillClick);
        document.getElementById("btn_save0").addEventListener("click", handleSave0Click);
        document.getElementById("btn_save1").addEventListener("click", handleSave1Click);
        document.getElementById("btn_save2").addEventListener("click", handleSave2Click);
        document.getElementById("btn_ok").addEventListener("click", handleOkClick);
        document.getElementById("btn_cancel").addEventListener("click", handleCancelClick);
        document.getElementById("btn_cancel0").addEventListener("click", handleCancelSignUpClick);
        document.getElementById("btn_save").addEventListener("click", handleSaveSignUpClick);
    }

    window.addEventListener("load", () => {
        if (window.location.pathname=="/popup.html") //will only run apon loading the chrome extension
            pageLoad();
    })

    //function that redirects to the sign up page for a new user
    function handleSignUpClick() {
        display_SignUp();
    }

    //function that redirects to the sign in page
    function handleGoToSignInClick() {
        display_SignIn();
    }

    //function handles the button click event for canceling a new user sign up
    function handleCancelSignUpClick() {
        display_main_menu();

    }

    //function that handels the button click event fro signing up to the chrome extension
    function handleSaveSignUpClick() {

        try {
            var txtBox = document.querySelector("#txtb_pwd");
            if (txtBox.value.length != 8) throw "the password must be 8 characters long";
            websocket.send("4," + document.querySelector("#txtb_pwd").value); //send password to save to the applet
        }
        catch (ex) {
            alert(ex);
        }
    }
    function signUpHelper(ans) {

        try {
        if (ans[0] == "1") // If the new password was saved succesfully
        {
            alert("the password was saved succesfully!");
            display_main_menu();
        }
        else if (ans[0] == "0") // If the new password was not saved succesfully
                throw "Error returned from the applet"
        else {//ans[0]=="2: If the is already a main password
            throw "You already have a password for this applet"
        }
    }
        catch (ex) {
        alert(ex);
    }
    }

    //function that handles the button click event for signing with the main password
    function handleSignInClick() {

        try {
            var txtBox = document.querySelector("#txtb_password");
            if (txtBox.value.length != 8) throw "the password must be 8 characters long";
            websocket.send("0," + document.querySelector("#txtb_password").value); //send password to check if its correct
        }
        catch (ex) {
            alert(ex);
        }

    }
    function signInhelper(ans) {
        try {
                if (ans[0]=="1")//if a password was entered and it was correct
                {
                    display_menu();
                }
                else if (ans[0]=="2") {
                    throw "Error returned from the applet";
                }
                else throw "incorrect password entered";

        }
        catch (ex) {
            alert(ex);
        }
    }

    //handle button save click from main menu
    function handleSave0Click() {
        display_Save();
    }

    //handle button save click after generating a password
    function handleSave1Click() {
        
        var usr = document.querySelector("#txtb_usr1").value;
        var pwd = document.querySelector("#txtb_pwd1").value;
        try {
            if (pwd.length != 8) throw "password must be 8 characters long";
            if (usr.length > 20) throw "user name exceeds maximum length of 20 characters"
            if (usr == "" || pwd == "") throw "please enter both the user name and password";
            sub_case = 2;
            websocket.send("1," + currentUrl);// call the retrieve_password_user from applet
        }
        catch (ex) {
            alert(ex);
        }


    }
    function handleSave1ClickHelper(res) {

        var usr = document.querySelector("#txtb_usr1").value;
        var pwd = document.querySelector("#txtb_pwd1").value;
         
        if (res[0] == "0") { //save the new password and user name to the applet
            websocket.send("2," + currentUrl + "," + pwd + "," + usr);//save_password_user(currentUrl,pwd,usr);
            display_menu();
        }
        else if (res[0] == "1") {
            alert("Error from the applet");
            display_menu();
        }
        else { // A password already exhists
            display_confirm_update();
        }
    }

    //handle button save click for saving a user name and password
    function handleSave2Click() {


        var usr = document.querySelector("#txtb_usr2").value;
        var pwd = document.querySelector("#txtb_pwd2").value;
        try {
            if (pwd.length != 8) throw "password must be 8 characters long";
            if (usr.length > 20) throw "user name exceeds maximum length of 20 characters"
            if (usr == "" || pwd == "") throw "please enter both the user name and password";
            sub_case = 3;
            websocket.send("1," + currentUrl);// call the retrieve_password_user from applet
        }
        catch (ex) {
            alert(ex);
        }
        
}
    function handleSave2ClickHelper(res) {

    var usr = document.querySelector("#txtb_usr2").value;
    var pwd = document.querySelector("#txtb_pwd2").value;

        if (res[0] == "0") { //save the new password and user name to the applet
            websocket.send("2," + currentUrl + "," + pwd + "," + usr);//save_password_user(currentUrl,pwd,usr);
            display_menu();
        }
        else if (res[0] == "1") {
            alert("Error from the applet");
            display_menu();
        }
        else { // A password already exhists
            display_confirm_update();
        }
}

    // handle the button autofill click
    function handleAutoFillClick() {
        sub_case = 1;
        websocket.send("1," + currentUrl);// call the retrieve_password_user from applet
    }
    function handleAutoFillHelper(res) {

        if (res[0] == "0")//if no password is saved for this website
        {
            alert("No password found to autofil,Generating a new password")
            handleGenerateClick();
        }
        else if (res[0] == "1") //Error
        {
            alert("Error returned from the applet")
            display_menu();
        }
        else  { //if a password was returned
            autofill(res[0], res[1]);
        }
       
    }
    //main scritp to autofill the login webpage page
    function autofill(pwd, usr) {
        chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => { //querys all open tabs and selects the current tab
            chrome.scripting.executeScript(
                {
                    target: { tabId: tabs[0].id },
                    func: fillPasswordUser,
                    args: [pwd, usr]
                });
        });
    }
    //will autofill the user name and password to the webpage
    function fillPasswordUser(pwd, usr) {

        var inputs = document.getElementsByTagName("input");//look for all "input" elements
        for (var i = 0; i < inputs.length; i++) {
            {
                var input = inputs[i];//look at the element 
                if (input.type == "password") {
                    input.value = pwd;
                }
                if ((input.type == "text" || input.type == "email") && (input.name.toLowerCase()=="name"  || input.name.toLowerCase().includes("username") || input.name.toLowerCase().includes("login") || input.name.toLowerCase().includes("user") || input.name.toLowerCase().includes("email"))) {
                    input.value = usr;
                }
            }
        };
    }

    //handle the button generate click
    function handleGenerateClick() {
        websocket.send("3,"); //autoGenPassword();
    }

    //handel the button cancel click
    function handleCancelClick() {
        clearTextBoxes();
        display_menu();
    }


    //handel the button ok click
    function handleOkClick() {
        var usr;
        var pwd;

        if (op == 1) {
            usr = document.querySelector("#txtb_usr1").value;
            pwd = document.querySelector("#txtb_pwd1").value;
        }
        else if (op == 2) {
            usr = document.querySelector("#txtb_usr2").value;
            pwd = document.querySelector("#txtb_pwd2").value;
        }

        try {
            if (pwd.length != 8) throw "password must be 8 characters long";
            if (usr.length>20) throw "user name exceeds maximum length of 20 characters"
            if(usr==""||pwd=="") throw "please enter both the user name and password";
            websocket.send("2," + currentUrl + "," + pwd + "," + usr);//save_password_user(currentUrl,pwd,usr);
            clearTextBoxes();
            display_menu();
        }
        catch (ex) {
            alert(ex);
           }
   
    }
    function saveHelper(res) {
        if (res[0] == "0")
            alert("Error from applet");
        else
            alert("password saved succesfully");
    }
}


