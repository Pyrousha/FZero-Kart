using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Singleton that holds all information about the current race.
/// Mainly tracks the race position of all racers, handles race setup, and race end code. 
/// Also is in charge of updating UI elements such as race position and lap count.
/// </summary>
public class RaceController : Singleton<RaceController>
{
    [System.Serializable]
    public struct EvolutionRacerPair
    {
        public Transform NPCParent;
        public AIEvolutionStats evoStats;
    }

    [SerializeField] private bool cutPowerToLastHumanPlayer; //If the last player is a human, should the race end prematurely or let them finish?

    [Header("References")]
    public Material activeCheckpointMaterial;
    public Material inactiveCheckpointMaterial;
    [Space(5)]
    [SerializeField] private TextMeshProUGUI posNumberText;
    [SerializeField] private TextMeshProUGUI posSuffixText;
    [SerializeField] private TextMeshProUGUI numAlivePlayersText;
    [SerializeField] private TextMeshProUGUI lapCountText;
    [SerializeField] private TextMeshProUGUI speedNumberText;
    public TextMeshProUGUI SpeedNumberText => speedNumberText;
    [Space(5)]
    [SerializeField] private GameObject raceOverCanvas;
    [Space(5)]
    [SerializeField] private ParticleSystemModifier speedLines;
    public ParticleSystemModifier SpeedLines => speedLines;

    public Checkpoint endCheckpoint { get; private set; }
    private List<MechRacer> finishedRacers = new List<MechRacer>();
    private List<MechRacer> currentRacers = new List<MechRacer>();

    [Header("Track Specific References/Settings")]
    [SerializeField] private int totalLaps;
    [SerializeField] private Transform checkpointParent;
    private List<Checkpoint> checkpoints;
    public int TotalLaps => totalLaps;

    [Header("AI Evolution Funsies")]
    [SerializeField] private bool saveAIRacers;
    [SerializeField] private EvolutionRacerPair racerPair; //NPC racers that will be in the actual game, referenced to set values from evolution stats
    [SerializeField] private List<EvolutionRacerPair> evolutionRacerPairs;

    private float raceStartTime;
    private bool savedParams = false;

    public void Start()
    {
        evolutionRacerPairs.Add(racerPair);
        foreach (EvolutionRacerPair pair in evolutionRacerPairs)
        {
            Transform NPCParent = pair.NPCParent;
            AIEvolutionStats evoStats = pair.evoStats;

            //Set AI race stats from evolution iteration
            for (int i = 0; i < NPCParent.childCount; i++)
            {
                int racerIndex = (i % evoStats.stats.Count);
                NPCParent.GetChild(i).GetComponent<NPCController>().SetAIParams(evoStats.stats[racerIndex]);
            }
        }

        checkpoints = new List<Checkpoint>(checkpointParent.GetComponentsInChildren<Checkpoint>());

        //Link checkpoints to each other
        Checkpoint firstCheckpoint = checkpoints[0];
        Checkpoint lastCheckpoint = checkpoints[checkpoints.Count - 1];

        //Set next checkpoints
        for (int i = 0; i < checkpoints.Count - 1; i++)
        {
            checkpoints[i].SetNextCheckpoint(checkpoints[i + 1]);
        }
        lastCheckpoint.SetNextCheckpoint(firstCheckpoint);

        //Set previous checkpoints
        for (int i = 1; i < checkpoints.Count; i++)
        {
            checkpoints[i].SetPrevCheckpoint(checkpoints[i - 1]);
        }
        firstCheckpoint.SetPrevCheckpoint(lastCheckpoint);

        //Set end checkpoint var
        endCheckpoint = lastCheckpoint;

        UpdateLapUI(0);

        Invoke("StartRace", 1f);
    }

    public void StartRace()
    {
        foreach (MechRacer racer in currentRacers)
        {
            racer.OnRaceStarted();
        }

        raceStartTime = Time.time;
    }

    public void AddRacer(MechRacer racer)
    {
        currentRacers.Add(racer);
        racer.SetNextCheckpoint(checkpoints[0]);
    }

    public void Update()
    {
        currentRacers.Sort(CompareRacerPositions);

        //Update position UI for the local racers
        for (int i = 0; i < currentRacers.Count; i++)
        {
            MechRacer mechRacer = currentRacers[i];
            if (mechRacer.IsLocalPlayer)
            {
                UpdateRacePosUI(i + 1 + finishedRacers.Count);
                return;
            }
        }
    }

    public void DestroyRacer(MechRacer racer)
    {
        if (finishedRacers.Contains(racer))
        {
            Debug.LogError("Cannot destroy finished racer: " + racer.name);
            return;
        }

        if (currentRacers.Remove(racer))
        {
            Debug.Log("Racer " + racer.name + " has been disqualified!");

            //if this was the last remaining racer, end the race
            CheckShouldEndRace();

            //Update UI showing #alive racers
            numAlivePlayersText.text = (currentRacers.Count + finishedRacers.Count).ToString();
        }
        else
        {
            Debug.LogError("Cannot destroy nonactive racer: " + racer.name);
        }
    }

    /// <summary>
    /// called when a racer finishes the race (passes last checkpoint on final lap)
    /// </summary>
    /// <param name="mechRacer"> racer that just finished </param>
    public void FinishedRace(MechRacer mechRacer)
    {
        currentRacers.Remove(mechRacer);
        finishedRacers.Add(mechRacer);

        //Set position UI based on how many finished before
        if (mechRacer.IsLocalPlayer)
            UpdateRacePosUI(finishedRacers.Count);

        //End the race if all racers have finished/died
        CheckShouldEndRace();

        if (saveAIRacers)
        {
            //AI param saving
            if ((savedParams == false) && (finishedRacers.Count >= 15))
            {
                SaveAIRacerParams(finishedRacers);

                savedParams = true;
            }
        }
    }

    private void CheckShouldEndRace()
    {
        //Outcome still uncertain
        if(currentRacers.Count >= 2)
            return;

        //Cut power and end race
        if(currentRacers.Count == 1)
        {   if(currentRacers[0].IsHuman)
            {
                //Last racer is human, check to see if power should be cut
                if(cutPowerToLastHumanPlayer)
                    currentRacers[0].LastRacerLeft();
                else
                    return; //stop race from ending
            }
            else
            {
                //last racer left is an AI, no need to check to cut power
                currentRacers[0].LastRacerLeft();
            }
        }

        //all racers have finished!
        Debug.Log("FINISH!!");

        raceOverCanvas.SetActive(true);
    }

    private void SaveAIRacerParams(List<MechRacer> racersToSave)
    {
        AIEvolutionStats newEvoStats = ScriptableObject.CreateInstance<AIEvolutionStats>();
        newEvoStats.numLaps = totalLaps;
        foreach (MechRacer racer in racersToSave)
        {
            if (!racer.IsHuman)
                newEvoStats.AddParam(racer, Time.time - raceStartTime);
        }

        //Set the iteration number from the evolutionStats currently being used
        int newIterationNum = -1;
        foreach (EvolutionRacerPair pair in evolutionRacerPairs)
        {
            newIterationNum = Mathf.Max(pair.evoStats.iterationNum + 1, newIterationNum);
        }
        newEvoStats.iterationNum = newIterationNum;

#if UNITY_EDITOR
        string filepath = "Assets/_Assets/ScriptableObjects/AIEvolution/iteration" + newIterationNum + ".asset";
        filepath = AssetDatabase.GenerateUniqueAssetPath(filepath);
        AssetDatabase.CreateAsset(newEvoStats, filepath);
        Debug.Log("Saved AI racers in Top 15's params to " + filepath);
#endif

        //SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Sets the UI displaying the local player's position in the race (stuff like 1st, 2nd, etc.)
    /// </summary>
    /// <param name="currPos"> local player's position/rank in the race. </param>
    private void UpdateRacePosUI(int currPos)
    {
        string posSuffix = RacePosSuffix(currPos);

        posNumberText.text = currPos.ToString();
        posSuffixText.text = "\n" + posSuffix;
        numAlivePlayersText.text = (currentRacers.Count + finishedRacers.Count).ToString();
    }

    /// <summary>
    /// Sets the UI displaying the local player's lap number
    /// </summary>
    /// <param name="lapsComplete"> local player's position/rank in the race. </param>
    public void UpdateLapUI(int lapsComplete)
    {
        if (lapsComplete >= totalLaps)
            lapCountText.text = "FINISH!";
        else
            lapCountText.text = "Lap " + (lapsComplete + 1) + "/" + totalLaps;
    }

    /// <summary>
    /// Comparison function used for sorting racers based on how far along the track they are
    /// </summary>
    /// <param name="racer1"></param>
    /// <param name="racer2"></param>
    /// <returns> 1 if racer1 is ahead of racer2, -1 if racer2 is ahead of racer1, 0 if tied.t  </returns>
    private int CompareRacerPositions(MechRacer racer2, MechRacer racer1)
    {
        if (racer1.TrackPos > racer2.TrackPos)
            return 1;
        if (racer1.TrackPos < racer2.TrackPos)
            return -1;
        return 0;
    }

    /// <summary>
    /// Converts a race position int to string suffix, for example 1 becomes "st", 2 becomes "nd", etc.
    /// </summary>
    /// <param name="racePos"> race position as an int. </param>
    /// <returns> race position suffix as a string. </returns>
    public static string RacePosSuffix(int racePos)
    {

        if ((racePos == 1) || ((racePos % 10 == 1) && (racePos > 20)))
            return "st";

        if ((racePos == 2) || ((racePos % 10 == 2) && (racePos > 20)))
            return "nd";

        if ((racePos == 3) || ((racePos % 10 == 3) && (racePos > 20)))
            return "rd";

        return "th";
    }
}
