<?php

if (isset($_GET['PHPSESSID'])) { //staat de sessie id in de url?

    $sid=htmlspecialchars($_GET['PHPSESSID']); //sessie id uit url sanitizen

    session_id($sid); //sessie id voor deze sessie instellen naar wat uit url kwam
}

session_start();  

if (isset($_SESSION["server_id"]) && $_SESSION["server_id"]!=0) {

    Insert();

} else {

  echo "0";

}

function Insert(){
    include "connect.php";
    
    $score = $_GET["score"];
    $user = $_SESSION["user_id"];
    $game = $_GET["game"];

    $varArray = array($score, $user, $game);

    $check = 0;

    foreach ($varArray as $var){
       if (!filter_var($var, FILTER_VALIDATE_INT)) {
           $check = 1;
       }
    }
    
    if ($user == 0){
        echo "0";
    }

    if ($check == 0){
        $query = "INSERT INTO `scores` (`id`, `score`, `datetime`, `user_id`, `game_id`) VALUES  (NULL, '$score', current_timestamp(), '$user', '$game')";

        if (!($result = $conn->query($query)))
            showerror($conn->errno,$conn->error);
        else
            echo "1";
    } else {
        echo "0";
    }
}

?>