using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SceneTransitioner;

public class MainMenuController : MonoBehaviour
{
    private RaceTypeEnum raceType;

    //private Transform player;

    public void SetRaceType(RaceTypeEnum _raceType)
    {
        raceType = _raceType;

        switch (_raceType)
        {
            case RaceTypeEnum.Story:
                {
                    //Load into story scene
                    break;
                }
            case RaceTypeEnum.BattleRoyale:
                {
                    //Automatically try to join a lobby, or create one
                    break;
                }
            case RaceTypeEnum.QuickPlay:
                {
                    //Automatically try to join a lobby, or create one
                    break;
                }
            default:
                {
                    //Take player to choose singleplayer or multiplayer
                    break;
                }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //player = GameObject.FindObjectOfType<MechRacer>().transform;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Called when the player pressed a gamemode button (such as vsRace, Grand Prix, etc.),
    /// and passes that info to SceneTransitioner.
    /// </summary>
    /// <param name="_gameModeButton"></param>
    public void OnGamemodeButtonPressed(GamemodeButton _gameModeButton)
    {
        SceneTransitioner.Instance.ButtonPressed(_gameModeButton);
    }
}
