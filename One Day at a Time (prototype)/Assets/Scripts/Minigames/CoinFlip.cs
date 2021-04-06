using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinFlip : Minigame
{
    private const int MAX_TIMES_CORRECT = 3;
    private int currentTimesCorrect;

    private const int MAX_ROTATIONS = 7;
    private const int MIN_ROTATIONS = 2;

    private const float TIME_BETWEEN_ROT = 0.02f;
    private const int ROT_SPEED = 12;

    [SerializeField] private GameObject coinObject;

    [SerializeField] private Text scoreText;

    private enum SIDE
    {
        HEADS = 0,
        TAILS = 1
    }

    private SIDE playerSideChoice;
    private SIDE coinCurrentSideShowing;

    private void Start()
    {
        // Base Class
        m_MinigameName = "Coin Flip";
        m_DisplayedInformation = "This is Coin Flipper.";
        m_MinigameEndCondition = MINIGAME_END_CONDITION.WIN_LOSE;

        InitMinigame();

        // This Class
        InitCoinFlip();
    }

    private void InitCoinFlip()
    {
        currentTimesCorrect = 0;
        coinCurrentSideShowing = SIDE.TAILS;
        UpdateCorrectTextUI();
    }

    private void FlipCoin()
    {
        // Decide the outcome randomly
        SIDE decidedSide = GetRandomSide();
        int rotationsToMake = Random.Range(MIN_ROTATIONS, MAX_ROTATIONS + 1);

        // Setup bool for whether the coin needs an extra 180 to show the decided side or not
        bool extraFlip = false;
        if (decidedSide != coinCurrentSideShowing)
            extraFlip = true;

        // Set the outcome
        coinCurrentSideShowing = decidedSide;

        // Visualise the flips
        StartCoroutine(RotateCoin(rotationsToMake, extraFlip));
    }

    private IEnumerator RotateCoin(int numberOfRotations, bool flip)
    {
        int degreesPerRotation = 360;

        for (int i = 0; i < numberOfRotations; i++)
        {
            for (int j = 0; j < degreesPerRotation; j += ROT_SPEED)
            {
                coinObject.transform.eulerAngles += new Vector3(0, ROT_SPEED, 0);
                yield return new WaitForSeconds(TIME_BETWEEN_ROT);
            }
        }

        if (flip)
        {
            for (int j = 0; j < 180; j += ROT_SPEED)
            {
                coinObject.transform.eulerAngles += new Vector3(0, ROT_SPEED, 0);
                yield return new WaitForSeconds(TIME_BETWEEN_ROT);
            }
        }

        FinishedFlippingCoin();
    }

    private void FinishedFlippingCoin()
    {
        if (GuessedCorrect())
        {
            currentTimesCorrect++;
            UpdateCorrectTextUI();

            if (currentTimesCorrect >= MAX_TIMES_CORRECT)
                MinigameWon();
        }
        else
            MinigameLost();
    }

    private bool GuessedCorrect()
    {
        if (playerSideChoice == coinCurrentSideShowing)
            return true;
        else
            return false;
    }

    private void UpdateCorrectTextUI()
    {
        scoreText.text = currentTimesCorrect + " / " + MAX_TIMES_CORRECT;
    }

    //
    // TOOLS
    //
    private SIDE GetRandomSide()
    {
        if (Random.Range(0, 2) == 0)
            return SIDE.HEADS;
        else
            return SIDE.TAILS;
    }

    //
    // EXTERNAL BUTTON PRESSES
    //
    public void HeadsPressed()
    {
        playerSideChoice = SIDE.HEADS;
        FlipCoin();
    }

    public void TailsPressed()
    {
        playerSideChoice = SIDE.TAILS;
        FlipCoin();
    }
}
