<?php
$servername = "localhost";
$username = "wridzerkamphuis";
$password = "oj1eNooQui";
$database_name = "wridzerkamphuis";
// Create connection
$conn = new mysqli($servername, $username, $password, $database_name);

// Check connection
if ($conn->connect_error) {
  die("Connection failed: " . $conn->connect_error);
}

// if(!($result = $conn->query($query)))
//     showerror($conn->errno,$conn->error);

function showerror($error, $errornr) {
    die("Error (".$errornr .")" .$error);
}

?>