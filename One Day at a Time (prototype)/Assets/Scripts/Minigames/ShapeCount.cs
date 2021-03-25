using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShapeCount : Minigame
{
    [SerializeField] private Sprite[] shapeSprites;

    private List<GameObject> shapes;
    private int circleCount;
    private int squareCount;
    private int triangleCount;

    private const int MIN_SHAPES = 10;
    private const int MAX_SHAPES = 30;

    private const float BOUND = 4.0f;
    private Vector2 UPPER_LEFT = new Vector2(-BOUND, BOUND);
    private Vector2 BOTTOM_RIGHT = new Vector2(BOUND, -BOUND);

    private const float MAX_TIME_ALLOWED = 30.0f;
    private float currentTime;

    [SerializeField] private Text timeText;
    [SerializeField] private Text optionAText;
    [SerializeField] private Text optionBText;
    private bool a_IsCorrectOption;

    private int timesCorrect;
    private const int MAX_TIMES_CORRECT = 5;

    //
    // GAME LOOP
    //
    private void Start()
    {
        // Base Class
        m_MinigameName = "Shape Counting";
        m_DisplayedInformation = "This is Shape Counting.";

        InitMinigame();

        // This Class
        shapes = new List<GameObject>();

        NewSetup();
    }

    private void Update()
    {
        currentTime += Time.deltaTime;

        if (currentTime >= MAX_TIME_ALLOWED && !Completed)
            MinigameLost();
        else
            timeText.text = (MAX_TIME_ALLOWED - currentTime).ToString("00:00");
    }

    private void NewSetup()
    {
        // Remove any pre existing shapes
        RemoveExistingShapes();

        // Calculate and create the shapes
        SpawnNewShapes();

        // Set the onscreen UI options
        SetOptions();
    }

    private void SpawnNewShapes()
    {
        // Find the number to spawn (Type specific)
        int circles = 0, squares = 0, triangles = 0;
        GetShapeNumberSplit(ref circles, ref squares, ref triangles);

        // Spawn Circles
        for (int i = 0; i < circles; i++)
            SpawnShape("CIRCLE", 0, ref circleCount, GetRandomInboundsPosition());

        // Spawn Squares
        for (int i = 0; i < circles; i++)
            SpawnShape("SQUARE", 1, ref squareCount, GetRandomInboundsPosition());

        // Spawn Triangles
        for (int i = 0; i < circles; i++)
            SpawnShape("TRIANGLE", 2, ref triangleCount, GetRandomInboundsPosition());
    }

    private void GetShapeNumberSplit(ref int circles, ref int squares, ref int triangles)
    {
        // Randomly pick max pool size of shapes
        int numOfShapes = Random.Range(MIN_SHAPES, MAX_SHAPES);

        // Randomly pick the number of each shape type
        circles = Random.Range(0, numOfShapes);
        squares = Random.Range(0, numOfShapes - circles);
        triangles = numOfShapes - circles - squares;
    }

    private void SpawnShape(string gameObjectName, int spriteIndex, ref int incrementer, Vector3 position)
    {
        GameObject newShape = new GameObject(gameObjectName);
        newShape.transform.position = position;
        newShape.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));

        SpriteRenderer spriteRenderer = newShape.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = shapeSprites[spriteIndex];
        spriteRenderer.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));

        incrementer++;

        shapes.Add(newShape);
    }

    private void SetOptions()
    {
        int trueOption = circleCount;
        int fakeShift = Random.Range(1, (trueOption / 10) * 1);
        if (FiftyFifty())
            fakeShift *= -1;
        int fakeOption = trueOption + fakeShift;
        
        if (FiftyFifty())
        {
            optionAText.text = trueOption.ToString();
            optionBText.text = fakeOption.ToString();
            a_IsCorrectOption = true;
        }
        else
        {
            optionBText.text = trueOption.ToString();
            optionAText.text = fakeOption.ToString();
            a_IsCorrectOption = false;
        }
    }

    //
    // TOOLS
    //
    private Vector3 GetRandomInboundsPosition()
    {
        return new Vector3(Random.Range(UPPER_LEFT.x, BOTTOM_RIGHT.x), Random.Range(BOTTOM_RIGHT.y, UPPER_LEFT.y), 0);
    }

    private bool FiftyFifty()
    {
        return Random.Range(0, 2) == 0;
    }

    private void RemoveExistingShapes()
    {
        for (int i = 0; i < shapes.Count; i++)
        {
            Destroy(shapes[i]);
        }

        shapes.Clear();

        circleCount = 0;
        squareCount = 0;
        triangleCount = 0;
    }

    //
    // EXTERNAL CALLS
    //
    public void OptionButtonPressed(int optionID)
    {
        if ((optionID == 0 && a_IsCorrectOption) || (optionID == 1 && !a_IsCorrectOption))
        {
            timesCorrect++;

            if (timesCorrect >= MAX_TIMES_CORRECT)
                MinigameWon();
            else
                NewSetup();
        }
        else
            MinigameLost();
    }
}
