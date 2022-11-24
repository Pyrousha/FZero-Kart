using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LobbySettings;

public class VotetypeSelection : MonoBehaviour
{
    [SerializeField] private LobbySettings lobbySettings;
    [SerializeField] private List<LobbyVoteType_Enum> voteTypes;

    public void GamemodeChanged(int _index)
    {
        lobbySettings.OnVoteTypeUpdated(voteTypes[_index]);
    }
}
