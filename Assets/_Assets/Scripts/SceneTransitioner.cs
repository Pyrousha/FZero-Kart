using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LobbySettings;

public class SceneTransitioner : Singleton<SceneTransitioner>
{
    public enum RaceTypeEnum
    {
        Story,
        TimeTrial,
        GrandPrix, //structed set of bundled races (predetermined order) (4 races per cup)
        CustomVsRace, //Series of races, # set by the player, option to vote for each track or random
        BattleRoyale, //Only 1 race at a time, defeat other players and survive
        QuickPlay, //Online multiplayer, only 1 race at a time and then voting between, also points earned is persistent
    }

    public bool SingleplayerMode { get; set; }
    public RaceTypeEnum RaceType { get; private set; }

    public LobbyVoteType_Enum VoteType { get; private set; } = LobbyVoteType_Enum.HostPick;
    //[SerializeField] private GameObject player;
    private GameObject player;

    [SerializeField] private int StorySceneIndex;
    [SerializeField] private int lobbySceneIndex;
    [SerializeField] private int scoreSceneIndex;
    [SerializeField] private int mainMenuSceneIndex;
    [SerializeField] private int emptySceneIndex;
    [SerializeField] private bool loadIntoEmptySceneInsteadOfMainMenu;

    //private int totalVSRaces; //how many races to play for this VS race series
    public int NumRacesCompleted { get; private set; } //Index of current race in currCup (so from 0 to length-1)
    public int NumRacesToPlay { get; private set; }

    private bool headBackToLobbyBetweenRaces;

    public bool IsFirstRace => (NumRacesCompleted == 0);

    public bool IsLocalPlayerHost { get; set; } = false;

    /// <summary>
    /// Called right after the local player is created, but before the lobby scene is loaded 
    /// </summary>
    /// <param name="_player"> Newly spawned player GameObject </param>
    public void SetLocalPlayer(GameObject _player)
    {
        player = _player;

        if (IsLocalPlayerHost)
            LobbyController.HostRacer = player.GetComponent<MechRacer>();
    }

    public void Start()
    {
        //Load main menu scene
        ToMainMenu();
    }

    /// <summary>
    /// Called when the player clicks on a gamemode button,
    /// Sets the gamemode type and singleplayer/multiplayer
    /// </summary>
    /// <param name="_gameModeButton"></param>
    public void GamemodeSelected(GamemodeButton _gameModeButton)
    {
        RaceType = _gameModeButton.RaceType;
        SingleplayerMode = _gameModeButton.Singleplayer;

        //TEMP: assume selecting gamemode makes local player host
        if (_gameModeButton.LoadSceneWhenClicked)
        {
            if (SingleplayerMode)
                NetworkManagerFZeroKart.SoloServer.StartServer();
            else
                NetworkManagerFZeroKart.MultiServer.StartServer();
        }
    }

    /// <summary>
    /// Called when the player selects a gamemode, either when creating or entering a lobby
    /// </summary>
    /// <param name="_raceType"> Race gamemode to play </param>
    /// <param name="_racesToPlay"> How many races to play before seeing score screen and reseting scores </param>
    /// <param name="_voteType"> How courses are selected (defaults to host pick) </param>
    public void SetRaceType(RaceTypeEnum _raceType, int _racesToPlay = -1, LobbyVoteType_Enum _voteType = LobbyVoteType_Enum.HostPick)
    {
        RaceType = _raceType;

        VoteType = _voteType;
        NumRacesCompleted = 0;

        switch (_raceType)
        {
            case RaceTypeEnum.GrandPrix:
                {
                    headBackToLobbyBetweenRaces = false;

                    if (_racesToPlay > 0)
                        NumRacesToPlay = _racesToPlay;
                    else
                        Debug.LogError("trying to set nonpositive number of races to play");
                    break;
                }
            case RaceTypeEnum.CustomVsRace:
                {
                    if (_voteType == LobbyVoteType_Enum.InOrder || _voteType == LobbyVoteType_Enum.Random)
                        headBackToLobbyBetweenRaces = false;
                    else
                        headBackToLobbyBetweenRaces = true;

                    if (_racesToPlay > 0)
                        NumRacesToPlay = _racesToPlay;
                    else
                        Debug.LogError("trying to set nonpositive number of races to play");
                    break;
                }
            case RaceTypeEnum.Story:
                {
                    Debug.LogError("Should not set sceneTransitioner racetype to \"Story\"");
                    break;
                }
            default: //time trial, battleRoyale, or quickPlay
                {
                    headBackToLobbyBetweenRaces = false;

                    NumRacesToPlay = 1;
                    break;
                }
        }
    }

    private RaceCup currCup;

    /// <summary>
    /// Sets what cup the local player has selected
    /// </summary>
    /// <param name="_cup"></param>
    public void SetCup(RaceCup _cup)
    {
        currCup = _cup;
        NumRacesToPlay = _cup.Courses.Count;
    }

    private RaceCourse currTrack;

    /// <summary>
    /// Sets what track the local player has selected
    /// </summary>
    /// <param name="_cup"></param>
    public void SetLocalTrack(RaceCourse _track)
    {
        currTrack = _track;
    }

    /// <summary>
    /// Called after host presses "start race", initializes racers (if needed) and loads race
    /// </summary>
    [Server]
    public void StartRace()
    {
        if (IsFirstRace)
            RacerStandingsTracker.Instance.InitalizeRacers();

        LoadNextRace();
    }

    /// <summary>
    /// Called when the race has finished for all players, determines what scene to load next (either to lobby, next race, or end screen)
    /// </summary>
    public void OnRaceFinished()
    {
        NumRacesCompleted++;

        if (NumRacesCompleted >= NumRacesToPlay)
        {
            //Save data
            switch (RaceType)
            {
                case RaceTypeEnum.GrandPrix:
                    {
                        MechRacer playerMech = player.GetComponent<MechRacer>();
                        int playerPos = RacerStandingsTracker.ExistingRacerStandings.IndexOf(playerMech) + 1;
                        currCup.OnCupFinished(playerPos);
                        break;
                    }
            }

            //Played all races, load the score scene
            SceneManager.LoadScene(scoreSceneIndex);
        }
        else
        {
            if (headBackToLobbyBetweenRaces)
            {
                //Head back to the lobby for voting
                for (int i = 0; i < RacerStandingsTracker.ExistingRacerStandings.Count; i++)
                {
                    MechRacer currRacer = RacerStandingsTracker.ExistingRacerStandings[i];
                    currRacer.OnEnterLobby(false);
                }

                SceneManager.LoadScene(lobbySceneIndex);
                StartCoroutine(EnableLobbyController());
            }
            else
            {
                //Load right into the next race (Grand Prix only so far)
                LoadNextRace();
            }
        }
    }

    /// <summary>
    /// Loads the next race scene based on race type
    /// </summary>
    [Server]
    private void LoadNextRace()
    {
        foreach (MechRacer racer in RacerStandingsTracker.ExistingRacerStandings)
        {
            racer.OnNewRaceLoading_Server();
        }

        switch (RaceType)
        {
            case RaceTypeEnum.GrandPrix:
                {
                    SceneManager.LoadScene(currCup.Courses[NumRacesCompleted].BuildIndex);
                    break;
                }
            default:
                {
                    SceneManager.LoadScene(currTrack.BuildIndex);
                    break;
                }
        }
    }

    /// <summary>
    /// Destroys all AI racer gameobjects and then loads back into the pre-race lobby scene
    /// <summary>
    public void BackToLobbyAfterScoreScene()
    {
        //Destroy all AI racers
        DestroyAIRacers();

        //Reset how many races have been played
        NumRacesCompleted = 0;

        //Load lobby scene
        SceneManager.LoadScene(lobbySceneIndex);
        StartCoroutine(EnableLobbyController());
    }

    private IEnumerator EnableLobbyController()
    {
        yield return new WaitForSeconds(0.1f);
        LobbyController.Instance.gameObject.SetActive(true);
    }

    /// <summary>
    /// Loads into the Lobby scene and then joins the localhost server
    /// </summary>
    public void JoinExistingServer()
    {
        StartCoroutine(ClientInitialization());
    }

    private IEnumerator ClientInitialization()
    {
        //Load lobby scene
        SceneManager.LoadScene(lobbySceneIndex);

        yield return new WaitForSeconds(0.1f);

        //Join server
        NetworkManagerFZeroKart.MultiServer.StartClient();
    }

    /// <summary>
    /// Loads into the Lobby scene and then opens the server
    /// </summary>
    public void StartNewServer()
    {
        StartCoroutine(ServerInitialization());
    }

    private IEnumerator ServerInitialization()
    {
        //Load lobby scene
        SceneManager.LoadScene(lobbySceneIndex);

        yield return new WaitForSeconds(0.1f);

        //Start server
        NetworkManagerFZeroKart.MultiServer.StartHost();
    }

    private void DestroyAIRacers()
    {
        for (int i = 0; i < RacerStandingsTracker.ExistingRacerStandings.Count; i++)
        {
            MechRacer currRacer = RacerStandingsTracker.ExistingRacerStandings[i];
            if (currRacer.IsHuman)
            {
                //reset score
                currRacer.ResetScore();
            }
            else
            {
                RacerStandingsTracker.ExistingRacerStandings.RemoveAt(i);
                Destroy(currRacer.gameObject);
                i--;
            }
        }
    }

    /// <summary>
    /// Called when the player creates or joins a lobby (both singleplayer or multiplayer)
    /// </summary>
    /// <param name="_hostNewLobby"> Is the player hosting a new lobby (track host for who gets to start the game) </param>
    // public void LoadIntoLobby(bool _hostNewLobby = false)
    // {
    //     //player.SetActive(true);

    //     if (_hostNewLobby)
    //         LobbyController.HostRacer = player.GetComponent<MechRacer>();

    //     //SceneManager.LoadScene(lobbySceneIndex);
    // }

    /// <summary>
    /// Loads back to the Main Menu
    /// </summary>
    public void ToMainMenu()
    {
        if (loadIntoEmptySceneInsteadOfMainMenu)
        {
            SceneManager.LoadScene(emptySceneIndex);
        }
        else
        {
            DestroyAIRacers();

            SceneManager.LoadScene(mainMenuSceneIndex);
        }
    }
}