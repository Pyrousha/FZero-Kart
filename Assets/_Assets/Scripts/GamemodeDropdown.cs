using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LobbySettings;

public class GamemodeDropdown : MonoBehaviour
{
    [SerializeField] private LobbySettings lobbySettings;
    [SerializeField] private List<RaceTypeEnum> gamemodeOptions;

    public void GamemodeChanged(int _index)
    {
        lobbySettings.OnGamemodeUpdated(gamemodeOptions[_index]);
    }
}
