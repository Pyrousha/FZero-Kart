using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : Singleton<SceneTransitioner>
{
    private enum RaceTypeEnum
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

    private int totalVSRaces; //how many races to play for this VS race series
    private int racesCompleted; //Index of current race in currCup (so from 0 to length-1)

    public bool isFirstRace => (racesCompleted == 0);

    void Start()
    {
        //Load lobby scene
        SceneManager.LoadScene(lobbySceneIndex);
    }

    public void StartCup(int cupIndex)
    {
        PreRaceInitializer.Instance.InitalizeRacers();

        currCup = cups[cupIndex];
        racesCompleted = 0;
        ToNextRaceInCup();
        //Spawn racers and load first race
    }


    /// <summary>
    /// Called when a race has finished, determines what scene to load next (either to lobby, next race, or end screen)
    /// </summary>
    public void OnRaceEnded()
    {
        foreach (MechRacer racer in PreRaceInitializer.ExistingRacerStandings)
        {
            racer.OnNewRaceLoading();
        }

        switch (raceType)
        {
            case RaceTypeEnum.GrandPrix:
                {
                    racesCompleted++;

                    ToNextRaceInCup();
                    break;
                }
            case RaceTypeEnum.VsRace:
                {
                    racesCompleted++;

                    ToNextRaceInVS();
                    break;
                }
            case RaceTypeEnum.Endless:
                {
                    break;
                }
            case RaceTypeEnum.BattleRoyale:
                {
                    break;
                }
        }
    }

    /// <summary>
    /// Called when a race in a cup has finished, loads the next race or goes to score screen.
    /// </summary>
    private void ToNextRaceInCup()
    {
        if (racesCompleted < currCup.raceBuildIndices.Count)
        {
            //load next race scene
            SceneManager.LoadScene(currCup.raceBuildIndices[racesCompleted]);
        }
        else
        {
            //all races played, go to score screen
            SceneManager.LoadScene(scoreSceneIndex);
        }
    }

    /// <summary>
    /// Called when a race in a cup has finished, loads the next race or goes to score screen.
    /// </summary>
    private void ToNextRaceInVS()
    {
        if (racesCompleted < totalVSRaces)
        {
            //Head back to the lobby and vote for the next race
            for (int i = 0; i < PreRaceInitializer.ExistingRacerStandings.Count; i++)
            {
                MechRacer currRacer = PreRaceInitializer.ExistingRacerStandings[i];
                currRacer.OnEnterLobby(false);
            }


            SceneManager.LoadScene(lobbySceneIndex);
        }
        else
        {
            //all races played, go to score screen
            SceneManager.LoadScene(scoreSceneIndex);
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
}
