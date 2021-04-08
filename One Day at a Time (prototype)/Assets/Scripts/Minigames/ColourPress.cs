using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColourPress : Minigame
{
    [System.Serializable]
    private class ColourData
    {
        public Color m_Colour;
        public string m_Name;
    }
    [SerializeField] private ColourData[] colourData;
    private ColourData currentColourData;

    private Color[] allColours;
    private List<Color> uniqueColours;

    private const int MAX_TIME = 20;
    private const int MAX_CORRECT = 15;
    private int currentCorrect;
    private int actualMaxTime;
    private int actualMaxCorrect;

    [SerializeField] private Text timeText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text colourText;
    [SerializeField] private Image[] buttonImages;

    private int correctButtonIndex;

    private void Start()
    {
        InitColourPress();
        InitMinigame("Colour Press", MINIGAME_END_CONDITION.BEFORE_TIMER, actualMaxTime);
    }

    private void InitColourPress()
    {
        currentCorrect = 0;
        correctButtonIndex = -1;

        // Account for difficulty here by scaling time and number correct needed
        actualMaxTime = (int)(MAX_TIME / m_MinigameDifficulty);
        actualMaxCorrect = (int)(MAX_CORRECT * m_MinigameDifficulty);

        SetupColours();
        NewSetup();
    }

    private void SetupColours()
    {
        uniqueColours = new List<Color>();

        for (int i = 0; i < colourData.Length; i++)
            uniqueColours.Add(colourData[i].m_Colour);

        allColours = uniqueColours.ToArray();
    }

    private void NewSetup()
    {
        // Reset the colour list from last time
        ResetUniqueColourList();

        // Pick a new true colour
        NewColour();

        // Update all the colours of the buttons
        UpdateButtons();
    }

    private void ResetUniqueColourList()
    {
        uniqueColours.Clear();

        for (int i = 0; i < allColours.Length; i++)
            uniqueColours.Add(allColours[i]);
    }

    private void NewColour()
    {
        int randomIndex = Random.Range(0, colourData.Length);
        currentColourData = colourData[randomIndex];

        colourText.text = currentColourData.m_Name.ToUpper();
        colourText.color = GetRandomColourExclude(currentColourData.m_Colour);
    }

    private void UpdateButtons()
    {
        // Choose the correct button and set it to the right correct
        correctButtonIndex = Random.Range(0, 3);

        SetButtonColour(correctButtonIndex, currentColourData.m_Colour);

        // Set the other buttons to a different random colour
        for (int i = 0; i < 3; i++)
        {
            if (i == correctButtonIndex)
                continue;

            SetButtonColour(i, GetRandomUniqueColour());
        }
    }

    private void SetButtonColour(int buttonIndex, Color colour)
    {
        buttonImages[buttonIndex].color = colour;
        RemoveColourFromUniqueList(colour);
    }

    private Color GetRandomUniqueColour()
    {
        return uniqueColours[Random.Range(0, uniqueColours.Count)];
    }

    private void RemoveColourFromUniqueList(Color usedColour)
    {
        uniqueColours.Remove(usedColour);
    }

    private Color GetRandomColourExclude(Color excludedColour)
    {
        List<Color> colourOptions = new List<Color>();

        for (int i = 0; i < allColours.Length; i++)
            if (allColours[i] != excludedColour)
                colourOptions.Add(allColours[i]);

        return colourOptions[Random.Range(0, colourOptions.Count)];
    }

    private void Update()
    {
        // Update and display the time
        SetUITimeText();

        // Update the internal timer
        UpdateMinigame();
    }

    private void SetUITimeText()
    {
        timeText.text = (actualMaxTime - GetTimerElapsedTime()).ToString("0.0");
    }

    private void SetUIScoreText()
    {
        scoreText.text = currentCorrect + " / " + actualMaxCorrect;
    }

    private void CorrectGuess()
    {
        currentCorrect++;
        SetUIScoreText();

        if (currentCorrect >= actualMaxCorrect)
            MinigameWon();
        else
            NewSetup();
    }

    //
    // EXTERNAL BUTTONS
    //
    public void OptionButtonPressed(int num)
    {
        if (num == correctButtonIndex)
            CorrectGuess();
        else
            MinigameLost();
    }
}
