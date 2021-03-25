using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Minigame : MonoBehaviour
{
    protected string m_MinigameName; // Name of the minigame
    protected string m_DisplayedInformation; // The raw text that is displayed to the players during the minigame in the helpful info box

    private bool m_Completed; // Whether the minigame has been completed or not
    protected bool Completed
    { 
        get { return m_Completed; } 
        set { m_Completed = value; } 
    }

    protected void InitMinigame()
    {
        SetDisplayInformation();
    }

    protected void SetDisplayInformation()
    {
        GameObject infoPanel = GameObject.Find("Info Panel");
        infoPanel.transform.GetChild(0).GetComponent<Text>().text = m_MinigameName;
        infoPanel.transform.GetChild(1).GetComponent<Text>().text = m_DisplayedInformation;
    }

    protected void MinigameLost()
    {
        Completed = false;
        SceneManager.LoadScene("Main Scene");
    }

    protected void MinigameWon()
    {
        Completed = true;
        SceneManager.LoadScene("Main Scene");
    }
}
