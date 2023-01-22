<?php

if (isset($_GET['PHPSESSID'])) { //staat de sessie id in de url?

    $sid=htmlspecialchars($_GET['PHPSESSID']); //sessie id uit url sanitizen

    session_id($sid); //sessie id voor deze sessie instellen naar wat uit url kwam
}

session_start();  

if (isset($_SESSION["server_id"]) && $_SESSION["server_id"]!=0) {

    Fetch();

} else {

  echo "0";
}


function Fetch(){
    include "connect.php";

    $playerid = $_SESSION["user_id"];

    if (!filter_var($playerid, FILTER_VALIDATE_INT)){
        echo "0";
    } else {
        $query = "SELECT `nickname` FROM users where id = $playerid";

        if (!($result = $conn->query($query))){
            showerror($conn->errno,$conn->error);
        }
        else {
            $result = $result->fetch_assoc();
            echo $result["nickname"];
        }        
    }    
}

?>