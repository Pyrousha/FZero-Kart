using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RaceCourse")]
public class RaceCourse : ScriptableObject
{
    [field: SerializeField] public int BuildIndex { get; private set; }
    [field: SerializeField] public Sprite RaceImage { get; private set; }

    [field: SerializeField] public Color SelectedColor { get; set; }
    [field: SerializeField] public Color BgColor { get; private set; }
}
