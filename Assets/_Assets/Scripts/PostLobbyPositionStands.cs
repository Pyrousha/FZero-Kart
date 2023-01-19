using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostLobbyPositionStands : MonoBehaviour
{
    [SerializeField] private Transform spawnLineParent;
    [SerializeField] private int numRacersPerLine;

    void Start()
    {
        PlaceRacers(PreRaceInitializer.ExistingRacerStandings);
    }
    void PlaceRacers(List<MechRacer> racers)
    {
        int numPodiums = transform.childCount;

        //place racers on podium
        for (int i = 0; i < numPodiums; i++)
        {
            if (i >= racers.Count)
            {
                //Less racers than stands, remove excess stands
                transform.GetChild(i).gameObject.SetActive(false);
            }
            else
            {
                //There is a racer for this stand, set values
                int racerPos = RaceController.GetCurrentSharedPosition(i, racers[i], racers);
                transform.GetChild(i).GetComponent<PostRaceStand>().SetRacerAndRank(racers[i], racerPos, racers.Count);
            }
        }

        List<Vector3> bgRacerSpacePositions = new List<Vector3>(); //positions to place racers that aren't on podium

        List<Transform> spawnLines = Utils.GetChildrenOfTransform(spawnLineParent);

        //Convert spawnlines into list of vector3s
        foreach (Transform spawnLine in spawnLines)
        {
            Vector3 lineStart = spawnLine.position;
            Vector3 lineEnd = spawnLine.GetChild(0).position;
            for (int i = 0; i < numRacersPerLine; i++)
            {
                //Take the line from startPos to endPos, and evenly break it up into enough chunks to place n racers
                Vector3 pos = Vector3.Lerp(lineStart, lineEnd, ((float)i) / (numRacersPerLine - 1));
                bgRacerSpacePositions.Add(pos);
            }
        }

        //Place racers not on podium
        for (int i = numPodiums; i < racers.Count; i++)
        {
            racers[i].transform.position = bgRacerSpacePositions[i - numPodiums] + new Vector3(0, 20, 0);
        }
    }

    /// <summary>
    /// [TEMP] Called when the "back to lobby" button is clicked, heads back to the preRace lobby
    /// </summary>
    public void BackToLobby()
    {
        SceneTransitioner.Instance.BackToLobbyAfterScoreScene();
    }
}
