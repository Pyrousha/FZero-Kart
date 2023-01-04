using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LobbySettings;

/// <summary>
/// Scene Transitioner used when the player is not connected to a server
/// </summary>
public class OfflineSceneTransitioner : Singleton<OfflineSceneTransitioner>
{
    [SerializeField] private int storySceneIndex;
    [SerializeField] private int lobbySceneIndex;
    [SerializeField] private int scoreSceneIndex;
    [SerializeField] private int mainMenuSceneIndex;
    [SerializeField] private int emptySceneIndex;
    [SerializeField] private bool loadIntoEmptySceneInsteadOfMainMenu;

    void Awake()
    {
        //Set all scene indices for "real" sceneTransitioner
        SceneTransitioner.StorySceneIndex = storySceneIndex;
        SceneTransitioner.LobbySceneIndex = lobbySceneIndex;
        SceneTransitioner.ScoreSceneIndex = scoreSceneIndex;
        SceneTransitioner.MainMenuSceneIndex = mainMenuSceneIndex;
        SceneTransitioner.EmptySceneIndex = emptySceneIndex;
        SceneTransitioner.LoadIntoEmptySceneInsteadOfMainMenu = loadIntoEmptySceneInsteadOfMainMenu;
    }

    public void Start()
    {
        //Load main menu scene
        ToMainMenu();
    }

    /// <summary>
    /// Loads into the Main Menu after initalization
    /// </summary>
    public void ToMainMenu()
    {
        if (loadIntoEmptySceneInsteadOfMainMenu)
        {
            SceneManager.LoadScene(emptySceneIndex);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneIndex);
        }
    }
}