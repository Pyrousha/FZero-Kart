using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Controls spawning in new lobby cards, and also starts the race once all players have readied up.
/// </summary>
public class LobbyController : Singleton<LobbyController>
{
    [SerializeField] private GameObject lobbyCardPrefab;
    [SerializeField] private Transform cardParent;

    [SerializeField] private TextMeshProUGUI numPlayersReadyText;

    private PlayerLobbyCard localPlayerLobbyCard;
    private List<PlayerLobbyCard> allLobbyCards = new List<PlayerLobbyCard>();
    private List<PlayerLobbyCard> notReadyCards = new List<PlayerLobbyCard>();

    [SerializeField] private GameObject readyButton;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateCard(MechRacer racer, int posNum)
    {
        PlayerLobbyCard newCard = Instantiate(lobbyCardPrefab).GetComponent<PlayerLobbyCard>();
        if (racer.IsLocalPlayer)
            localPlayerLobbyCard = newCard;

        newCard.transform.parent = cardParent;

        newCard.SetRacerStats(racer, posNum);

        allLobbyCards.Add(newCard);
        notReadyCards.Add(newCard);

        UpdateReadyText();

        if (racer.IsHuman == false)
            OnPlayerReadiedUp(newCard);
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

            UpdateReadyText();

            readyButton.SetActive(false);

            if (notReadyCards.Count == 0)
            {
                //All players are ready, start the race
                StartCoroutine(StartRace());
            }
        }
    }

    private IEnumerator StartRace()
    {
        yield return new WaitForSeconds(1);
        PreRaceInitializer.Instance.InitalizeRacers();
        SceneTransitioner.Instance.LoadRace(4);
        //SceneTransitioner.Instance.StartCup(0);
    }

    /// <summary>
    /// Sets the text displaying how many players have readied up, or says "all players ready"
    /// </summary>
    private void UpdateReadyText()
    {
        if (notReadyCards.Count > 0)
            numPlayersReadyText.text = (allLobbyCards.Count - notReadyCards.Count).ToString() + "/" + allLobbyCards.Count + " players ready";
        else
            numPlayersReadyText.text = "All players ready!";
    }

    /// <summary>
    /// Called when the local player clicks the "ready" button
    /// </summary>
    public void OnClickedReady()
    {
        OnPlayerReadiedUp(localPlayerLobbyCard);
    }
}
