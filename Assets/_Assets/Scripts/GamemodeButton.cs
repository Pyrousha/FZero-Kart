using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LobbySettings;

public class GamemodeButton : MonoBehaviour
{
    [field: SerializeField] public bool Singleplayer { get; set; }
    [field: SerializeField] public RaceTypeEnum RaceType { get; set; }
    [field: SerializeField] public bool LoadSceneWhenClicked { get; set; } = true;
}
