using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SceneTransitioner;

public class GamemodeButton : MonoBehaviour
{
    [field: SerializeField] public bool Singleplayer { get; set; }
    [field: SerializeField] public RaceTypeEnum RaceType { get; set; }
}
