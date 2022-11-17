using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static SceneTransitioner;

/// <summary>
/// Controls spawning in new lobby cards, and also starts the race once all players have readied up.
/// </summary>
public class LobbyController : Singleton<LobbyController>
{
    public static MechRacer HostRacer { get; set; }

    [SerializeField] private GameObject lobbyCardPrefab;
    [SerializeField] private Transform cardParent;

    [SerializeField] private TextMeshProUGUI numPlayersReadyText;

    private PlayerLobbyCard localPlayerLobbyCard;
    private List<PlayerLobbyCard> allLobbyCards = new List<PlayerLobbyCard>();
    private List<PlayerLobbyCard> notReadyCards = new List<PlayerLobbyCard>();

    [SerializeField] private NestedMenuCategory readyButton;

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
                        break;
                    }
                case RaceTypeEnum.GrandPrix:
                    {
                        //Show solo cup selection menu
                        solo_cupSelection.SetActive(true);
                        break;
                    }
                case RaceTypeEnum.CustomVsRace:
                    {
                        //Show solo track selection menu
                        solo_raceSelection.SetActive(true);
                        break;
                    }
                case RaceTypeEnum.BattleRoyale:
                    {
                        //Show solo track selection menu
                        solo_raceSelection.SetActive(true);
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
                        break;
                    }
                case RaceTypeEnum.GrandPrix:
                    {
                        //Show multi cup selection menu
                        multi_cupSelection.SetActive(true);
                        break;
                    }
                case RaceTypeEnum.CustomVsRace:
                    {
                        //Show multi track selection menu
                        multi_raceSelection.SetActive(true);
                        break;
                    }
                case RaceTypeEnum.BattleRoyale:
                    {
                        //Show multi track selection menu
                        multi_raceSelection.SetActive(true);
                        break;
                    }
                case RaceTypeEnum.QuickPlay:
                    {
                        //Show multi track selection menu
                        multi_raceSelection.SetActive(true);
                        break;
                    }
            }
        }

        //if (SceneTransitioner.Instance.SingleplayerMode)
        //    gameObject.SetActive(false);
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

        newCard.transform.SetParent(cardParent);

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
        Debug.Log("back to main menu?");
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
        PreRaceInitializer.Instance.InitalizeRacers();
        SceneTransitioner.Instance.StartFirstRace();
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

        readyButton.OnDeactivate(false);
    }
}
