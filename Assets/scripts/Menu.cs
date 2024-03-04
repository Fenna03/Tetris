using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    //starts gameMode
    public void PlayGame(int gameMode = 1)
    {
        if (gameMode == 0)
        {
            SceneManager.LoadScene("Menu");
        }
        if (gameMode == 1)
        {
            SceneManager.LoadScene("Tetris");
        }
        if(gameMode == 2)
        {
            SceneManager.LoadScene("HowToPlay");
        }
    }

    //quits gameMode
    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    //resets game, just starts it again
    public void ResetGame()
    {
        SceneManager.LoadScene(1);
    }
}
