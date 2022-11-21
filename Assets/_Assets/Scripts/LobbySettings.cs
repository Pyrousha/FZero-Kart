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
    private enum LobbyGamemode_Enum
    {
        GrandPrix,
        VSRace,
        TimeTrial,
        BattleRoyale
    }

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

    private LobbyGamemode_Enum gamemode; //what gamemode to play
    private int numRacers; //# of racers to have in the lobby max
    private int numRacesToPlay; //# of tracks to play before showing score screen
    private LobbyVoteType_Enum voteType; //how tracks should be selected
    private bool spawnAI = true;


    void Start()
    {
        //Call these on start so the input fields aren't blank
        //Also initializes #racers and #tracks variables 
        NumRacersSliderUpdated();
        NumTracksSliderUpdated();
    }


    /// <summary>
    /// Called when the player selects a new gamemode type
    /// </summary>
    /// <param name="_newGamemode"> what gamemode was just selected </param>
    public void OnGamemodeUpdated(int _newGamemode)
    {
        gamemode = (LobbyGamemode_Enum)_newGamemode;
    }


    /// <summary>
    /// Called when the numRacers slider has its value updated, 
    /// saves the new value to the variable and also updates the corresponding inputField
    /// </summary>
    public void NumRacersSliderUpdated()
    {
        Debug.Log("S");

        numRacers = Mathf.Clamp((int)numRacers_Slider.value, 2, 99);
        numRacers_InputField.text = numRacers.ToString();
    }

    /// <summary>
    /// Called when the numRacers inputField has its value updated, 
    /// saves the new value to the variable and also updates the corresponding slider
    /// </summary>
    public void NumRacersInputFieldUpdated(string _newValue)
    {
        Debug.Log("I");

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
        Debug.Log("S");

        numRacesToPlay = Mathf.Clamp((int)numRacesToPlay_Slider.value, 1, 64);
        numRacesToPlay_InputField.text = numRacesToPlay.ToString();
    }

    /// <summary>
    /// Called when the numTracks inputField has its value updated, 
    /// saves the new value to the variable and also updates the corresponding slider
    /// </summary>
    public void NumTracksInputFieldUpdated(string _newValue)
    {
        Debug.Log("I");

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
    public void OnVoteTypeUpdated(int _newVoteType)
    {
        voteType = (LobbyVoteType_Enum)_newVoteType;
    }

    public void SetSpawnAI(bool _newSpawnAI)
    {
        spawnAI = _newSpawnAI;
    }

    /// <summary>
    /// Called when the "create lobby" button is pressed.
    /// Sends info about settings to sceneTransitioner and loads into lobby scene
    /// </summary>
    public void CreateLobby()
    {
        RaceTypeEnum raceType = RaceTypeEnum.Story;
        switch (gamemode)
        {
            case LobbyGamemode_Enum.GrandPrix:
                raceType = RaceTypeEnum.GrandPrix;
                break;
            case LobbyGamemode_Enum.VSRace:
                raceType = RaceTypeEnum.CustomVsRace;
                break;
            case LobbyGamemode_Enum.TimeTrial:
                raceType = RaceTypeEnum.TimeTrial;
                break;
            case LobbyGamemode_Enum.BattleRoyale:
                raceType = RaceTypeEnum.BattleRoyale;
                break;
        }

        SceneTransitioner.Instance.SetRaceType(raceType, numRacesToPlay, voteType);
        PreRaceInitializer.NumTotalRacers = numRacers;
        PreRaceInitializer.SpawnAIRacers = spawnAI;
    }
}