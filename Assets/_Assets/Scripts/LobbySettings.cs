using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SceneTransitioner;

/// <summary>
/// Holds data for what settings the user has specified in the lobby.
/// Such as race type (grand prix, vs race, battle royale, or time trial), # players, etc.
/// </summary>
public class LobbySettings : MonoBehaviour
{
    public enum LobbyVoteType_Enum
    {
        HostPick,
        Vote,
        Random,
        TakeTurns,
        InOrder
    }

    [SerializeField] private Slider numRacers_Slider;
    [SerializeField] private TMP_InputField numRacers_InputField;
    [SerializeField] private Slider numRacesToPlay_Slider;
    [SerializeField] private TMP_InputField numRacesToPlay_InputField;
    [Space(10)]

    [SerializeField] private Selectable gamemode_dropdown;
    [SerializeField] private Toggle aiRacers_toggle;
    [SerializeField] private Selectable votetype_dropdown;


    private static RaceTypeEnum gamemode = RaceTypeEnum.CustomVsRace; //what gamemode to play

    private static int numRacers = 32; //# of racers to have in the lobby max
    public static int NumRacers => numRacers;

    private static bool spawnAI = true;
    public static bool SpawnAI => spawnAI;

    private static int numRacesToPlay = 4; //# of tracks to play before showing score screen

    private static LobbyVoteType_Enum voteType = LobbyVoteType_Enum.HostPick; //how tracks should be selected


    private List<Selectable> selectablesToDisableForGP;


    void Start()
    {
        selectablesToDisableForGP = new List<Selectable> { numRacers_Slider, numRacers_InputField, aiRacers_toggle, numRacesToPlay_Slider, numRacers_InputField };

        //Call these on start so the input fields aren't blank
        //Also initializes #racers and #tracks variables 
        NumRacersSliderUpdated();
        NumTracksSliderUpdated();
    }


    /// <summary>
    /// Called when the player selects a new gamemode type
    /// </summary>
    /// <param name="_newGamemode"> what gamemode was just selected </param>
    public void OnGamemodeUpdated(RaceTypeEnum _newGamemode)
    {
        gamemode = _newGamemode;

        if (gamemode == RaceTypeEnum.GrandPrix)
        {
            //Disable #racers, Ai racers, and #tracks
            foreach (Selectable select in selectablesToDisableForGP)
            {
                select.interactable = false;

                ColorBlock colBlock = select.colors;
                Color col = colBlock.normalColor;
                col.a = 0.5f;
                colBlock.normalColor = col;
                select.colors = colBlock;
            }

            //Link remaining menus together
            SetNavigationDown(gamemode_dropdown, votetype_dropdown);
            SetNavigationUp(votetype_dropdown, gamemode_dropdown);

            //Set values for #racers and such
            numRacers_Slider.value = 32;
            numRacesToPlay_Slider.value = 4;
            aiRacers_toggle.isOn = true;

            NumRacersSliderUpdated();
            NumTracksSliderUpdated();
            spawnAI = true;
        }
        else
        {
            //Enable #racers, Ai racers, and #tracks
            foreach (Selectable select in selectablesToDisableForGP)
            {
                select.interactable = true;

                ColorBlock colBlock = select.colors;
                Color col = colBlock.normalColor;
                col.a = 1.0f;
                colBlock.normalColor = col;
                select.colors = colBlock;
            }

            //Link remaining menus together
            SetNavigationDown(gamemode_dropdown, numRacers_Slider);
            SetNavigationUp(votetype_dropdown, numRacesToPlay_Slider);
        }
    }

    private void SetNavigationUp(Selectable _select, Selectable _target)
    {
        Navigation nav = _select.navigation;
        nav.selectOnUp = _target;
        _select.navigation = nav;
    }

    private void SetNavigationDown(Selectable _select, Selectable _target)
    {
        Navigation nav = _select.navigation;
        nav.selectOnDown = _target;
        _select.navigation = nav;
    }

    /// <summary>
    /// Called when the numRacers slider has its value updated, 
    /// saves the new value to the variable and also updates the corresponding inputField
    /// </summary>
    public void NumRacersSliderUpdated()
    {
        //Debug.Log("S");

        numRacers = Mathf.Clamp((int)numRacers_Slider.value, 2, 99);
        numRacers_InputField.text = numRacers.ToString();
    }

    /// <summary>
    /// Called when the numRacers inputField has its value updated, 
    /// saves the new value to the variable and also updates the corresponding slider
    /// </summary>
    public void NumRacersInputFieldUpdated(string _newValue)
    {
        //Debug.Log("I");

        if (int.TryParse(_newValue, out int parsedNumRacers))
        {
            numRacers = numRacers = Mathf.Clamp(parsedNumRacers, 2, 99);
            numRacers_Slider.value = numRacers;

            numRacers_InputField.text = numRacers.ToString();
        }
    }


    /// <summary>
    /// Called when the numTracks slider has its value updated, 
    /// saves the new value to the variable and also updates the corresponding inputField
    /// </summary>
    public void NumTracksSliderUpdated()
    {
        //Debug.Log("S");

        numRacesToPlay = Mathf.Clamp((int)numRacesToPlay_Slider.value, 1, 64);
        numRacesToPlay_InputField.text = numRacesToPlay.ToString();
    }

    /// <summary>
    /// Called when the numTracks inputField has its value updated, 
    /// saves the new value to the variable and also updates the corresponding slider
    /// </summary>
    public void NumTracksInputFieldUpdated(string _newValue)
    {
        //Debug.Log("I");

        if (int.TryParse(_newValue, out int parsedNumTracks))
        {
            numRacesToPlay = Mathf.Clamp(parsedNumTracks, 1, 64);
            numRacesToPlay_Slider.value = numRacesToPlay;

            numRacesToPlay_InputField.text = numRacesToPlay.ToString();
        }
    }

    /// <summary>
    /// Called when the player selects a new vote options type
    /// </summary>
    /// <param name="_newVoteType"> what voteType was just selected </param>
    public void OnVoteTypeUpdated(LobbyVoteType_Enum _newVoteType)
    {
        voteType = _newVoteType;
    }

    public void SetSpawnAI(bool _newSpawnAI)
    {
        spawnAI = _newSpawnAI;
    }

    /// <summary>
    /// Called when the "create lobby" button is pressed.
    /// Sends info about settings to sceneTransitioner and loads into lobby scene
    /// </summary>
    public void CreateLobby(bool _singleplayer)
    {
        if (_singleplayer)
        {
            gamemode = RaceTypeEnum.CustomVsRace;
        }

        //Set race parameters
        SceneTransitioner.Instance.SingleplayerMode = _singleplayer;
        SceneTransitioner.Instance.SetRaceType(gamemode, numRacesToPlay, voteType);

        //Tell scenetransitioner the local player is host
        SceneTransitioner.Instance.IsLocalPlayerHost = true;

        //Start a server (spawns a player, sets local player var in SceneTransitioner, and loads to the lobby scene)
        NetworkManagerFZeroKart.MultiServer.StartHost();
    }
}