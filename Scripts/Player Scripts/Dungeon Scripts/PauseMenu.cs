using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    private GameObject pauseMenu;
    private GameManager gameManager;
    private Button exitMenu;

    private bool gamePaused;

    public void SetUp(GameManager gameManager)
    {
        this.gameManager = gameManager;
        pauseMenu = GameObject.Find("Pause Menu");
        exitMenu = GameObject.Find("Exit Menu").GetComponent<Button>();
        exitMenu.onClick.AddListener(SetUpExitMenuListener);
        pauseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !gamePaused)
        {
            gamePaused = true;
            pauseMenu.SetActive(true);
            gameManager.DeactivateAllTownEntrances();
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && gamePaused)
        {
            gamePaused = false;
            pauseMenu.SetActive(false);
            gameManager.ReactivateAllTownEntrances();
        }
    }

    private void SetUpExitMenuListener()
    {
        gamePaused = false;
        pauseMenu.SetActive(false);
        gameManager.ReactivateAllTownEntrances();
    }
}
