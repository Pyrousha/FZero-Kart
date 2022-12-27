using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RaceCup")]
public class RaceCup : ScriptableObject
{
    [field: SerializeField] public List<RaceCourse> Courses { get; private set; }
    //public List<int> RaceBuildIndices { get; private set; }
    [field: SerializeField] public Sprite CupSprite { get; private set; }

    [field: SerializeField] public Color BgColor { get; private set; }
    [field: SerializeField] public Color SelectedColor { get; private set; }
    [Space(5)]
    [SerializeField] private Sprite goldTrophy;
    [SerializeField] private Sprite silverTrophy;
    [SerializeField] private Sprite bronzeTrophy;
    [SerializeField] private Sprite emptyTrophy;

    [field: SerializeField] public int BestPlacement { get; private set; }

    /// <summary>
    /// Called when a cup is finished. Saves the local player's race position.
    /// TODO: save best placement to a file 
    /// </summary>
    /// <param name="localPlayerPos"> Position ranking of the local player </param>
    public void OnCupFinished(int localPlayerPos)
    {
        Debug.Log("Setting new best placement for " + name);
        BestPlacement = Mathf.Min(localPlayerPos, BestPlacement);
        Debug.Log("New placement: " + BestPlacement);
    }

    public Sprite GetTrophySprite()
    {
        if (BestPlacement == 1)
            return goldTrophy;

        if (BestPlacement == 2)
            return silverTrophy;

        if (BestPlacement == 3)
            return bronzeTrophy;

        return emptyTrophy;
    }
}
