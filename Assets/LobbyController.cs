using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls spawning in new lobby cards, and also starts the race once all players have readied up.
/// </summary>
public class LobbyController : MonoBehaviour
{
    private PlayerLobbyCard localPlayerLobbyCard;
    private List<PlayerLobbyCard> allLobbyCards = new List<PlayerLobbyCard>();
    private List<PlayerLobbyCard> notReadyCards = new List<PlayerLobbyCard>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Called when any player readies up, updates visuals and checks if race should start.
    /// </summary>
    /// <param name="player"> Card of player that just readied up </param>
    private void OnPlayerReadiedUp(PlayerLobbyCard player)
    {
        if (player.isReady == false)
        {
            player.ReadyUp();
            notReadyCards.Remove(player);

            if (notReadyCards.Count == 0)
            {

            }
        }
    }

    /// <summary>
    /// Called when the local player clicks the "ready" button
    /// </summary>
    public void OnClickedReady()
    {
        OnPlayerReadiedUp(localPlayerLobbyCard);
    }
}
