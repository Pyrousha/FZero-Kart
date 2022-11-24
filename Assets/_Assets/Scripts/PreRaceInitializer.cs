using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///Handles spawning in as many NPC racers as needed, and intializes any needed parameters
public class PreRaceInitializer : Singleton<PreRaceInitializer>
{
    [Header("References")]
    //[SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private GameObject NPCPrefab;


    public static int NumTotalRacers { get; set; } = 30;
    public static bool SpawnAIRacers { get; set; } = true;
    [Header("Parameters")]
    [SerializeField] private AIEvolutionStats evoStats;

    private static List<MechRacer> existingRacerStandings = new List<MechRacer>();
    public static List<MechRacer> ExistingRacerStandings => existingRacerStandings;

    /// <summary>
    /// Get all racers that are in the lobby, check if they are in the existingRacer list, then spawn cards for all racers.
    /// </summary>
    public void Start()
    {
        List<MechRacer> racersInLobby = new List<MechRacer>(FindObjectsOfType(typeof(MechRacer)) as MechRacer[]);
        foreach (MechRacer racer in racersInLobby)
        {
            if (!existingRacerStandings.Contains(racer))
            {
                //Add this racer to the array
                existingRacerStandings.Add(racer);
            }
        }

        RaceController.SortRacerScores(existingRacerStandings);

        //If the first place player has score > 0, then at least 1 race has been played, so rankings should be shown in card
        bool showPositionInCard = existingRacerStandings[0].Score > 0;


        for (int i = 0; i < existingRacerStandings.Count; i++)
        {
            MechRacer racer = existingRacerStandings[i];

            int posNum;
            if (showPositionInCard)
                posNum = RaceController.GetCurrentSharedPosition(i, racer, existingRacerStandings);
            else
                posNum = 0;

            LobbyController.Instance.CreateCard(racer, posNum);
        }

        LobbyController.Instance.ReadyUpAIRacers();

        //RaceController.PrintRankings(existingRacerStandings);
    }

    /// <summary>
    /// Called when everyone is ready and the cup starts.
    /// Spawns as many NPC racers as needed, and initializes their parameters.
    /// </summary>
    public void InitalizeRacers()
    {
        existingRacerStandings = new List<MechRacer>(FindObjectsOfType(typeof(MechRacer)) as MechRacer[]);

        if (SpawnAIRacers)
        {
            int n = existingRacerStandings.Count;
            while (existingRacerStandings.Count < NumTotalRacers)
            {
                MechRacer newRacer = Instantiate(NPCPrefab, Vector3.zero, Quaternion.identity).GetComponent<MechRacer>();
                existingRacerStandings.Add(newRacer);

                newRacer.gameObject.name = "AI RACER #" + n;

                n++;
            }
        }


        #region Set AI Input Parameters
        //Set AI race stats from evolution iteration
        for (int i = 0; i < existingRacerStandings.Count; i++)
        {
            int racerIndex = (i % evoStats.stats.Count);
            existingRacerStandings[i].GetComponent<NPCController>().SetAIParams(evoStats.stats[racerIndex]);
        }
        #endregion

        //RaceController.PrintRankings(existingRacerStandings);
    }

    public static void UpdateRacerStandings(List<MechRacer> newStandings)
    {
        existingRacerStandings = new List<MechRacer>(newStandings);

        //RaceController.PrintRankings(existingRacerStandings);
    }
}
