using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : Singleton<SceneTransitioner>
{
    [SerializeField] private int lobbySceneIndex;
    [SerializeField] private int scoreSceneIndex;

    [System.Serializable]
    public struct RaceCupStruct
    {
        public List<int> raceBuildIndices;
    }

    [SerializeField] private List<RaceCupStruct> cups;

    private RaceCupStruct currCup;
    private int currentRaceIndex; //Index of current race in currCup (so from 0 to length-1)

    void Start()
    {
        //Load lobby scene
        SceneManager.LoadScene(lobbySceneIndex);
    }

    public void StartCup(int cupIndex)
    {
        PreRaceInitializer.Instance.InitalizeRacers();

        currCup = cups[cupIndex];
        currentRaceIndex = -1;
        ToNextRace();
        //Spawn racers and load first race
    }


    // void Update()
    // {
    //     Debug.Log("count: "+PreRaceInitializer.ExistingRacerStandings.Count);
    // }

    public void ToNextRace()
    {
        currentRaceIndex++;

        int sceneIndexToLoad;
        if (currentRaceIndex < currCup.raceBuildIndices.Count)
        {
            //load next race scene
            sceneIndexToLoad = currCup.raceBuildIndices[currentRaceIndex];
        }
        else
        {
            //all races played, go to score screen
            sceneIndexToLoad = scoreSceneIndex;
        }

        foreach(MechRacer racer in PreRaceInitializer.ExistingRacerStandings)
        {
            racer.OnNewRaceLoading();
        }

        SceneManager.LoadScene(sceneIndexToLoad);
    }

    /// <summary>
    /// Destroys all AI racer gameobjects and then loads back into the pre-race lobby scene
    /// <summary>
    public void BackToLobby()
    {
        //Destroy all AI racers
        for(int i = 0; i< PreRaceInitializer.ExistingRacerStandings.Count; i++)
        {
            MechRacer currRacer = PreRaceInitializer.ExistingRacerStandings[i];
            if(currRacer.IsHuman)
            {
                currRacer.OnEnterLobby();
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
