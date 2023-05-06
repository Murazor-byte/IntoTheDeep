using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class NewGame : MonoBehaviour
{
    public void CreateNewGame()
    {
        //prevent other menu buttons to be selected when loading
        gameObject.GetComponent<Button>().interactable = false;
        GameObject.Find("Load Game").GetComponent<Button>().interactable = false;
        StartCoroutine(StartNewGame());
    }

    private IEnumerator StartNewGame()
    {
        AsyncOperation loaded = SceneManager.LoadSceneAsync("Town", LoadSceneMode.Additive);
        loaded.allowSceneActivation = false;

        while (!loaded.isDone)
        {
            if(loaded.progress >= 0.9f)
            {
                loaded.allowSceneActivation = true;
            }
            yield return null;
        }

        SceneManager.UnloadSceneAsync("Title Screen");

        //set all new game attributes
        StartCoroutine(GameObject.Find("Game Manager").GetComponent<GameManager>().SetNewGameAttributes());

        Debug.Log("Started a new game");
        SaveManager.SaveGame();
        Debug.Log("Saved Game");
    }
}
