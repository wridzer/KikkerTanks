<?php

if (isset($_GET['PHPSESSID'])) { //staat de sessie id in de url?

    $sid=htmlspecialchars($_GET['PHPSESSID']); //sessie id uit url sanitizen

    session_id($sid); //sessie id voor deze sessie instellen naar wat uit url kwam
}

session_start(); 

if (isset($_SESSION["server_id"]) && $_SESSION["server_id"]!=0) {

    createUser();

} else {

  echo "0";
}


function createUser(){
    include "connect.php";
    
    $name = $_GET["name"];
    $email = $_GET["email"];
    $password = $_GET["password"];
    $dateofbirth = $_GET["dateofbirth"];
    $nickname = $_GET["nickname"];

    //Sanitize and filter
    $check = 0;
    
    $name = filter_var($name, FILTER_SANITIZE_STRING);
    $nickname = filter_var($nickname, FILTER_SANITIZE_STRING);
    
    if (!filter_var($email, FILTER_VALIDATE_EMAIL)){
        $check = 1;
        echo "invalid mail adress";
    }
    
    if ($password != trim($password) && str_contains($password, ' ')) { //Checks password for spaces
        $check = 1;
        echo "invalid password";
    }
    
    $date = str_replace("/", "-", $dateofbirth);
    $d = explode("-"  , $date);
    if (!checkdate($d[1], $d[0], $d[2])) {
        $check = 1;
        echo "invalid date";
    }
    //Set date to something sql gets
    $insertDate = mktime(0, 0, 0, $d[0], $d[1], $d[2]);
    
    //Check if email exsists in db already
    $query = "SELECT email FROM users where email = '$email'";
    if (!($result = $conn->query($query))){
        showerror($conn->errno,$conn->error);
    }
    $row = $result->fetch_assoc();
    if ($row["email"] != NULL) {
        echo -1;
    }

    //Hash password
    $hashedPassword = password_hash($password, PASSWORD_DEFAULT);

    
    //Add user to db
    $query = "INSERT INTO `users` (`id`, `name`, `email`, `password`, `dateofbirth`, `momentjoined`, `nickname`) VALUES (NULL, '$name', '$email', '$hashedPassword', $insertDate, current_timestamp(), '$nickname')";
    
    if (!($result = $conn->query($query))){
        showerror($conn->errno,$conn->error);
    }
    
}

?>