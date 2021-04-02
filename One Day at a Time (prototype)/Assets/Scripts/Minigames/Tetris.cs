using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetris : Minigame
{
    private class TetrisShape
    {
        public bool[] chunks;

        public TetrisShape(string shape)
        {
            chunks = new bool[9];

            for (int i = 0; i < chunks.Length; i++)
                if (shape[i] == '#')
                    chunks[i] = true;
        }

        public TetrisShape(bool[] shape)
        {
            chunks = new bool[9];

            for (int i = 0; i < chunks.Length; i++)
                if (shape[i])
                    chunks[i] = true;
        }

        public void RotateShape()
        {
            bool[] duplicateChunks = chunks;

            chunks[0] = duplicateChunks[2];
            chunks[1] = duplicateChunks[5];
            chunks[2] = duplicateChunks[8];

            chunks[3] = duplicateChunks[1];
            chunks[5] = duplicateChunks[7];

            chunks[6] = duplicateChunks[0];
            chunks[7] = duplicateChunks[3];
            chunks[8] = duplicateChunks[6];
        }
    }
    private TetrisShape[] shapes;

    private class TetrisPiece
    {
        public TetrisShape shape;
        public Color colour;

        public BoardPosition[] positions;

        public void RotatePiece()
        {
            shape.RotateShape();

            UpdateSquarePositions();
        }

        private void UpdateSquarePositions()
        {
            // Find point at which the shape begins -> (0,0) origin
            BoardPosition originPoint = GetLowestPoint();

            // Get position array
            positions = TranslateShapeToPositionArray(shape.chunks, originPoint);
        }

        public BoardPosition GetLowestPoint()
        {
            BoardPosition currentLowest = new BoardPosition(int.MaxValue, int.MaxValue);

            for (int i = 0; i < positions.Length; i++)
            {
                if (positions[i].x < currentLowest.x)
                    currentLowest.x = positions[i].x;
                if (positions[i].y < currentLowest.y)
                    currentLowest.y = positions[i].y;
            }

            return currentLowest;
        }

        private void PrintDebug(BoardPosition[] original, BoardPosition[] copy)
        {
            Debug.Log("ORIG| L: " + original.Length);
            for (int i = 0; i < original.Length; i++)
            {
                Debug.Log("ORIG| " + i + ": " + original[i].x + ", " + original[i].x);
            }

            Debug.Log("COPY| L: " + copy.Length);
            for (int i = 0; i < copy.Length; i++)
            {
                Debug.Log("COPY| " + i + ": " + copy[i].x + ", " + copy[i].x);
            }
        }
    }

    // Board vars
    private Board board; // Actual playfield board
    private const int BOARD_WIDTH = 5;
    private const int BOARD_HEIGHT = 10;

    // Square gameobjects
    [SerializeField] private GameObject tetrisSquarePrefab;
    private GameObject[,] tetrisSquareGrid;
    private SpriteRenderer[,] tetrisSquareGridRenderer;

    // Input Keys
    [SerializeField] private KeyCode rotateKeycode;
    [SerializeField] private KeyCode moveLeftKeycode;
    [SerializeField] private KeyCode moveRightKeycode;
    [SerializeField] private KeyCode moveDownKeycode;

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
        m_MinigameEndCondition = MINIGAME_END_CONDITION.AFTER_TIMER;

        InitMinigame(30);

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
        gameStepTimer.SetElapsedTime(0);
        gameStepTimer.SetTargetTime(TIME_STEP);
    }

    private void BeginTetris()
    {
        SpawnNextPiece();
    }

    private void Update()
    {
        // Update Minigame
        base.UpdateMinigame();

        // Interval between shape dropping
        if (gameStepTimer.HasReachedTarget())
        {
            gameStepTimer.ResetTimer();

            if (CanMovePiece(BoardPosition.down))
                MovePiece(BoardPosition.down);
            else
            {
                if (!GameOver())
                {
                    CheckForFilledRows();
                    SpawnNextPiece();
                }
                else
                    MinigameLost();
            }
        }
        else
            gameStepTimer.AddTime(Time.deltaTime);

        // Move Left
        if (Input.GetKeyUp(moveLeftKeycode))
            if (CanMovePiece(BoardPosition.left))
                MovePiece(BoardPosition.left);

        // Move Right
        if (Input.GetKeyUp(moveRightKeycode))
            if (CanMovePiece(BoardPosition.right))
                MovePiece(BoardPosition.right);

        // Move Down
        if (Input.GetKeyUp(moveDownKeycode))
            if (CanMovePiece(BoardPosition.down))
                MovePiece(BoardPosition.down);

        // Rotate
        if (Input.GetKeyUp(rotateKeycode))
            if (CanRotatePiece())
                RotatePiece();

        // Clear board
        if (Input.GetKeyUp(KeyCode.Backspace))
            ClearBoard();
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

        // Add positions to shape
        int startX = (BOARD_WIDTH / 2) - 1;
        int startY = BOARD_HEIGHT - 3 - 1;

        newPiece.positions = TranslateShapeToPositionArray(newPiece.shape.chunks, new BoardPosition(startX, startY));

        // Return the piece
        return newPiece;
    }

    private bool CanMovePiece(BoardPosition direction)
    {
        if (currentPiece == null)
            return false;

        bool obstructed = false;

        for (int i = 0; i < currentPiece.positions.Length; i++)
        {
            BoardPosition movedPosition = currentPiece.positions[i] + direction;

            if (!IsInbounds(movedPosition) || (!IsSelfContained(currentPiece.positions, movedPosition) && IsPieceInDirection(currentPiece.positions[i], direction)))
                obstructed = true;
        }

        return !obstructed;
    }

    private void MovePiece(BoardPosition direction)
    {
        for (int i = 0; i < currentPiece.positions.Length; i++)
        {
            // Get old position
            BoardPosition currentPosition = currentPiece.positions[i];

            // Set old square back to colourless and sleeping
            DeactivateSquare(currentPosition);
        }

        for (int i = 0; i < currentPiece.positions.Length; i++)
        {
            // Get new position
            BoardPosition movedPosition = currentPiece.positions[i] + direction;

            // Set new square to colour and awake
            ActivateSquare(movedPosition);

            // Update the position in the piece
            currentPiece.positions[i] = movedPosition;
        }
    }

    private bool CanRotatePiece()
    {
        if (currentPiece == null)
            return false;

        // Check if the piece would fit when rotated
        bool canRotate = true;

        // Get origin of piece on board
        BoardPosition shapeOrigin = currentPiece.GetLowestPoint();
        shapeOrigin.DebugLog();
        
        // Can't rotate pieces on outer edges of level
        if (shapeOrigin.x == 0 || shapeOrigin.x == BOARD_WIDTH - 1)
            return false;

        // Copy the shape and rotate it on the local copy
        TetrisShape shapeCopy = currentPiece.shape;
        shapeCopy.RotateShape();

        // Check if each chunk would fit if rotated
        for (int i = 0; i < shapeCopy.chunks.Length; i++)
        {
            // Check if the chunk is necessary and then if it would be taken in the board
            if (shapeCopy.chunks[i] && board.board[shapeOrigin.x + (i % 3), shapeOrigin.y + ((int)Mathf.Floor(i / 3))].filled)
                canRotate = false;
        }

        return canRotate;
    }

    private void RotatePiece()
    {
        // Set old squares to empty and colourless
        for (int i = 0; i < currentPiece.positions.Length; i++)
            DeactivateSquare(currentPiece.positions[i]);

        // Rotate the piece 90 degrees clockwise
        currentPiece.RotatePiece();

        // Set new squares to colour and filled
        for (int i = 0; i < currentPiece.positions.Length; i++)
            ActivateSquare(currentPiece.positions[i]);
    }

    private TetrisShape GetRandomShape() { return shapes[Random.Range(0, shapes.Length)]; }
    private Color GetRandomColour() { return shapeColours[Random.Range(0, shapeColours.Length)]; }

    private void ClearBoard()
    {
        for (int x = 0; x < BOARD_WIDTH; x++)
        {
            for (int y = 0; y < BOARD_HEIGHT; y++)
            {
                DeactivateSquare(new BoardPosition(x, y));
            }
        }

        currentPiece = null;
    }

    private void CheckForFilledRows()
    {
        // Get the Y levels of the rows that have been filled
        int[] rowsFilled = GetRowsFilled();

        // If any at all, remove them
        if (rowsFilled.Length > 0)
            RemoveFilledRows(rowsFilled);
    }

    private int[] GetRowsFilled()
    {
        // Index Y level of row filled
        List<int> rowsFilled = new List<int>();

        // Loop through all board Y levels
        for (int y = 0; y < BOARD_HEIGHT; y++)
        {
            bool rowFilled = true;

            // Loop through each square
            for (int x = 0; x < BOARD_WIDTH; x++)
            {
                // Check if the square is filled stopping if there is a break in the chain
                if (!board.board[x, y].filled)
                    rowFilled = false;
            }

            if (rowFilled)
                rowsFilled.Add(y);
        }

        return rowsFilled.ToArray();
    }

    private void RemoveFilledRows(int[] rowsFilled)
    {
        // Loop through this process for as many rows as has been filled
        for (int i = 0; i < rowsFilled.Length; i++)
        {
            // Remove the bottom row
            for (int x = 0; x < BOARD_WIDTH; x++)
                DeactivateSquare(new BoardPosition(x, rowsFilled[i] - i));

            // Move all squares down
            MoveAllSquaresDown(rowsFilled[i] - i);
        }
    }

    //
    // SQUARE TOOLS
    //
    private void DeactivateSquare(BoardPosition position)
    {
        board.board[position.x, position.y].Sleep();
        SetSquareColour(position, Color.white);
    }

    private void ActivateSquare(BoardPosition position)
    {
        board.board[position.x, position.y].WakeUp();
        SetSquareColour(position, currentPiece.colour);
    }

    private bool IsPieceInDirection(BoardPosition origin, BoardPosition direction)
    {
        BoardPosition movedPosition = origin + direction;

        if (IsInbounds(movedPosition))
            if (board.board[movedPosition.x, movedPosition.y].filled)
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

    private void MoveSquare(BoardPosition origin, BoardPosition direction)
    {
        BoardPosition movedPosition = origin + direction;

        // Set new position to the old values
        board.board[movedPosition.x, movedPosition.y].WakeUp();
        SetSquareColour(movedPosition, tetrisSquareGridRenderer[origin.x, origin.y].color);

        // Set old values to default
        board.board[origin.x, origin.y].Sleep();
        SetSquareColour(origin, Color.white);
    }

    private void MoveAllSquaresDown(int yCutoff = 0)
    {
        // Move all squares on board down one Y level
        for (int x = 0; x < BOARD_WIDTH; x++)
            // Start at 1 as the bottom row does not need to be moved down
            for (int y = 1 + yCutoff; y < BOARD_HEIGHT; y++)
                MoveSquare(new BoardPosition(x, y), BoardPosition.down);
    }

    private void SetSquareColour(BoardPosition squarePosition, Color colour)
    {
        tetrisSquareGridRenderer[squarePosition.x, squarePosition.y].color = colour;
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

    private bool GameOver()
    {
        for (int x = 0; x < BOARD_WIDTH; x++)
            if (board.board[x, BOARD_HEIGHT - 3].filled)
                return true;

        return false;
    }

    // 
    // STATICS
    //

    public static BoardPosition[] TranslateShapeToPositionArray(bool[] chunks, BoardPosition origin)
    {
        // Translate shape to positions
        List<BoardPosition> positionsList = new List<BoardPosition>();

        for (int i = 0; i < chunks.Length; i++)
        {
            if (chunks[i] == true)
                positionsList.Add(new BoardPosition(origin.x + (i % 3), origin.y + ((int)Mathf.Floor(i / 3))));
        }

        return positionsList.ToArray();
    }
}
