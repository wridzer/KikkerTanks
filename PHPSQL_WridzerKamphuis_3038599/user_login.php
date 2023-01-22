<?php

if (isset($_GET['PHPSESSID'])) { //staat de sessie id in de url?

    $sid=htmlspecialchars($_GET['PHPSESSID']); //sessie id uit url sanitizen

    session_id($sid); //sessie id voor deze sessie instellen naar wat uit url kwam
}

session_start();  

if (isset($_SESSION["server_id"]) && $_SESSION["server_id"]!=0) {
    
    Login();

} else {

  echo "0";

}


function Login(){
    include "connect.php";
    
    $email = $_GET["email"];
    $password = $_GET["password"];

    $query = "SELECT id FROM `users` WHERE email='$email' AND password='$password'";

    if (!($result = $conn->query($query))){
        showerror($conn->errno,$conn->error);    
    } else {
        $user_id = $result->fetch_assoc();
        $_SESSION["user_id"] = $user_id["id"];
        echo  $user_id["id"];
    }
}


?>