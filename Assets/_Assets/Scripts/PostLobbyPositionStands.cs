using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostLobbyPositionStands : MonoBehaviour
{

    void Start()
    {
        PlaceRacers(PreRaceInitializer.ExistingRacerStandings);
    }
    void PlaceRacers(List<MechRacer> racers)
    {
        for(int i = 0; i< transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<PostRaceStand>().SetRacerAndRank(racers[i], i+1, racers.Count);
        }
    }

    public void BackToLobby()
    {
        SceneTransitioner.Instance.BackToLobby();
    }
}
