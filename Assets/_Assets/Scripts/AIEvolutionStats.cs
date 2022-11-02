using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="AIEvolutionStats")]

public class AIEvolutionStats : ScriptableObject
{
    public int iterationNum;
    [SerializeField]private float averageDriftThresh;
    [SerializeField] private float averageNoAccelThresh;

    [SerializeField] public List<ParamStruct> stats = new List<ParamStruct>();

    public void AddParam(float driftThresh, float noAccelThresh)
    {
        stats.Add(new ParamStruct(driftThresh, noAccelThresh));
    }

    public void AddParam(MechRacer racer)
    {
        NPCController npc = racer.GetComponent<NPCController>();
        stats.Add(npc.GetAIParams());
    }

    [System.Serializable]
    public struct ParamStruct
    {
        public float driftThresh;
        public float noAccelThresh;

        public ParamStruct(float _driftThresh, float _noAccelThresh)
        {
            this.driftThresh = _driftThresh;
            this.noAccelThresh = _noAccelThresh;
        }
    }

    public void OnValidate()
    {
        float avgD = 0;
        float avgA = 0;
        foreach(ParamStruct param in stats)
        {
            avgD += param.driftThresh;
            avgA += param.noAccelThresh;
        }

        averageDriftThresh = avgD / stats.Count;
        averageNoAccelThresh = avgA / stats.Count;
    }
}
