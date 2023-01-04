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
    private bool isRaceOver;

    [System.Serializable]
    public struct EvolutionRacerPair
    {
        public Transform NPCParent;
        public AIEvolutionStats evoStats;
    }

    [SerializeField] private bool cutPowerToLastHumanPlayer; //If the last player is a human, should the race end prematurely or let them finish?
    [SerializeField] private bool endRaceOnceAllHumansFinish = true;
    [Space(5)]
    [SerializeField] private bool scoreMode_125Max;
    private float maxScoreFillDuration = 1.5f; //How many seconds does it take to add points from 0 -> (points earned by 1st place)

    [SerializeField] private Color firstPlaceColor;
    [SerializeField] private Color secondPlaceColor;
    [SerializeField] private Color thirdPlaceColor;
    [SerializeField] private Color fourthPlaceColor;
    [SerializeField] private Color lastPlaceColor;

    private static Color static_firstPlaceColor;
    private static Color static_secondPlaceColor;
    private static Color static_thirdPlaceColor;
    private static Color static_fourthPlaceColor;
    private static Color static_lastPlaceColor;

    [Header("References")]
    private Animator racePosCanvasAnim;
    [SerializeField] private Animator countdownAnim;
    [SerializeField] private RacerScoreDisplay playerScoreDisplay;
    [SerializeField] private Transform scoreDisplayParent;
    [SerializeField] private GameObject scoreDisplayPrefab;
    [Space(5)]
    public Material activeCheckpointMaterial;
    public Material inactiveCheckpointMaterial;
    [Space(5)]
    [SerializeField] private TextMeshProUGUI posNumberText;
    [SerializeField] private TextMeshProUGUI posSuffixText;
    [SerializeField] private TextMeshProUGUI numAlivePlayersText;
    [SerializeField] private TextMeshProUGUI lapCountText;
    [Space(5)]
    [SerializeField] private GameObject raceOverCanvas;

    public Checkpoint endCheckpoint { get; private set; }
    private MechRacer localPlayerMech;
    private List<MechRacer> finishedRacers = new List<MechRacer>();
    private List<MechRacer> currentRacers = new List<MechRacer>();
    private List<MechRacer> deadRacers = new List<MechRacer>();

    [Header("Track Specific References/Settings")]
    [SerializeField] private int totalLaps;
    [SerializeField] private Transform checkpointParent;
    [Space(5)]
    [SerializeField] private GameObject SpawnIndicator;
    //[SerializeField] private List<SpawnLine> spawnLines;

    [SerializeField] private Transform spawnLineParent;
    [SerializeField] private int numRacersPerLine;
    private List<Checkpoint> checkpoints;
    public int TotalLaps => totalLaps;
    [SerializeField] private LayerMask groundLayerForSpawning;

    [Header("AI Evolution Funsies")]
    [SerializeField] private bool saveAIRacers;
    [SerializeField] private string nameToSaveAIParametersTo;
    //[SerializeField] private EvolutionRacerPair racerPair; //NPC racers that will be in the actual game, referenced to set values from evolution stats
    //[SerializeField] private List<EvolutionRacerPair> evolutionRacerPairs;

    //[SerializeField] private AIEvolutionStats

    #region Spawnpoint positioning
    // [System.Serializable]
    // private struct SpawnLine
    // {
    //     public Transform startPosition;
    //     //public Transform endPosition;
    //     public int numberOfRacersOnLine;
    // }

    private List<Vector3> racerSpacePositions = new List<Vector3>();
    #endregion

    private float raceStartTime;
    private bool savedParams = false;

    private int numTotalRacers;

    public void Start()
    {
        #region Set static parameters
        static_firstPlaceColor = firstPlaceColor;
        static_secondPlaceColor = secondPlaceColor;
        static_thirdPlaceColor = thirdPlaceColor;
        static_fourthPlaceColor = fourthPlaceColor;
        static_lastPlaceColor = lastPlaceColor;
        #endregion


        racePosCanvasAnim = posNumberText.transform.parent.GetComponent<Animator>();


        #region Initialize checkpoints
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
        #endregion

        //Get list of current racers
        currentRacers = new List<MechRacer>(RacerStandingsTracker.ExistingRacerStandings);
        numTotalRacers = currentRacers.Count;

        scoreSortedRacers = new List<MechRacer>(currentRacers);

        //Find current racer, as well as set map-specific parameters to racers (ex: first checkpoint to hit)
        foreach (MechRacer racer in currentRacers)
        {
            if (racer.IsLocalPlayer)
                localPlayerMech = racer;

            racer.SetNextCheckpoint(checkpoints[0]);
        }


        #region Place racers on track based on standings
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
                racerSpacePositions.Add(pos);

                RaycastHit _hit;
                if (Physics.Raycast(pos, -transform.up, out _hit, 50, groundLayerForSpawning))
                {
                    Instantiate(SpawnIndicator, _hit.point, Quaternion.LookRotation(transform.forward, _hit.normal));
                }
            }
        }

        //PrintRankings(currentRacers);

        for (int i = 0; i < currentRacers.Count; i++)
        {
            currentRacers[i].transform.position = racerSpacePositions[numTotalRacers - 1 - i];
            currentRacers[i].transform.forward = spawnLineParent.forward;
            currentRacers[i].OnRaceFinishedLoading();
        }
        #endregion

        UpdateLapUI(0);

        //TEMP: Check all players have connected before calling
        Invoke("StartCountdown", 1f);
    }

    //Assume currentRacers have all connected and are initialized
    private void StartCountdown()
    {
        countdownAnim.enabled = true;

        countdownAnim.GetComponentInChildren<AudioClipPlayer>().PlayClipWithIndex(0);

        Invoke("StartRace", 3.5f);
    }

    public void StartRace()
    {
        foreach (MechRacer racer in currentRacers)
        {
            racer.OnRaceStarted();
        }

        raceStartTime = Time.time;
    }

    // public void AddRacer(MechRacer racer)
    // {
    //     if (currentRacers.Contains(racer))
    //     {
    //         Debug.LogError("Cannot add duplicate racer to currentRacers: " + racer.name);
    //     }
    //     else
    //     {
    //         currentRacers.Add(racer);
    //         if (racer.IsLocalPlayer)
    //             localPlayerMech = racer;
    //     }
    //     CheckAddRacerToScoreList(racer);
    //     racer.SetNextCheckpoint(checkpoints[0]);

    //     numTotalRacers = currentRacers.Count;
    // }

    public void DestroyRacer(MechRacer racer)
    {
        if (finishedRacers.Contains(racer))
        {
            Debug.LogError("Cannot destroy finished racer: " + racer.name);
            return;
        }

        if (currentRacers.Remove(racer))
        {
            //Debug.Log("Racer " + racer.name + " has been disqualified!");
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
                StartCoroutine(UpdateRacePosUI(i + 1 + finishedRacers.Count));
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
        {
            StartCoroutine(UpdateRacePosUI(finishedRacers.Count));
            raceOverCanvas.SetActive(true);
        }

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
        if (isRaceOver)
            return;

        if (endRaceOnceAllHumansFinish)
        {
            bool allAIRacers = true;
            for (int i = 0; i < currentRacers.Count; i++)
            {
                if (currentRacers[i].IsHuman)
                {
                    allAIRacers = false;
                    break;
                }
            }

            if (allAIRacers) //end race now
            {
                while (currentRacers.Count > 0)
                {
                    MechRacer mechRacer = currentRacers[0];
                    currentRacers.RemoveAt(0);
                    finishedRacers.Add(mechRacer);
                    mechRacer.AIFinishRaceEarly();
                }
            }
        }

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
        isRaceOver = true;

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

        SortRacerScores(scoreSortedRacers);

        StartCoroutine(SpawnRacerScorecards(endingPositions, scoreSortedRacers));

        //PrintRankings(scoreSortedRacers);
    }

    public static void PrintRankings(List<MechRacer> _racers)
    {
        //Print curring ranking to console
        string rankings = "New Rankings: \n";
        for (int i = 0; i < _racers.Count; i++)
        {
            MechRacer currRacer = _racers[i];
            rankings += "-" + (i + 1) + RacePosSuffix(i + 1) + ": " + currRacer.Score + "pts: " + currRacer.name + "\n";
        }

        Debug.Log(rankings);
    }

    /// <summary>
    /// Spawns racer scorecards for the current race
    /// Then, fades the scorecards out
    /// Sets data/sorts racers based on previous standings (ranking when race started)
    /// Next, lerps position of scorecards to the new ranking, and start countup
    /// -When lerp is done, update ranking graphic/suffix
    /// <summary>
    private IEnumerator SpawnRacerScorecards(List<MechRacer> thisRacePlacements, List<MechRacer> newOverallPlacements)
    {
        List<MechRacer> previousOverallPlacements = new List<MechRacer>(RacerStandingsTracker.ExistingRacerStandings);
        if (SceneTransitioner.Instance.IsFirstRace)
            previousOverallPlacements.Reverse();

        raceOverCanvas.GetComponent<Animator>().SetTrigger("ShowCurr");

        #region Current race scorecards
        int pointsEarnedByFirstPlace;
        if (scoreMode_125Max)
            pointsEarnedByFirstPlace = RacePosToScoredPoints_125Max(1);
        else
            pointsEarnedByFirstPlace = RacePosToScoredPoints_1Min(1);
        float pointsToAddPerSec = pointsEarnedByFirstPlace / maxScoreFillDuration;

        List<RacerScoreDisplay> racerScoreDisplays = new List<RacerScoreDisplay>();

        //int numRacersToDisplay = Mathf.Min(endingPositions.Count, 15);
        int numRacersToDisplay = thisRacePlacements.Count;
        for (int i = 0; i < numRacersToDisplay; i++)
        {
            //Create Score prefab instance
            RacerScoreDisplay scoreDisplay = Instantiate(scoreDisplayPrefab, Vector3.zero, Quaternion.identity, scoreDisplayParent).GetComponent<RacerScoreDisplay>();
            racerScoreDisplays.Add(scoreDisplay);

            //Fill data from racers based on position
            MechRacer racer = thisRacePlacements[i];
            int posNum = i + 1;
            scoreDisplay.SetData(true, posNum, -1, -1, racer, pointsToAddPerSec);

            yield return new WaitForSeconds(0.1f / (numRacersToDisplay + 1));
        }

        //Set local player score
        playerScoreDisplay.gameObject.SetActive(true);
        playerScoreDisplay.SetData(true, thisRacePlacements.IndexOf(localPlayerMech) + 1, -1, -1, localPlayerMech, pointsToAddPerSec);
        //racerScoreDisplays.Add(playerScoreDisplay);
        #endregion


        yield return new WaitForSeconds(3f);

        raceOverCanvas.GetComponent<Animator>().SetTrigger("ShowOverall");
        #region Change scorecards to be based on previous overall score
        //Disable all scoredisplays
        foreach (RacerScoreDisplay scoreDisplay in racerScoreDisplays)
        {
            scoreDisplay.FlipShut();
            yield return new WaitForSeconds(0.05f / (numRacersToDisplay + 1));
        }
        playerScoreDisplay.FlipShut();

        yield return new WaitForSeconds(0.5f);

        //Update scorecards based on previous overall placement
        for (int i = 0; i < previousOverallPlacements.Count; i++)
        {
            //Get Score prefab instance
            RacerScoreDisplay scoreDisplay = racerScoreDisplays[i];

            //Fill data from racers based on position
            MechRacer racer = previousOverallPlacements[i];
            int prevPosNum = GetPreviousSharedPosition(i, racer, previousOverallPlacements);
            int newPosIndex = newOverallPlacements.IndexOf(racer);
            int newPosNum = GetCurrentSharedPosition(newPosIndex, racer, newOverallPlacements);

            int firstRacePlace = 0;
            if (SceneTransitioner.Instance.IsFirstRace)
            {
                firstRacePlace = numTotalRacers;
                prevPosNum = numTotalRacers;
            }
            scoreDisplay.SetData(false, prevPosNum, newPosNum, newPosIndex, racer, pointsToAddPerSec, firstRacePlace);

            scoreDisplay.FlipOpen();

            yield return new WaitForSeconds(0.1f / (numRacersToDisplay + 1));
        }

        //Set player scoreCard
        int playerPrevPosNum = GetPreviousSharedPosition(previousOverallPlacements.IndexOf(localPlayerMech), localPlayerMech, previousOverallPlacements);
        int playerNewPosNum = GetCurrentSharedPosition(newOverallPlacements.IndexOf(localPlayerMech), localPlayerMech, newOverallPlacements);

        int playerFirstRacePlace = 0;
        if (SceneTransitioner.Instance.IsFirstRace)
        {
            playerFirstRacePlace = numTotalRacers;
            playerPrevPosNum = numTotalRacers;
        }

        playerScoreDisplay.SetData(false, playerPrevPosNum, playerNewPosNum, -1, localPlayerMech, pointsToAddPerSec, playerFirstRacePlace);

        playerScoreDisplay.FlipOpen();
        #endregion


        yield return new WaitForSeconds(1f);


        #region Lerp all scorecards and count up score
        float lerpDuration = maxScoreFillDuration; //Seconds it takes for a scorecard to arrive at its new location
        RacerScoreDisplay.scoreDisplays = racerScoreDisplays; //Gives all scoredisplays reference to the list of indexed scoreDisplays
        foreach (RacerScoreDisplay scoreDisplay in racerScoreDisplays)
        {
            scoreDisplay.StartLerp(lerpDuration);
        }
        playerScoreDisplay.StartLerp(lerpDuration, true);
        #endregion


        yield return new WaitForSeconds(4f + maxScoreFillDuration);


        //Save new racer standing to persistent data
        RacerStandingsTracker.Instance.UpdateRacerStandings(scoreSortedRacers);


        Debug.Log("Time for the next race!");
        foreach (MechRacer racer in thisRacePlacements)
        {
            racer.OnNewRaceLoading_Server();
        }

        SceneTransitioner.Instance.OnRaceFinished();
    }


    /// <summary>
    /// Gets the overall position of a racer, by getting the best (lowest number) position of the racer with the same score
    /// </summary>
    /// <param name="currRacerIndex"> index of the given racer in sortedRacers array </param>
    /// <param name="racer"> racer to give a placement to </param>
    /// <param name="sortedRacers"> list of sorted racers, where racer at 0 is in first </param>
    /// <returns> position ranking for the racer (1 for first, 2 for second, etc.) </returns>
    public static int GetCurrentSharedPosition(int currRacerIndex, MechRacer racer, List<MechRacer> sortedRacers)
    {
        int currIndex = currRacerIndex;
        while (currIndex > 0)
        {
            MechRacer prevRacer = sortedRacers[currIndex - 1];
            if (prevRacer.Score == racer.Score)
            {
                currIndex--;
            }
            else
            {
                break;
            }
        }

        return currIndex + 1;
    }

    /// <summary>
    /// Gets the overall position of a racer, by getting the best (lowest number) position of the racer with the same previous score
    /// <summary>
    public static int GetPreviousSharedPosition(int currRacerIndex, MechRacer racer, List<MechRacer> sortedRacers)
    {
        int currIndex = currRacerIndex;
        while (currIndex > 0)
        {
            MechRacer prevRacer = sortedRacers[currIndex - 1];
            if (prevRacer.LastScore == racer.LastScore)
            {
                currIndex--;
            }
            else
            {
                break;
            }
        }

        return currIndex + 1;
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

#if UNITY_EDITOR
        string filepath = "Assets/_Assets/ScriptableObjects/AIEvolution/" + nameToSaveAIParametersTo + ".asset";
        filepath = AssetDatabase.GenerateUniqueAssetPath(filepath);
        AssetDatabase.CreateAsset(newEvoStats, filepath);
        Debug.Log("Saved AI racers in Top 15's params to " + filepath);
#endif

        //SceneManager.LoadScene(0);
    }

    private int lastRacePos = -1;

    /// <summary>
    /// Sets the UI displaying the local player's position in the race (stuff like 1st, 2nd, etc.)
    /// </summary>
    /// <param name="currPos"> local player's position/rank in the race. </param>
    private IEnumerator UpdateRacePosUI(int currPos)
    {
        if (currPos == lastRacePos)
            yield break;

        lastRacePos = currPos;

        racePosCanvasAnim.SetTrigger("PosChange");
        yield return new WaitForSecondsRealtime(1f / 12f);

        string posSuffix = RacePosSuffix(currPos);

        posNumberText.text = currPos.ToString();
        posSuffixText.text = "\n" + posSuffix;
        numAlivePlayersText.text = (currentRacers.Count + finishedRacers.Count).ToString();


        Color posColor = GetColorForPos(currPos);
        //Debug.Log("position: " + currPos + ", color:" + posColor);
        posNumberText.color = posColor;
        posSuffixText.color = posColor;
    }

    /// <summary>
    /// Gets color to use for position UI based on current placement
    /// </summary>
    /// <param name="currPos"> position/rank in the race. </param>
    /// <returns> Color based on race position. </returns>
    public Color GetColorForPos(int currPos)
    {
        if (currPos == 1)
            return firstPlaceColor;

        if (currPos == 2)
            return secondPlaceColor;

        if (currPos == 3)
            return thirdPlaceColor;

        //4th or below
        float posRatio = (currPos - 4.0f) / (numTotalRacers - 4.0f);
        return Color.Lerp(fourthPlaceColor, lastPlaceColor, posRatio);
    }

    public static Color Static_GetColorForPos(int currPos, int numRacers)
    {
        if (currPos == 1)
            return static_firstPlaceColor;

        if (currPos == 2)
            return static_secondPlaceColor;

        if (currPos == 3)
            return static_thirdPlaceColor;

        //4th or below
        float posRatio = (currPos - 4.0f) / (numRacers - 4.0f);
        return Color.Lerp(static_fourthPlaceColor, static_lastPlaceColor, posRatio);
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
    private List<MechRacer> scoreSortedRacers;

    // public void CheckAddRacerToScoreList(MechRacer racer)
    // {
    //     if (scoreSortedRacers.Contains(racer))
    //     {
    //         Debug.LogError("scoreSortedRacers already contains racer " + racer.name);
    //         return;
    //     }

    //     scoreSortedRacers.Add(racer);
    // }


    /// <summary>
    /// Take a list of mechRacers and sorts it based on current racer scores, with highest score being element 0.
    /// </summary>
    /// <param name="racersToSort"> List of mechRacers to sort based on current scores. </param>
    public static void SortRacerScores(List<MechRacer> racersToSort)
    {
        racersToSort.Sort(CompareRacerScores);

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
