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

    $gameid = $_GET["game_id"];

    if (!filter_var($gameid, FILTER_VALIDATE_INT)){
        echo "0";
    } else {
        $query = "SELECT
    AVG(score),
    u.name
FROM
    scores s
LEFT JOIN users u ON
    (s.user_id = u.id)
WHERE
    s.datetime BETWEEN DATE_SUB(NOW(), INTERVAL 1 MONTH) AND NOW() AND s.game_id = $gameid
GROUP BY
    s.user_id
ORDER BY
    AVG(score)
DESC";

        if (!($result = $conn->query($query))){
            showerror($conn->errno,$conn->error);
        }
        else {
            $my_json = "{\"times\":[";
            $row = $result->fetch_assoc();

            do{
                $my_json .= json_encode($row);
                $my_json .= ",";
            }while($row = $result->fetch_assoc());
            
            substr_replace($my_json ,"",-1);
            $my_json .= "]}";

            echo $my_json;
        }        
    }
}

?>