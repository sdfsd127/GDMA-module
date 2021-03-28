using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetris : Minigame
{
    private struct TetrisShape
    {
        public string shape; // string to be converted to 3x3 shape (# Filled, ~ Empty)

        public TetrisShape(string shape_)
        {
            shape = shape_;
        }
    }
    private TetrisShape[] shapes;

    private class TetrisPiece
    {
        public TetrisShape shape;
        public Color colour;

        public BoardPosition[] positions;
    }

    // Board vars
    private Board board; // Actual playfield board
    private const int BOARD_WIDTH = 5;
    private const int BOARD_HEIGHT = 10;

    // Square gameobjects
    [SerializeField] private GameObject tetrisSquarePrefab;
    private GameObject[,] tetrisSquareGrid;
    private SpriteRenderer[,] tetrisSquareGridRenderer;

    // Colours
    [SerializeField] private Color[] shapeColours;

    // Game vars
    private const float TIME_STEP = 0.5f;
    Timer gameStepTimer;

    [SerializeField] private bool SHOW_FIRST_THREE; // Whether or not to render the first three rows of the tetris shape

    TetrisPiece currentPiece; // The reference to the piece currently in play

    //
    // GAME LOOP
    //
    private void Start()
    {
        // Base Class
        m_MinigameName = "Tetris";
        m_DisplayedInformation = "This is Tetris.";

        InitMinigame();

        // This Class
        InitBoard();
        InitShapes();

        InitTetris();

        // Centre camera
        int yDif = (SHOW_FIRST_THREE) ? 0 : -3;
        Camera.main.transform.position = new Vector3((BOARD_WIDTH / 2) + 0.5f, (BOARD_HEIGHT / 2) + 0.5f + yDif, Camera.main.transform.position.z);

        BeginTetris();
    }

    private void InitBoard()
    {
        // Initialise board
        board = new Board(BOARD_WIDTH, BOARD_HEIGHT);

        // Create a GameObject for each square
        GameObject squareParent = new GameObject("Tetris Background Square Parent");

        tetrisSquareGrid = new GameObject[BOARD_WIDTH, BOARD_HEIGHT];
        tetrisSquareGridRenderer = new SpriteRenderer[BOARD_WIDTH, BOARD_HEIGHT];

        for (int x = 0; x < BOARD_WIDTH; x++)
        {
            for (int y = 0; y < BOARD_HEIGHT; y++)
            {
                GameObject backgroundSquare = Instantiate(tetrisSquarePrefab, new Vector3(x, y, 0), Quaternion.identity, squareParent.transform);
                tetrisSquareGrid[x, y] = backgroundSquare;

                tetrisSquareGridRenderer[x, y] = backgroundSquare.GetComponent<SpriteRenderer>();

                if (y > BOARD_HEIGHT - 4 && !SHOW_FIRST_THREE)
                    tetrisSquareGridRenderer[x, y].enabled = false;
            }
        }
    }

    private void InitShapes()
    {
        List<TetrisShape> shapesList = new List<TetrisShape>();

        shapesList.Add(new TetrisShape("~~~~##~##")); // O Shape
        shapesList.Add(new TetrisShape("~#~~#~~#~")); // I Shape
        shapesList.Add(new TetrisShape("~~~##~~##")); // S Shape
        shapesList.Add(new TetrisShape("~~~~####~")); // Z Shape
        shapesList.Add(new TetrisShape("~##~#~~#~")); // L Shape
        shapesList.Add(new TetrisShape("##~~#~~#~")); // J Shape
        shapesList.Add(new TetrisShape("~#~###~~~")); // T Shape

        shapes = shapesList.ToArray();
    }

    private void InitTetris()
    {
        gameStepTimer = new Timer();
        gameStepTimer.SetTargetTime(TIME_STEP);
    }

    private void BeginTetris()
    {
        SpawnNextPiece();
    }

    private void Update()
    {
        if (gameStepTimer.HasReachedTarget())
        {
            gameStepTimer.ResetTimer();

            if (CanMovePieceDown())
                MovePieceDown();
            else
                SpawnNextPiece();
        }
        else
            gameStepTimer.AddTime(Time.deltaTime);

        Debug.Log(currentPiece == null);
    }

    private void SpawnNextPiece()
    {
        // Update current piece to new piece
        currentPiece = GetNewPiece();

        // Set the squares on the grid that pieces colour
        for (int i = 0; i < currentPiece.positions.Length; i++)
            SetSquareColour(currentPiece.positions[i], currentPiece.colour);
    }

    private TetrisPiece GetNewPiece()
    {
        // Pick a shape and colour
        TetrisShape randomShape = GetRandomShape();
        Color randomColour = GetRandomColour();

        // Create the piece representative
        TetrisPiece newPiece = new TetrisPiece();
        newPiece.shape = randomShape;
        newPiece.colour = randomColour;

        // Translate shape to positions
        List<BoardPosition> positionsList = new List<BoardPosition>();

        int startX = (BOARD_WIDTH / 2) - 1;
        int startY = BOARD_HEIGHT - 3 - 1;

        for (int i = 0; i < randomShape.shape.Length; i++)
        {
            // If # in shape code then that position is part of the shape
            if (randomShape.shape[i] == '#')
                positionsList.Add(new BoardPosition(startX + (i % 3), startY + ((int)Mathf.Floor(i / 3))));
        }

        // Add positions to shape
        newPiece.positions = positionsList.ToArray();

        // Return the piece
        return newPiece;
    }

    private bool CanMovePieceDown()
    {
        // Check all pieces can move down
        bool obstructed = false;

        for (int i = 0; i < currentPiece.positions.Length; i++)
            if (!IsSelfContained(currentPiece.positions, currentPiece.positions[i] + BoardPosition.down) && IsPieceBelow(currentPiece.positions[i]))
                obstructed = true;

        return obstructed;
    }

    private void MovePieceDown()
    {
        for (int i = 0; i < currentPiece.positions.Length; i++)
        {
            MoveSquareDown(currentPiece.positions[i]);
            currentPiece.positions[i] += BoardPosition.down;
        }   
    }

    private TetrisShape GetRandomShape() { return shapes[Random.Range(0, shapes.Length)]; }
    private Color GetRandomColour() { return shapeColours[Random.Range(0, shapeColours.Length)]; }

    //
    // SQUARE TOOLS
    //
    private bool IsPieceBelow(BoardPosition origin)
    {
        BoardPosition below = origin + BoardPosition.down;

        if (IsInbounds(below))
            if (board.board[below.x, below.y].filled)
                return true;

        return false;
    }

    private bool IsInbounds(BoardPosition position)
    {
        if (position.x >= 0 && position.y >= 0 && position.x < BOARD_WIDTH && position.y < BOARD_HEIGHT)
            return true;
        else
            return false;
    }

    private void MoveSquareDown(BoardPosition origin)
    {
        BoardPosition below = origin + BoardPosition.down;

        // Set new position to the old values
        board.board[below.x, below.y].WakeUp();
        SetSquareColour(below, tetrisSquareGridRenderer[origin.x, origin.y].color);

        // Set old values to default
        board.board[origin.x, origin.y].Sleep();
        SetSquareColour(origin, Color.white);
    }

    private void MoveAllSquaresDown()
    {
        // Move all squares on board down one Y level
        for (int x = 0; x < BOARD_WIDTH; x++)
            // Start at 1 as the bottom row does not need to be moved down
            for (int y = 1; y < BOARD_HEIGHT; y++)
                MoveSquareDown(new BoardPosition(x, y));
    }

    private void SetSquareColour(BoardPosition squarePosition, Color colour)
    {
        tetrisSquareGridRenderer[squarePosition.x, squarePosition.y].color = colour;
    }

    private void RandomSq()
    {
        int x = Random.Range(0, BOARD_WIDTH);
        int y = Random.Range(0, BOARD_HEIGHT);
        int colourIndex = Random.Range(0, shapeColours.Length);

        SetSquareColour(new BoardPosition(x, y), shapeColours[colourIndex]);
        board.board[x, y].SetActiveState(true);
    }

    //
    // OTHER TOOLS
    //
    private bool IsSelfContained(BoardPosition[] array, BoardPosition target)
    {
        for (int i = 0; i < array.Length; i++)
            if (array[i] == target)
                return true;

        return false;
    }
}
