using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : Singleton<SceneTransitioner>
{
    public enum RaceTypeEnum
    {
        GrandPrix, //structed set of bundled races (predetermined order) (4 races per cup)
        VsRace, //Series of races, # set by the player, option to vote for each track or random
        Endless, //Online multiplayer, only 1 race at a time and then voting between, also points earned is persistent
        BattleRoyale //Only 1 race at a time
    }

    private RaceTypeEnum raceType;

    [SerializeField] private int lobbySceneIndex;
    [SerializeField] private int scoreSceneIndex;

    [System.Serializable]
    public struct RaceCupStruct
    {
        public List<int> raceBuildIndices;
    }

    [SerializeField] private List<RaceCupStruct> cups;
    private RaceCupStruct currCup;

    //private int totalVSRaces; //how many races to play for this VS race series
    private int numRacesCompleted; //Index of current race in currCup (so from 0 to length-1)
    private int numRacesToPlay;

    private bool headBackToLobbyBetweenRaces;

    public bool isFirstRace => (numRacesCompleted == 0);

    void Start()
    {
        //Load lobby scene
        SceneManager.LoadScene(lobbySceneIndex);
    }

    public void SetRaceType(RaceTypeEnum _raceType, int _racesToPlay = 1, bool vsRaceBackToLobby = true)
    {
        raceType = _raceType;
        numRacesCompleted = 0;

        switch (_raceType)
        {
            case (RaceTypeEnum.GrandPrix):
                {
                    headBackToLobbyBetweenRaces = false;

                    numRacesToPlay = _racesToPlay;
                    break;
                }
            case (RaceTypeEnum.VsRace):
                {
                    headBackToLobbyBetweenRaces = vsRaceBackToLobby;

                    numRacesToPlay = _racesToPlay;
                    break;
                }
            case (RaceTypeEnum.Endless):
                {
                    headBackToLobbyBetweenRaces = false;

                    numRacesToPlay = 1;
                    break;
                }
            case (RaceTypeEnum.BattleRoyale):
                {
                    headBackToLobbyBetweenRaces = false;

                    numRacesToPlay = 1;
                    break;
                }
        }
    }

    public void StartCup(int cupIndex)
    {
        PreRaceInitializer.Instance.InitalizeRacers();

        currCup = cups[cupIndex];
        SetRaceType(RaceTypeEnum.GrandPrix, currCup.raceBuildIndices.Count);

        LoadRace(currCup.raceBuildIndices[0]);
        //Spawn racers and load first race
    }

    /// <summary>
    /// Called when the race has finished forr all players, determines what scene to load next (either to lobby, next race, or end screen)
    /// </summary>
    public void OnRaceFinished()
    {
        numRacesCompleted++;

        if (numRacesCompleted >= numRacesToPlay)
        {
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
                LoadRace(currCup.raceBuildIndices[numRacesCompleted]);
            }
        }
    }


    /// <summary>
    /// Loads the given race scene index 
    /// </summary>
    /// <param name="raceIndex"> build index of race to load </param>
    public void LoadRace(int raceIndex)
    {
        foreach (MechRacer racer in PreRaceInitializer.ExistingRacerStandings)
        {
            racer.OnNewRaceLoading();
        }

        SceneManager.LoadScene(raceIndex);
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
}
