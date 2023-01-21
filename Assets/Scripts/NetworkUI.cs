using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    private static string userId, nickname, sessionId;
    private bool loggedIn = false;

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
        mainMenu,
        loginMenu,
        signupMenu,
        leaderboardMenu,
        startGameMenu;
    [SerializeField] private TMP_Text mainMenuNickname;

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
    
    public void LeaderboardMenu()
    {
        leaderboardMenu.SetActive(true);
        loginMenu.SetActive(false);
        mainMenu.SetActive(false);
        signupMenu.SetActive(false);
        startGameMenu.SetActive(false);
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
        if (response == "0")
        {
            sessionId = await RequestManager.ServerLogin();
            url = $"https://studenthome.hku.nl/~wridzer.kamphuis/kernmodule_networking/user_login.php?PHPSESSID={sessionId}&email={emailLogin.text}&password={passwordLogin.text}";
            response = await RequestManager.GetRequest(url);
            if (response == null || response == "0") return; // TODO: Error
        }
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
        Debug.Log(url);
        string response = await RequestManager.GetRequest(url);
        Debug.Log(response);
        if (response == "0")
        {
            sessionId = await RequestManager.ServerLogin();
            url = url = $"https://studenthome.hku.nl/~wridzer.kamphuis/kernmodule_networking/insert_user.php?PHPSESSID={sessionId}&name={nameSignup.text}&email={emailSignup.text}&password={passwordSignup.text}&dateofbirth={dateOfBirthSignup.text}&nickname={nicknameSignup.text}";
            Debug.Log(url);
            response = await RequestManager.GetRequest(url);
            Debug.Log(response);
            if (response == null || response == "0") return; // TODO: Error
        }
        url = $"https://studenthome.hku.nl/~wridzer.kamphuis/kernmodule_networking/user_login.php?PHPSESSID={sessionId}&email={emailSignup.text}&password={passwordSignup.text}";
        response = await RequestManager.GetRequest(url);
        Debug.Log(response);
        userId = response;
        loginMenuBtn.interactable = false;
        signupMenuBtn.interactable = false;
        startBtn.interactable = true;
        nickname = await RequestManager.GetRequest($"https://studenthome.hku.nl/~wridzer.kamphuis/kernmodule_networking/fetch_userinfo.php?PHPSESSID={sessionId}");
        mainMenuNickname.text = "You are currently logged in as: " + nickname;
        Back();
    }
}

public static class RequestManager
{

    public static async Task<string> GetRequest(string _url)
    {
        using (HttpClient client = new HttpClient())
        {
            string response = await client.GetStringAsync(_url);
            return response;
        }
    }
    public static async Task<string> ServerLogin()
    {
        using (HttpClient client = new HttpClient())
        {
            string response = await client.GetStringAsync("https://studenthome.hku.nl/~wridzer.kamphuis/kernmodule_networking/server_login.php?id=1&password=admin");
            Debug.Log(response);
            return response;
        }
    }
}
