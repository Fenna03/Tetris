using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;


public class Menu : MonoBehaviour
{
    public void PlayGame(int gameMode = 1)
    {
        PlayerPrefs.SetInt("mode", gameMode);

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
        if(gameMode == 3)
        {
            SceneManager.LoadScene("TetrisUpsideDown");
        }
        if (gameMode == 4)
        {
            SceneManager.LoadScene("reverse");
        }
        if(gameMode == 5)
        {
            SceneManager.LoadScene("blackWhite");
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
