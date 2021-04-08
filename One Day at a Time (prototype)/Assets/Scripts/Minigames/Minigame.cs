using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Minigame : MonoBehaviour
{
    protected enum MINIGAME_END_CONDITION
    {
        WIN_LOSE = 0, // Minigame ends after being won or lost
        BEFORE_TIMER = 1, // Minigame must be completed by an action before the timer
        AFTER_TIMER = 2 // Minigame is completed when the timer is up
    }
    protected MINIGAME_END_CONDITION m_MinigameEndCondition; // This minigames method of concluding

    protected string m_MinigameName; // Name of the minigame
    private bool m_Completed; // Whether the minigame has been completed or not
    protected bool Completed
    { 
        get { return m_Completed; } 
        set { m_Completed = value; } 
    }

    private Timer m_Timer = new Timer();

    protected void InitMinigame(string minigameName, MINIGAME_END_CONDITION endCondition, float timerTarget = 0.0f)
    {
        m_MinigameName = minigameName;
        m_MinigameEndCondition = endCondition;
        
        if (timerTarget > 0.0f)
            SetEndConditionTimerTarget(timerTarget);

        ControlCursor();
    }

    private void SetEndConditionTimerTarget(float endTime)
    {
        m_Timer.SetTargetTime(endTime);
    }

    private void ControlCursor()
    {
        if (!Completed)
            CursorControl.SetCursorState(CursorLockMode.None, true);
        else
            CursorControl.SetCursorState(CursorLockMode.Locked, false);
    }

    protected void UpdateMinigame()
    {
        m_Timer.AddTime(Time.deltaTime);

        if (m_Timer.HasReachedTarget())
            switch (m_MinigameEndCondition)
            {
                case MINIGAME_END_CONDITION.WIN_LOSE:
                    // Do nothing. No timer involved here
                    break;
                case MINIGAME_END_CONDITION.BEFORE_TIMER:
                    // Timer has reached end and player has not completed objective -> Player loses
                    MinigameLost();
                    break;
                case MINIGAME_END_CONDITION.AFTER_TIMER:
                    // Timer has reached end and player has successfully survived -> Player wins
                    MinigameWon();
                    break;
            }
    }

    protected void MinigameLost()
    {
        Completed = false;
        Debug.Log("MINIGAME LOST");
        SceneManager.LoadScene("Main Scene");
    }

    protected void MinigameWon()
    {
        Completed = true;
        Debug.Log("MINIGAME WON");
        SceneManager.LoadScene("Main Scene");
    }

    protected float GetTimerElapsedTime()
    {
        return m_Timer.GetElapsedTime();
    }
}
