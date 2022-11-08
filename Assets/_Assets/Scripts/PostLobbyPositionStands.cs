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
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i >= racers.Count)
            {
                //Less racers than stands, remove excess stands
                transform.GetChild(i).gameObject.SetActive(false);
            }
            else
            {
                //There is a racer for this stand, set values
                int racerPos = RaceController.GetSharedPosition(i, racers[i], racers);
                transform.GetChild(i).GetComponent<PostRaceStand>().SetRacerAndRank(racers[i], racerPos, racers.Count);
            }
        }
    }

    public void BackToLobby()
    {
        SceneTransitioner.Instance.BackToLobby();
    }
}
