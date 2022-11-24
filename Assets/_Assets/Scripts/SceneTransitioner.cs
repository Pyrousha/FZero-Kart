using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private GameObject player;

    [SerializeField] private int StorySceneIndex;
    [SerializeField] private int lobbySceneIndex;
    [SerializeField] private int scoreSceneIndex;
    [SerializeField] private int mainMenuSceneIndex;

    //private int totalVSRaces; //how many races to play for this VS race series
    private int numRacesCompleted; //Index of current race in currCup (so from 0 to length-1)
    private int numRacesToPlay;

    private bool headBackToLobbyBetweenRaces;

    public bool IsFirstRace => (numRacesCompleted == 0);

    void Start()
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

        //TEMP: Set preraceinitializer settings
        PreRaceInitializer.SpawnAIRacers = true;
        PreRaceInitializer.NumTotalRacers = 32;

        //TEMP: assume selecting gamemode makes local player host
        if (_gameModeButton.LoadSceneWhenClicked)
            LoadIntoLobby(!SingleplayerMode);
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
        numRacesCompleted = 0;

        switch (_raceType)
        {
            case RaceTypeEnum.GrandPrix:
                {
                    headBackToLobbyBetweenRaces = false;

                    if (_racesToPlay > 0)
                        numRacesToPlay = _racesToPlay;
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
                        numRacesToPlay = _racesToPlay;
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

                    numRacesToPlay = 1;
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
        numRacesToPlay = _cup.Courses.Count;
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
    /// Spawn in NPC racers and load the first (or only) race
    /// </summary>
    public void StartFirstRace()
    {
        //Spawn racers and load first race
        PreRaceInitializer.Instance.InitalizeRacers();

        LoadNextRace();
    }

    /// <summary>
    /// Called when the race has finished forr all players, determines what scene to load next (either to lobby, next race, or end screen)
    /// </summary>
    public void OnRaceFinished()
    {
        numRacesCompleted++;

        if (numRacesCompleted >= numRacesToPlay)
        {
            //Save data
            switch (RaceType)
            {
                case RaceTypeEnum.GrandPrix:
                    {
                        MechRacer playerMech = player.GetComponent<MechRacer>();
                        int playerPos = PreRaceInitializer.ExistingRacerStandings.IndexOf(playerMech) + 1;
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
                for (int i = 0; i < PreRaceInitializer.ExistingRacerStandings.Count; i++)
                {
                    MechRacer currRacer = PreRaceInitializer.ExistingRacerStandings[i];
                    currRacer.OnEnterLobby(false);
                }

                SceneManager.LoadScene(lobbySceneIndex);
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
    private void LoadNextRace()
    {
        foreach (MechRacer racer in PreRaceInitializer.ExistingRacerStandings)
        {
            racer.OnNewRaceLoading();
        }

        switch (RaceType)
        {
            case RaceTypeEnum.GrandPrix:
                {
                    SceneManager.LoadScene(currCup.Courses[numRacesCompleted].BuildIndex);
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
        for (int i = 0; i < PreRaceInitializer.ExistingRacerStandings.Count; i++)
        {
            MechRacer currRacer = PreRaceInitializer.ExistingRacerStandings[i];
            if (currRacer.IsHuman)
            {
                //Cup has ended, so reset score
                currRacer.OnEnterLobby(true);
            }
            else
            {
                PreRaceInitializer.ExistingRacerStandings.RemoveAt(i);
                Destroy(currRacer.gameObject);
                i--;
            }
        }

        //Load lobby scene
        SceneManager.LoadScene(lobbySceneIndex);
    }

    /// <summary>
    /// Called when the player creates or joins a lobby (both singleplayer or multiplayer)
    /// </summary>
    /// <param name="_hostNewLobby"> Is the player hosting a new lobby (host for start game comfirmation) </param>
    public void LoadIntoLobby(bool _hostNewLobby = false)
    {
        player.SetActive(true);

        if (_hostNewLobby)
            LobbyController.HostRacer = player.GetComponent<MechRacer>();

        SceneManager.LoadScene(lobbySceneIndex);
    }

    /// <summary>
    /// Loads back to the Main Menu
    /// </summary>
    public void ToMainMenu()
    {
        player.SetActive(false);

        SceneManager.LoadScene(mainMenuSceneIndex);
    }
}