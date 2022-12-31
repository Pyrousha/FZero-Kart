using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

///Handles spawning in as many NPC racers as needed, and intializes any needed parameters
public class RacerStandingsTracker : NetworkBehaviour
{
    //TODO: Spawn on the server
    //TODO: update existingRacerStandings on the clients each time it is updated on the server
    // OR don't update on clients and have some basic system for when no races have been played 

    #region Singleton stuff
    private static RacerStandingsTracker instance = null;

    public static RacerStandingsTracker Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<RacerStandingsTracker>();
            return instance;
        }
    }
    #endregion

    [Header("References")]
    //[SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private GameObject NPCPrefab;

    [Header("Parameters")]
    [SerializeField] private AIEvolutionStats evoStats;

    private static List<MechRacer> existingRacerStandings = new List<MechRacer>();
    public static List<MechRacer> ExistingRacerStandings => existingRacerStandings;

    bool showPositionInCard = false;

    /// <summary>
    /// Called on each client when a player joins a lobby.
    /// Adds the new player to the list of all racers and creates a lobby card if needed.
    /// </summary>
    /// <param name="racer"> Mechracer of player who has just joined </param>
    [Client]
    public static void OnPlayerJoined_Client(MechRacer racer)
    {
        if (!existingRacerStandings.Contains(racer))
        {
            //Add new racer to array of all racers and sort based on score
            existingRacerStandings.Add(racer);
            RaceController.SortRacerScores(existingRacerStandings);

            int posNum;
            //If the first place player has score > 0, then at least 1 race has been played, so rankings should be shown in card
            if ((existingRacerStandings.Count > 0) && (existingRacerStandings[0].Score > 0))
                posNum = RaceController.GetCurrentSharedPosition(existingRacerStandings.IndexOf(racer), racer, existingRacerStandings);
            else
                posNum = 0; //A position of < 1 means do not display the ranking on the lobby cards

            LobbyController.Instance.TryCreateCard(racer, posNum);
        }
        else
            Debug.Log("Duplicate player joining lobby: " + racer.gameObject.name);
    }

    [Server]
    public static void OnPlayerJoined_Server(MechRacer racer)
    {
        if (!existingRacerStandings.Contains(racer))
        {
            //Add new racer to array of all racers and sort based on score
            existingRacerStandings.Add(racer);
            RaceController.SortRacerScores(existingRacerStandings);
        }
        else
            Debug.Log("Duplicate player joining lobby: " + racer.gameObject.name);
    }

    /// <summary>
    /// Get all racers that are in the lobby, check if they are in the existingRacer list, then spawn cards for all racers.
    /// </summary>
    public void Start()
    {
        if ((Instance != null) && (Instance != this))
        {
            Debug.Log("Destroyed script type" + typeof(RacerStandingsTracker) + " on gameObject" + gameObject.name);
            Destroy(gameObject);
        }


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
        if ((existingRacerStandings.Count > 0) && (existingRacerStandings[0].Score > 0))
            showPositionInCard = true;
        else
            showPositionInCard = false;


        for (int i = 0; i < existingRacerStandings.Count; i++)
        {
            MechRacer racer = existingRacerStandings[i];

            int posNum;
            if (showPositionInCard)
                posNum = RaceController.GetCurrentSharedPosition(i, racer, existingRacerStandings);
            else
                posNum = 0;

            LobbyController.Instance.TryCreateCard(racer, posNum);
        }

        LobbyController.Instance.ReadyUpAIRacers();

        //RaceController.PrintRankings(existingRacerStandings);
    }

    /// <summary>
    /// Called when everyone is ready and the cup starts.
    /// Spawns as many NPC racers as needed, and initializes their parameters.
    /// </summary>
    [Server]
    public void InitalizeRacers()
    {
        existingRacerStandings = new List<MechRacer>(FindObjectsOfType(typeof(MechRacer)) as MechRacer[]);

        if (LobbySettings.SpawnAI)
        {
            int n = existingRacerStandings.Count;
            while (existingRacerStandings.Count < LobbySettings.NumRacers)
            {
                GameObject newRacer = Instantiate(NPCPrefab, Vector3.zero, Quaternion.identity);
                existingRacerStandings.Add(newRacer.GetComponent<MechRacer>());
                NetworkServer.Spawn(newRacer);

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
