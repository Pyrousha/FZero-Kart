using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="AIEvolutionStats")]

public class AIEvolutionStats : ScriptableObject
{
    public int iterationNum;
    [SerializeField] private float averageDriftThresh;
    [SerializeField] private float averageNoAccelThresh;
    public int numLaps;
    [SerializeField] private float averageRaceTime;

    [SerializeField] public List<ParamStruct> stats = new List<ParamStruct>();

    public void AddParam(float driftThresh, float noAccelThresh, float raceTime)
    {
        stats.Add(new ParamStruct(driftThresh, noAccelThresh, raceTime));
    }

    public void AddParam(MechRacer racer, float raceTime)
    {
        NPCController npc = racer.GetComponent<NPCController>();
        stats.Add(npc.GetAIParams(raceTime));
    }

    [System.Serializable]
    public struct ParamStruct
    {
        public float driftThresh;
        public float noAccelThresh;
        public float raceTime;

        public ParamStruct(float _driftThresh, float _noAccelThresh, float _raceTime)
        {
            driftThresh = _driftThresh;
            noAccelThresh = _noAccelThresh;
            raceTime = _raceTime;
        }
    }

    public void OnValidate()
    {
        float avgD = 0;
        float avgA = 0;
        float avgT = 0;
        foreach(ParamStruct param in stats)
        {
            avgD += param.driftThresh;
            avgA += param.noAccelThresh;
            avgT += param.raceTime;
        }

        averageDriftThresh = avgD / stats.Count;
        averageNoAccelThresh = avgA / stats.Count;
        averageRaceTime = avgT / stats.Count;
    }
}
