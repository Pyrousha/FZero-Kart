using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static SceneTransitioner;
using Mirror;

/// <summary>
/// Controls spawning in new lobby cards, and also starts the race once all players have readied up.
/// </summary>
public class LobbyController : NetworkBehaviour
{
    public static MechRacer HostRacer { get; set; }

    [SerializeField] private GameObject lobbyCardPrefab;
    [SerializeField] private Transform cardParent;

    [SerializeField] private TextMeshProUGUI numPlayersReadyText;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI raceNumber_Text;

    private PlayerLobbyCard localPlayerLobbyCard;
    private List<PlayerLobbyCard> allLobbyCards = new List<PlayerLobbyCard>();
    private List<PlayerLobbyCard> notReadyCards = new List<PlayerLobbyCard>();

    [Header("Gamemode-Specific References")]
    [SerializeField] private GameObject lobbyList;
    [SerializeField] private NestedMenuCategory startRaceButton;
    [SerializeField] private GameObject raceTimePopup;
    [Space(5)]
    [SerializeField] private GameObject solo_cupSelection;
    [SerializeField] private GameObject multi_cupSelection;
    [Space(5)]
    [SerializeField] private GameObject solo_raceSelection;
    [SerializeField] private GameObject multi_raceSelection;
    [Space(10)]
    [SerializeField] private NestedMenuCategory readyButton_cup;
    [SerializeField] private NestedMenuCategory readyButton_track;


    private NestedMenuCategory readyButton;

    #region Singleton stuff
    private static LobbyController instance = null;

    void Awake()
    {
        if ((Instance != null) && (Instance != this))
        {
            Debug.Log("Destroyed script type" + typeof(LobbyController) + " on gameObject" + gameObject.name);
            Destroy(gameObject);
        }
    }

    public static LobbyController Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<LobbyController>();
            return instance;
        }
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        if (SceneTransitioner.Instance.SingleplayerMode)
        {
            switch (SceneTransitioner.Instance.RaceType)
            {
                case RaceTypeEnum.Story:
                    {
                        Debug.LogError("Lobby scene should not be loaded for story mode");
                        break;
                    }
                case RaceTypeEnum.TimeTrial:
                    {
                        //Show solo track selection menu
                        solo_raceSelection.SetActive(true);
                        readyButton = readyButton_track;
                        break;
                    }
                case RaceTypeEnum.GrandPrix:
                    {
                        //Show solo cup selection menu
                        solo_cupSelection.SetActive(true);
                        readyButton = readyButton_cup;
                        break;
                    }
                case RaceTypeEnum.CustomVsRace:
                    {
                        //Show solo track selection menu
                        solo_raceSelection.SetActive(true);
                        readyButton = readyButton_track;
                        break;
                    }
                case RaceTypeEnum.BattleRoyale:
                    {
                        //Show solo track selection menu
                        solo_raceSelection.SetActive(true);
                        readyButton = readyButton_track;
                        break;
                    }
            }
        }
        else
        {
            lobbyList.SetActive(true);

            switch (SceneTransitioner.Instance.RaceType)
            {
                case RaceTypeEnum.TimeTrial:
                    {
                        //Show multi track selection menu
                        multi_raceSelection.SetActive(true);
                        readyButton = readyButton_track;
                        break;
                    }
                case RaceTypeEnum.GrandPrix:
                    {
                        //Show multi cup selection menu
                        multi_cupSelection.SetActive(true);
                        readyButton = readyButton_cup;
                        break;
                    }
                case RaceTypeEnum.CustomVsRace:
                    {
                        //Show multi track selection menu
                        multi_raceSelection.SetActive(true);
                        readyButton = readyButton_track;
                        break;
                    }
                case RaceTypeEnum.BattleRoyale:
                    {
                        //Show multi track selection menu
                        multi_raceSelection.SetActive(true);
                        readyButton = readyButton_track;
                        break;
                    }
                case RaceTypeEnum.QuickPlay:
                    {
                        //Show multi track selection menu
                        multi_raceSelection.SetActive(true);
                        readyButton = readyButton_track;
                        break;
                    }
            }
        }

        raceNumber_Text.text = $"Race {SceneTransitioner.Instance.NumRacesCompleted + 1} / {SceneTransitioner.Instance.NumRacesToPlay}";
    }


    /// <summary>
    /// Checks if there is already a card for this player, and if not creates one (client only)
    /// </summary>
    /// <param name="racer"></param>
    /// <param name="posNum"></param>
    [Client]
    public void TryCreateCard(MechRacer racer, int posNum)
    {
        //Check to make sure a card for this player doesn't already exist
        foreach (PlayerLobbyCard card in allLobbyCards)
        {
            if (card.Racer == racer)
                return;
        }

        Debug.Log($"creating card for player {racer.gameObject.name}");

        //create a new card for this player
        PlayerLobbyCard newCard = Instantiate(lobbyCardPrefab).GetComponent<PlayerLobbyCard>();
        Debug.Log($"player: {racer.gameObject.name}");
        if (racer.IsLocalPlayer)
        {
            localPlayerLobbyCard = newCard;
            Debug.Log("setting localPlayerCard");
        }
        else
            Debug.Log("not local player, do not set localPlayerCard");

        newCard.transform.SetParent(cardParent);

        newCard.SetRacerStats(racer, posNum);

        allLobbyCards.Add(newCard);
        notReadyCards.Add(newCard);

        UpdateReadyText();
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

            if (notReadyCards.Count == 0)
            {
                //All players are ready, show start race button to host
                if (HostRacer.IsLocalPlayer)
                    startRaceButton.OnActivate();
            }
        }
    }

    /// <summary>
    /// Called when player presses back button, loads back to main menu
    /// </summary>
    public void BackToMainMenu()
    {
        SceneTransitioner.Instance.ToMainMenu();
    }

    /// <summary>
    /// Called when the host clicks "start race" (means all players are ready)
    /// Play the "race time" popup anim and then load into the race
    /// </summary>
    public void OnClickedStartRace()
    {
        raceTimePopup.SetActive(true);
    }

    /// <summary>
    /// Called when raceTime anim ends, loads into race
    /// </summary>
    /// <returns></returns>
    public void StartRace()
    {
        //PreRaceInitializer.Instance.InitalizeRacers();
        SceneTransitioner.Instance.StartRace();
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
    [Client]
    public void OnClickedReady()
    {
        OnPlayerReadiedUp(localPlayerLobbyCard);

        readyButton.OnDeactivate(false);
    }

    /// <summary>
    /// Sets all AI racer cards to "ready"
    /// </summary>
    public void ReadyUpAIRacers()
    {
        for (int i = 0; i < notReadyCards.Count; i++)
        {
            PlayerLobbyCard currCard = notReadyCards[i];
            if (currCard.Racer.IsHuman == false)
            {
                OnPlayerReadiedUp(currCard);
                i--;
            }
        }
    }
}
