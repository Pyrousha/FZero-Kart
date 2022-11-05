using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///Handles spawning in as many NPC racers as needed, and places racers on the track based on their score
public class RaceStartPositioner : MonoBehaviour
{
    [System.Serializable]
    private struct SpawnLine
    {
        public Transform startPosition;
        public Transform endPosition;
        public int numberOfRacersOnLine;
    }

    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private GameObject NPCPrefab;
    [SerializeField] private GameObject SpawnIndicator;
    [SerializeField] private List<SpawnLine> spawnLines;


    [Header("Parameters")]
    [SerializeField] private int numTotalRacers;
    public static List<MechRacer> existingRacerPositions { get; set; }

    private List<Vector3> racerSpacePositions = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        existingRacerPositions = new List<MechRacer>(FindObjectsOfType(typeof(MechRacer)) as MechRacer[]);

        //Convert spawnlines into list of vector3s
        foreach (SpawnLine spawnLine in spawnLines)
        {
            for (int i = 0; i < spawnLine.numberOfRacersOnLine; i++)
            {
                //Take the line from startPos to endPos, and evenly break it up into enough chunks to place n racers
                Vector3 pos = Vector3.Lerp(spawnLine.startPosition.position, spawnLine.endPosition.position, ((float)i) / (spawnLine.numberOfRacersOnLine - 1));
                racerSpacePositions.Add(pos);

                RaycastHit _hit;
                if (Physics.Raycast(pos, -transform.up, out _hit, 50, groundLayer))
                {
                    Instantiate(SpawnIndicator, _hit.point, Quaternion.LookRotation(transform.forward,_hit.normal));
                }
            }
        }

        int n = existingRacerPositions.Count;
        while (existingRacerPositions.Count < numTotalRacers)
        {
            MechRacer newRacer = Instantiate(NPCPrefab, Vector3.zero, Quaternion.identity).GetComponent<MechRacer>();
            existingRacerPositions.Add(newRacer);

            newRacer.gameObject.name = "AI RACER #"+n;
            n++;
        }

        for (int i = 0; i < existingRacerPositions.Count; i++)
        {
            existingRacerPositions[i].transform.position = racerSpacePositions[numTotalRacers - 1 - i];
        }
    }
}
