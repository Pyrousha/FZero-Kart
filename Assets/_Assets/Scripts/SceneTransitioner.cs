using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : Singleton<SceneTransitioner>
{
    [SerializeField] private int scoreSceneIndex;

    [System.Serializable]
    public struct RaceCupStruct
    {
        public List<int> raceBuildIndices;
    }

    [SerializeField] private List<RaceCupStruct> cups;

    private RaceCupStruct currCup;
    private int currentRaceIndex; //Index of current race in currCup (so from 0 to length-1)


    public void StartCup(int cupIndex)
    {
        PreRaceInitializer.Instance.InitalizeRacers();

        currCup = cups[cupIndex];
        currentRaceIndex = -1;
        ToNextRace();
        //Spawn racers and load first race
    }


    public void ToNextRace()
    {
        currentRaceIndex++;

        int sceneIndexToLoad;
        if(currentRaceIndex < currCup.raceBuildIndices.Count)
        {
            //load next race scene
            sceneIndexToLoad = currCup.raceBuildIndices[currentRaceIndex];
        }
        else
        {
            //all races played, go to score screen
            sceneIndexToLoad = scoreSceneIndex;
        }

        SceneManager.LoadScene(sceneIndexToLoad);
    }
}
