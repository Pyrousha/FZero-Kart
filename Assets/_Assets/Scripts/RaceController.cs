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

    [Header("References")]
    public Material activeCheckpointMaterial;
    public Material inactiveCheckpointMaterial;
    [SerializeField] private TextMeshProUGUI posNumberText;
    [SerializeField] private TextMeshProUGUI posSuffixText;
    [SerializeField] private TextMeshProUGUI lapCountText;

    public Checkpoint endCheckpoint { get; private set; }
    private List<MechRacer> finishedRacers = new List<MechRacer>();
    private List<MechRacer> currentRacers = new List<MechRacer>();

    [Header("Track Specific References/Settings")]
    [SerializeField] private int totalLaps;
    [SerializeField] private Transform checkpointParent;
    private List<Checkpoint> checkpoints;
    public int TotalLaps => totalLaps;

    [Header("AI Evolution Funsies")]
    [SerializeField] private EvolutionRacerPair racerPair;
    [SerializeField] private List<EvolutionRacerPair> evolutionRacerPairs;

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
            if (racer.playerController != null)
                racer.playerController.canMove = true;

            if (racer.npcController != null)
                racer.npcController.canMove = true;
        }
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

    private bool savedParams = false;

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

        if (currentRacers.Count == 0)
        {
            //all racers have finished!
            Debug.Log("FINISH!!");
        }

#if UNITY_EDITOR
        //AI param saving
        if ((finishedRacers.Count >= 15) && (savedParams == false))
        {
            SaveAIRacerParams(finishedRacers);

            savedParams = true;
#endif
        }
    }

    private void SaveAIRacerParams(List<MechRacer> racersToSave)
    {
        AIEvolutionStats newEvoStats = ScriptableObject.CreateInstance<AIEvolutionStats>();
        foreach (MechRacer racer in racersToSave)
        {
            newEvoStats.AddParam(racer);
        }

        int newIterationNum = -1;
        foreach (EvolutionRacerPair pair in evolutionRacerPairs)
        {
            newIterationNum = Mathf.Max(pair.evoStats.iterationNum + 1, newIterationNum);
        }
        newEvoStats.iterationNum = newIterationNum;

        string filepath = AssetDatabase.GenerateUniqueAssetPath("Assets/_Assets/Prefabs/AIEvolution/iteration" + newEvoStats.iterationNum + ".asset");
        AssetDatabase.CreateAsset(newEvoStats, filepath);
        Debug.Log("Saved Top 15 AI racer params to iteration" + newEvoStats.iterationNum);

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
