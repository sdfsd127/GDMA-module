using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Simon : Minigame
{
    //
    // DEFINITIONS
    // 
    private enum COLOUR
    {
        GREEN = 0,
        RED = 1,
        BLUE = 2,
        YELLOW = 3
    }
    [SerializeField] private Image[] squareGrids;
    private Color[] originalColours;
    private List<COLOUR> colourEntries;

    private enum GAME_STATE
    {
        WATCHING = 0,
        REPEATING = 1
    }
    private GAME_STATE gameState;

    private int currentCount;
    private const int MAX_COUNT = 5;
    private const float TIME_BETWEEN_FADE_CYCLE = 0.25f;
    private const float TIME_HIGHLIGHTED = 0.5f;
    
    //
    // GAME LOOP
    //
    private void Start()
    {
        // Base Class
        m_MinigameName = "Simon";
        m_DisplayedInformation = "This is Simon.";

        InitMinigame();

        // This Class
        colourEntries = new List<COLOUR>();

        originalColours = new Color[squareGrids.Length];
        for (int i = 0; i < squareGrids.Length; i++)
            originalColours[i] = squareGrids[i].color;

        currentCount = 0;

        // Begin
        AddNewColour();
        DisplayCurrentPattern();
    }

    private void Update()
    {

    }

    private void AddNewColour()
    {
        ResetCount();

        int colourKey = Random.Range(0, 4);
        COLOUR colour = ConvertKeyToColour(colourKey);

        colourEntries.Add(colour);
    }

    private void ResetCount()
    {
        currentCount = 0;
    }

    private void DisplayCurrentPattern()
    {
        StartCoroutine(DisplayPattern());
    }

    //
    // TOOLS
    //
    private void SetGameState(GAME_STATE state)
    {
        gameState = state;
    }

    private COLOUR ConvertKeyToColour(int key)
    {
        switch (key)
        {
            case 0:
                return COLOUR.GREEN;
            case 1:
                return COLOUR.RED;
            case 2:
                return COLOUR.BLUE;
            case 3:
                return COLOUR.YELLOW;
            default:
                return COLOUR.GREEN;
        }
    }

    private int ConvertColourToKey(COLOUR colour)
    {
        switch (colour)
        {
            case COLOUR.GREEN:
                return 0;
            case COLOUR.RED:
                return 1;
            case COLOUR.BLUE:
                return 2;
            case COLOUR.YELLOW:
                return 3;
            default:
                return -1;
        }
    }

    private bool SameColour(COLOUR a, COLOUR b)
    {
        if (a == b)
            return true;
        else
            return false;
    }

    private void HighlightSegment(int segmentID, Color hightlight)
    {
        squareGrids[segmentID].color = hightlight;
    }

    private void ResetSegment(int segmentID)
    {
        squareGrids[segmentID].color = originalColours[segmentID];
    }

    private void ResetAllSegments()
    {
        for (int i = 0; i < squareGrids.Length; i++)
            ResetSegment(i);
    }

    //
    // CO-ROUTINES
    //
    IEnumerator DisplayPattern()
    {
        // Set player watching game state
        SetGameState(GAME_STATE.WATCHING);

        for (int i = 0; i < colourEntries.Count; i++)
        {
            ResetAllSegments();
            yield return new WaitForSeconds(TIME_BETWEEN_FADE_CYCLE);

            int segmentID = ConvertColourToKey(colourEntries[i]);
            HighlightSegment(segmentID, Color.white);

            yield return new WaitForSeconds(TIME_HIGHLIGHTED);
        }

        ResetAllSegments();

        // Set player repeating game state
        SetGameState(GAME_STATE.REPEATING);
    }

    //
    // EXTERNAL CALLS
    //
    public void PressedSegment(int colourKey)
    {
        // If currently in INPUT stage
        if (gameState == GAME_STATE.REPEATING)
        {
            // If input colour is correct
            if (SameColour(colourEntries[currentCount], ConvertKeyToColour(colourKey)))
            {
                // Move onto next colour
                currentCount++;

                // Check if there is a next colour
                if (currentCount >= colourEntries.Count)
                {
                    // If reached end, check finished or continuing
                    if (currentCount >= MAX_COUNT)
                        MinigameWon();
                    else
                    {
                        AddNewColour();
                        DisplayCurrentPattern();
                    }
                }
            }
            else
                MinigameLost();
        } 
    }
}
