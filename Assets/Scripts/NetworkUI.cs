using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    private static string userId, nickname, sessionId, email, password;

    [SerializeField] private Button
        hostBtn,
        clientBtn,
        loginMenuBtn,
        signupMenuBtn,
        startBtn,
        loginBtn,
        signupBtn;
    [SerializeField] private TMP_InputField
        emailLogin,
        passwordLogin,
        nameSignup,
        nicknameSignup,
        emailSignup,
        passwordSignup,
        passwordAgainSignup,
        dateOfBirthSignup;
    [SerializeField] private GameObject
        completeMenu,
        mainMenu,
        loginMenu,
        signupMenu,
        leaderboardMenu,
        startGameMenu,
        scoreMenu;
    [SerializeField] private TMP_Text 
        mainMenuNickname,
        scoreText,
        leaderboardText;

    private void Awake()
    {
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
        clientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
    }

    public void Back()
    {
        mainMenu.SetActive(true);
        loginMenu.SetActive(false);
        signupMenu.SetActive(false);
        leaderboardMenu.SetActive(false);
        startGameMenu.SetActive(false);
        scoreMenu.SetActive(false);
    }

    public void LoginMenu()
    {
        loginMenu.SetActive(true);
        mainMenu.SetActive(false);
        signupMenu.SetActive(false);
        leaderboardMenu.SetActive(false);
        startGameMenu.SetActive(false);
    }
    
    public void SignupMenu()
    {
        signupMenu.SetActive(true);
        loginMenu.SetActive(false);
        mainMenu.SetActive(false);
        leaderboardMenu.SetActive(false);
        startGameMenu.SetActive(false);
    }
    
    public async void LeaderboardMenu()
    {
        if (sessionId == null) sessionId = await RequestManager.ServerLogin();
        leaderboardMenu.SetActive(true);
        loginMenu.SetActive(false);
        mainMenu.SetActive(false);
        signupMenu.SetActive(false);
        startGameMenu.SetActive(false);
        leaderboardText.text = await RequestManager.GetRequest($"https://studenthome.hku.nl/~wridzer.kamphuis/kernmodule_networking/get_statistics.php?PHPSESSID={sessionId}&game_id=6");
    }
    
    public void StartMenu()
    {
        startGameMenu.SetActive(true);
        loginMenu.SetActive(false);
        mainMenu.SetActive(false);
        signupMenu.SetActive(false);
        leaderboardMenu.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public async void Login()
    {
        if (sessionId == null) sessionId = await RequestManager.ServerLogin();
        string url = $"https://studenthome.hku.nl/~wridzer.kamphuis/kernmodule_networking/user_login.php?PHPSESSID={sessionId}&email={emailLogin.text}&password={passwordLogin.text}";
        string response = await RequestManager.GetRequest(url);
        if (response == "0") { Login(); }
        userId = response;
        loginMenuBtn.interactable = false;
        signupMenuBtn.interactable = false;
        startBtn.interactable = true;
        nickname = await RequestManager.GetRequest($"https://studenthome.hku.nl/~wridzer.kamphuis/kernmodule_networking/fetch_userinfo.php?PHPSESSID={sessionId}");
        mainMenuNickname.text = "You are currently logged in as: " + nickname;
        email = emailLogin.text;
        password = passwordLogin.text;
        Back();
    }

    public async void Login(string _url)
    {
        if (sessionId == null) sessionId = await RequestManager.ServerLogin();
        string response = await RequestManager.GetRequest(_url);
        userId = response;
        loginMenuBtn.interactable = false;
        signupMenuBtn.interactable = false;
        startBtn.interactable = true;
        nickname = await RequestManager.GetRequest($"https://studenthome.hku.nl/~wridzer.kamphuis/kernmodule_networking/fetch_userinfo.php?PHPSESSID={sessionId}");
        mainMenuNickname.text = "You are currently logged in as: " + nickname;
        Back();
    }

    public async void Signup()
    {
        if (passwordSignup.text != passwordAgainSignup.text) return; // TODO: error
        if (sessionId == null) sessionId= await RequestManager.ServerLogin();
        string url = $"https://studenthome.hku.nl/~wridzer.kamphuis/kernmodule_networking/insert_user.php?PHPSESSID={sessionId}&name={nameSignup.text}&email={emailSignup.text}&password={passwordSignup.text}&dateofbirth={dateOfBirthSignup.text}&nickname={nicknameSignup.text}";
        string response = await RequestManager.GetRequest(url);
        if (response == "0") { Signup(); }
        url = $"https://studenthome.hku.nl/~wridzer.kamphuis/kernmodule_networking/user_login.php?PHPSESSID={sessionId}&email={emailSignup.text}&password={passwordSignup.text}";
        email = emailSignup.text;
        password = passwordSignup.text;
        Login(url);
    }

    public void StartGame()
    {
        completeMenu.SetActive(false);
    }

    public async void EndGame(float _score)
    {
        completeMenu.SetActive(true);
        startGameMenu.SetActive(false);
        scoreMenu.SetActive(true);
        scoreText.text = "Your score was: " + _score;
        string response = await RequestManager.GetRequest($"https://studenthome.hku.nl/~wridzer.kamphuis/kernmodule_networking/score_insert.php?PHPSESSID={sessionId}&score={(int)_score}&game=6");
        if (response == "0") 
        { 
            Login($"https://studenthome.hku.nl/~wridzer.kamphuis/kernmodule_networking/user_login.php?PHPSESSID={sessionId}&email={email}&password={password}");
            EndGame(_score); 
        }
    }
}

public static class RequestManager
{

    public static async Task<string> GetRequest(string _url)
    {
        using (HttpClient client = new HttpClient())
        {
            string response = await client.GetStringAsync(_url);
            if (response == "0") await ServerLogin(); // change sessionid and try again
            return response;
        }
    }
    public static async Task<string> ServerLogin()
    {
        using (HttpClient client = new HttpClient())
        {
            string response = await client.GetStringAsync("https://studenthome.hku.nl/~wridzer.kamphuis/kernmodule_networking/server_login.php?id=1&password=admin");
            return response;
        }
    }
}
