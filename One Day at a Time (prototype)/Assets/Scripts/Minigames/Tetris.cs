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

    // Board vars
    private Board preboard; // Where the shapes initially spawn
    private Board board; // Actual playfield board
    private const int BOARD_WIDTH = 5;
    private const int BOARD_HEIGHT = 8;
    private const int PREBOARD_HEIGHT = 3;

    // Square gameobjects
    [SerializeField] private GameObject tetrisSquarePrefab;
    private GameObject[,] tetrisSquareGrid;
    private SpriteRenderer[,] tetrisSquareGridRenderer;

    // Colours
    [SerializeField] private Color[] shapeColours;

    // Game vars
    private const float TIME_STEP = 0.5f;
    Timer gameStepTimer;

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
        Camera.main.transform.position = new Vector3((BOARD_WIDTH / 2) + 0.5f, (BOARD_HEIGHT / 2) + 0.5f, Camera.main.transform.position.z);

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
            }
        }

        // Initialise preboard
        preboard = new Board(3, 3);
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
        PlaceShapeIntoPreboard(GetRandomShape());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RandomSq();
        }

        if (gameStepTimer.HasReachedTarget())
        {
            gameStepTimer.ResetTimer();

            MoveAllSquaresDown();
        }
        else
            gameStepTimer.AddTime(Time.deltaTime);
    }

    private void MoveAllSquaresDown()
    {
        for (int x = 0; x < BOARD_WIDTH; x++)
        {
            for (int y = 1; y < BOARD_HEIGHT; y++) // Start at 1 as the bottom row does not need to be moved down
            {
                BoardPosition currentSq = new BoardPosition(x, y);

                // If the piece is still actively moving
                if (board.board[currentSq.x, currentSq.y].active)
                {
                    // If there is not a piece below
                    if (!IsPieceBelow(currentSq))
                    {
                        // Move square down
                        MoveSquareDown(currentSq);
                    }
                    else
                    {
                        // If there is, set piece to no longer actively moving
                        board.board[currentSq.x, currentSq.y].SetActiveState(false);
                    }
                }
            }
        }
    }

    private void PlaceShapeIntoPreboard(TetrisShape shape)
    {
        for (int y = 0; y < preboard.BOARD_HEIGHT; y++)
            for (int x = 0; x < preboard.BOARD_WIDTH; x++)
                if (shape.shape[y * preboard.BOARD_WIDTH + x] == '#')
                    preboard.board[x, y].filled = true;
    }

    private TetrisShape GetRandomShape() { return shapes[Random.Range(0, shapes.Length)]; }

    //
    // SQUARE TOOLS
    //
    private bool IsInbounds(BoardPosition position)
    {
        if (position.x > 0 && position.y > 0 && position.x < BOARD_WIDTH && position.y < BOARD_HEIGHT)
            return true;
        else
            return false;
    }

    private bool IsPieceBelow(BoardPosition origin)
    {
        BoardPosition below = origin + BoardPosition.down;

        if (IsInbounds(below))
            if (board.board[below.x, below.y].filled)
                return true;

        return false;
    }

    private void MoveSquareDown(BoardPosition origin)
    {
        BoardPosition below = origin + BoardPosition.down;

        // Set new position to the old values
        board.board[below.x, below.y].SetActiveState(board.board[origin.x, origin.y].active);
        board.board[below.x, below.y].SetFilledState(board.board[origin.x, origin.y].filled);
        SetSquareColour(below, tetrisSquareGridRenderer[below.x, below.y].color);

        // Set old values to default
        board.board[origin.x, origin.y].SetActiveState(false);
        board.board[origin.x, origin.y].SetFilledState(false);
        SetSquareColour(origin, Color.white);
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
}
