using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScene : MonoBehaviour
{
    // References to the UI GameObjects
    public GameObject mainMenuUI;
    public GameObject selectStageUI;
    public GameObject gameControlUI;

    // Scene names for stages
    public string stage1SceneName;
    public string stage2SceneName;

    void Start()
    {
        // Ensure references are set
        if (!mainMenuUI || !selectStageUI || !gameControlUI ||
            string.IsNullOrEmpty(stage1SceneName) || string.IsNullOrEmpty(stage2SceneName))
        {
            Debug.LogError("One or more UI elements or scene names are not assigned in the inspector.");
            return;
        }

        // Initial setup: show main menu, hide others
        mainMenuUI.SetActive(true);
        selectStageUI.SetActive(false);
        gameControlUI.SetActive(false);
    }

    public void OnPlayButtonClicked()
    {
        Debug.Log("Play button clicked");
        // Hide main menu and show select stage UI
        mainMenuUI.SetActive(false);
        selectStageUI.SetActive(true);
    }

    public void OnGameControlButtonClicked()
    {
        Debug.Log("Game control button clicked");
        // Hide main menu and show game control UI
        mainMenuUI.SetActive(false);
        gameControlUI.SetActive(true);
    }

    public void OnBackButtonClicked()
    {
        Debug.Log("Back button clicked");
        // Hide game control UI and show main menu
        gameControlUI.SetActive(false);
        mainMenuUI.SetActive(true);
    }

    public void OnGameControlPlayButtonClicked()
    {
        Debug.Log("Game control play button clicked");
        // Hide game control UI and show select stage UI
        gameControlUI.SetActive(false);
        selectStageUI.SetActive(true);
    }

    public void OnStageButtonClicked(string sceneName)
    {
        Debug.Log("Stage button clicked: " + sceneName);
        // Load the corresponding stage scene
        SceneManager.LoadScene(sceneName);
    }
}

