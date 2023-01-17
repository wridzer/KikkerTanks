using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MainMenuUI : MonoBehaviour
{
    [Header("Menu UI's")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject lobbyMenu;
    [SerializeField] private GameObject createMenu;
    [SerializeField] private GameObject joinMenu;
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private TMP_Text lobbyName;

    [Header("UI prefans")]
    [SerializeField] private GameObject lobbyPrefab;
    [SerializeField] private GameObject playerPrefab;

    private Lobby currentLobby = new Lobby();


    private void Start()
    {
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void JoinLobby(Lobby _lobby) // Lobby param?
    {
        lobbyMenu.SetActive(true);
        createMenu.SetActive(false);
        joinMenu.SetActive(false);
        lobbyName.text = _lobby.Name;
        //Server.Join()?
    }

    public void CreateLobby()
    {
        //Lobby lobby = new Lobby() { Name = lobbyNameInput.text, Server = new Server()};
        //JoinLobby(lobby);
        //this.GetComponent<ClientServerSelection>().GoServer();
    }

    public void JoinLobbyScreen()
    {
        joinMenu.SetActive(true);
        mainMenu.SetActive(false);

        //this.GetComponent<ClientServerSelection>().GoClient();

        // load lobbys from server
        // foreach lobby create lobbyprefab and text = lobbyname
    }

    public void CreateLobbyScreen()
    {
        createMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void StartGame()
    {
        // load scene
        // create players
    }

    public void LeaveLobby()
    {
        if (currentLobby.Name != null)
        { 
            // Exit Lobby
            // if last then destroy lobby
        }
        mainMenu.SetActive(true);
        lobbyMenu.SetActive(false);
        createMenu.SetActive(false);
    }
}

public struct Lobby
{
    public string Name;
    //public Server Server;
}
