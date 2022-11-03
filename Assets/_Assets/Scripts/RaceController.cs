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
    [SerializeField] private bool scoreMode_125Max;

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
    private List<MechRacer> deadRacers = new List<MechRacer>();

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

    private int numTotalRacers;

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
        numTotalRacers = currentRacers.Count;

        foreach (MechRacer racer in currentRacers)
        {
            racer.OnRaceStarted();
        }

        raceStartTime = Time.time;
    }

    public void AddRacer(MechRacer racer)
    {
        if (currentRacers.Contains(racer))
        {
            Debug.LogError("Cannot add duplicate racer to currentRacers: " + racer.name);
        }
        else
        {
            currentRacers.Add(racer);
        }
        CheckAddRacerToScoreList(racer);
        racer.SetNextCheckpoint(checkpoints[0]);
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
            deadRacers.Insert(0, racer);

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

    /// <summary>
    /// called when a racer finishes the race (passes last checkpoint on final lap)
    /// </summary>
    /// <param name="mechRacer"> racer that just finished </param>
    public void OnRacerFinishedFinalLap(MechRacer mechRacer)
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
        if (currentRacers.Count >= 2)
            return;

        //Cut power and end race
        if (currentRacers.Count == 1)
        {
            if (currentRacers[0].IsHuman)
            {
                //Last racer is human, check to see if power should be cut
                if (cutPowerToLastHumanPlayer)
                    currentRacers[0].LastRacerLeft();
                else
                    return; //stop race from ending
            }
            else
            {
                //last racer left is an AI, no need to check to cut power
                currentRacers[0].LastRacerLeft();
            }

            OnRacerFinishedFinalLap(currentRacers[0]);
            return;
        }

        //all racers have finished!
        OnRaceEnd();
    }

    private void OnRaceEnd()
    {
        Debug.Log("FINISH!!");

        raceOverCanvas.SetActive(true);

        List<MechRacer> endingPositions = new List<MechRacer>();
        endingPositions.AddRange(finishedRacers);
        endingPositions.AddRange(deadRacers);

        for (int i = 0; i < endingPositions.Count; i++)
        {
            MechRacer currRacer = endingPositions[i];
            int placement = i + 1;

            if (scoreMode_125Max)
                currRacer.AddPoints(RacePosToScoredPoints_125Max(placement));
            else
                currRacer.AddPoints(RacePosToScoredPoints_1Min(placement));
        }

        SortRacerScores();

        //Print curring ranking to console
        string rankings = "Current Rankings: \n";
        for(int i = 0; i < scoreSortedRacers.Count; i++)
        {
            MechRacer currRacer = scoreSortedRacers[i];
            rankings += ("-"+(i+1)+RacePosSuffix(i+1)+": "+currRacer.Score+"pts: "+currRacer.name+"\n");
        }

        Debug.Log(rankings);
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
    /// <returns> 1 if racer1 is ahead of racer2, -1 if racer2 is ahead of racer1, 0 if tied.  </returns>
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

    #region Score Tracking
    private List<MechRacer> scoreSortedRacers = new List<MechRacer>();

    public void CheckAddRacerToScoreList(MechRacer racer)
    {
        if (scoreSortedRacers.Contains(racer))
        {
            Debug.LogError("scoreSortedRacers already contains racer " + racer.name);
            return;
        }

        scoreSortedRacers.Add(racer);
    }

    public void SortRacerScores()
    {
        scoreSortedRacers.Sort(CompareRacerScores);

        /// <summary>
        /// Comparison function used for sorting racers based on total score
        /// </summary>
        /// <param name="racer1"></param>
        /// <param name="racer2"></param>
        /// <returns> 1 if racer1 has more points than racer2, -1 if racer2 has more points than racer1, 0 if tied.  </returns>
        int CompareRacerScores(MechRacer racer2, MechRacer racer1)
        {
            if (racer1.Score > racer2.Score)
                return 1;
            if (racer1.Score < racer2.Score)
                return -1;
            return 0;
        }
    }
    private int firstPlaceBonus = 3;
    private int secondPlaceBonus = 1;

    /// <summary>
    /// Converts a race position int to number of points earned, where first place gets 125 points.
    /// </summary>
    /// <param name="racePos"> race position as an int. </param>
    /// <returns> points earned for getting position racePos </returns>
    public int RacePosToScoredPoints_125Max(int racePos)
    {
        int points = (126 - firstPlaceBonus) - racePos;

        if (racePos == 1) //125 points
            return points + firstPlaceBonus;
        if (racePos == 2) //122 points
            return points + secondPlaceBonus;
        return points;
    }

    /// <summary>
    /// Converts a race position int to number of points earned, where last place gets 1 point.
    /// </summary>
    /// <param name="racePos"> race position as an int. </param>
    /// <returns> points earned for getting position racePos </returns>
    public int RacePosToScoredPoints_1Min(int racePos)
    {
        int numRacersBeaten = numTotalRacers - racePos;
        int points = numRacersBeaten + 1;

        if (racePos == 1) //125 points
            return points + firstPlaceBonus;
        if (racePos == 2) //122 points
            return points + secondPlaceBonus;
        return points;
    }
    #endregion
}
