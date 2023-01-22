<?php
include "connect.php";

$id = $_GET["id"];
$password = $_GET["password"];

$query = "SELECT * FROM `servers` WHERE id=$id AND password='$password'";

//TODO: sanitize

if (!($result = $conn->query($query))){
    showerror($conn->errno,$conn->error);    
}

$row = $result->fetch_assoc();

if ($id == $row['id']){
    session_start();
    $_SESSION["server_id"] = $row['id'];
    $_SESSION['PHPSESSID'] = session_id();
    echo session_id();
}
else {
    echo 0;
}

?>