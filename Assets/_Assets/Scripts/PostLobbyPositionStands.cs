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
            racers[i].gameObject.transform.position = transform.GetChild(i).position + new Vector3(0,5,0);
        }
    }

    public void BackToLobby()
    {
        SceneTransitioner.Instance.BackToLobby();
    }
}
