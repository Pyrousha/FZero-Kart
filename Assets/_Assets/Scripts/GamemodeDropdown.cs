using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SceneTransitioner;

public class GamemodeDropdown : MonoBehaviour
{
    [SerializeField] private LobbySettings lobbySettings;
    [SerializeField] private List<RaceTypeEnum> gamemodeOptions;

    public void GamemodeChanged(int _index)
    {
        lobbySettings.OnGamemodeUpdated(gamemodeOptions[_index]);
    }
}
