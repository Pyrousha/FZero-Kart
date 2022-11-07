using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///Handles spawning in as many NPC racers as needed, and intializes any needed parameters
public class PreRaceInitializer : Singleton<PreRaceInitializer>
{
    [Header("References")]
    //[SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private GameObject NPCPrefab;

    [Header("Parameters")]
    [SerializeField] private int numTotalRacers;
    [SerializeField] private AIEvolutionStats evoStats;

    private static List<MechRacer> existingRacerStandings = new List<MechRacer>();
    public static List<MechRacer> ExistingRacerStandings => existingRacerStandings;

    /// <summary>
    /// Spawns as many NPC racers as needed, and initializes their parameters
    /// </summary>
    public void InitalizeRacers()
    {
        existingRacerStandings = new List<MechRacer>(FindObjectsOfType(typeof(MechRacer)) as MechRacer[]);

        int n = existingRacerStandings.Count;
        while (existingRacerStandings.Count < numTotalRacers)
        {
            MechRacer newRacer = Instantiate(NPCPrefab, Vector3.zero, Quaternion.identity).GetComponent<MechRacer>();
            existingRacerStandings.Add(newRacer);

            newRacer.gameObject.name = "AI RACER #" + n;

            n++;
        }


        #region Set AI Input Parameters
        //Set AI race stats from evolution iteration
        for (int i = 0; i < existingRacerStandings.Count; i++)
        {
            int racerIndex = (i % evoStats.stats.Count);
            existingRacerStandings[i].GetComponent<NPCController>().SetAIParams(evoStats.stats[racerIndex]);
        }
        #endregion
    }

    public static void UpdateRacerStandings(List<MechRacer> newStandings)
    {
        existingRacerStandings = new List<MechRacer>(newStandings);
    }
}
