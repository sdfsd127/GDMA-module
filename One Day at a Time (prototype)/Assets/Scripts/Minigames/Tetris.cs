using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetris : Minigame
{
    private class TetrisShape
    {
        public bool[] m_Chunks;

        public TetrisShape(string shape, char chunkKey)
        {
            m_Chunks = new bool[9];

            for (int i = 0; i < m_Chunks.Length; i++)
                if (shape[i] == chunkKey)
                    m_Chunks[i] = true;
        }

        public TetrisShape(bool[] shape)
        {
            m_Chunks = new bool[9];

            for (int i = 0; i < m_Chunks.Length; i++)
                if (shape[i])
                    m_Chunks[i] = true;
        }

        public void RotateShape()
        {
            bool[] duplicateChunks = new bool[9];
            for (int i = 0; i < m_Chunks.Length; i++)
                duplicateChunks[i] = (m_Chunks[i]) ? true : false;

            m_Chunks[0] = duplicateChunks[2];
            m_Chunks[1] = duplicateChunks[5];
            m_Chunks[2] = duplicateChunks[8];

            m_Chunks[3] = duplicateChunks[1];
            // chunks[4] = duplicateChunks[4]; // Redundant -> but serves to explain the process
            m_Chunks[5] = duplicateChunks[7];

            m_Chunks[6] = duplicateChunks[0];
            m_Chunks[7] = duplicateChunks[3];
            m_Chunks[8] = duplicateChunks[6];
        }
    }
    private TetrisShape[] shapes;

    private class TetrisPiece
    {
        public TetrisShape m_Shape;
        public Color m_Colour;

        public BoardPosition m_Origin;
        public BoardPosition[] m_BoardPositionsClaimed;

        public TetrisPiece(BoardPosition origin)
        {
            m_Origin = origin;
        }

        public void RotatePiece()
        {
            m_Shape.RotateShape();

            UpdateSquarePositions();
        }

        private void UpdateSquarePositions()
        {
            // Get position array and update the currently claimed board squares
            m_BoardPositionsClaimed = TranslateShapeToPositionArray(m_Shape.m_Chunks, m_Origin);
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
    private BoardPosition shapesOrigin;
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

        // Initialise spawn point of shapes on board
        shapesOrigin = new BoardPosition((BOARD_WIDTH / 2) - 2, BOARD_HEIGHT - 4);
    }

    private void InitShapes()
    {
        List<TetrisShape> shapesList = new List<TetrisShape>();

        shapesList.Add(new TetrisShape("~~~~##~##", '#')); // O Shape
        shapesList.Add(new TetrisShape("~#~~#~~#~", '#')); // I Shape
        shapesList.Add(new TetrisShape("~~~##~~##", '#')); // S Shape
        shapesList.Add(new TetrisShape("~~~~####~", '#')); // Z Shape
        shapesList.Add(new TetrisShape("~##~#~~#~", '#')); // L Shape
        shapesList.Add(new TetrisShape("##~~#~~#~", '#')); // J Shape
        shapesList.Add(new TetrisShape("~#~###~~~", '#')); // T Shape

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

            // Spawn the next piece in if there isnt one
            if (currentPiece == null)
            {
                SpawnNextPiece();
            }

            if (CanMovePiece(BoardPosition.down))
                MovePiece(BoardPosition.down);
            else
            {
                // Piece is no longer moving -> deactivate and delink
                currentPiece = null;

                // Check for any rows that have now been filled in and remove them
                CheckForFilledRows();
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
        for (int i = 0; i < currentPiece.m_BoardPositionsClaimed.Length; i++)
        {
            ActivateSquare(currentPiece.m_BoardPositionsClaimed[i]);
            SetSquareColour(currentPiece.m_BoardPositionsClaimed[i], currentPiece.m_Colour);
        }
        
        // Check if the piece will fit on the board if spawned
        if (!CanSpawnShape(currentPiece.m_Shape))
            MinigameLost(); // If not then the player has lot
    }

    private TetrisPiece GetNewPiece()
    {
        // Pick a shape and colour
        TetrisShape randomShape = GetRandomShape();
        Color randomColour = GetRandomColour();

        // Create the piece representative
        TetrisPiece newPiece = new TetrisPiece(new BoardPosition(shapesOrigin));
        newPiece.m_Shape = randomShape;
        newPiece.m_Colour = randomColour;

        // Add positions to shape
        int startX = (BOARD_WIDTH / 2) - 1;
        int startY = BOARD_HEIGHT - 3 - 1;

        newPiece.m_BoardPositionsClaimed = TranslateShapeToPositionArray(newPiece.m_Shape.m_Chunks, new BoardPosition(startX, startY));

        // Return the piece
        return newPiece;
    }

    private bool CanMovePiece(BoardPosition direction)
    {
        if (currentPiece == null)
            return false;

        bool obstructed = false;

        for (int i = 0; i < currentPiece.m_BoardPositionsClaimed.Length; i++)
        {
            BoardPosition movedPosition = currentPiece.m_BoardPositionsClaimed[i] + direction;

            if (!IsInbounds(movedPosition) || (!IsSelfContained(currentPiece.m_BoardPositionsClaimed, movedPosition) && IsPieceInDirection(currentPiece.m_BoardPositionsClaimed[i], direction)))
                obstructed = true;
        }

        return !obstructed;
    }

    private void MovePiece(BoardPosition direction)
    {
        for (int i = 0; i < currentPiece.m_BoardPositionsClaimed.Length; i++)
        {
            // Get old position
            BoardPosition currentPosition = currentPiece.m_BoardPositionsClaimed[i];

            // Set old square back to colourless and sleeping
            DeactivateSquare(currentPosition);
        }

        for (int i = 0; i < currentPiece.m_BoardPositionsClaimed.Length; i++)
        {
            // Get new position
            BoardPosition movedPosition = currentPiece.m_BoardPositionsClaimed[i] + direction;

            // Set new square to colour and awake
            ActivateSquare(movedPosition);

            // Update the position in the piece
            currentPiece.m_BoardPositionsClaimed[i] = movedPosition;
        }

        currentPiece.m_Origin += direction;
    }

    private bool CanRotatePiece()
    {
        if (currentPiece == null)
            return false;

        // Check if the piece would fit when rotated
        bool canRotate = true;

        // Get origin of piece on board
        BoardPosition shapeOrigin = new BoardPosition(currentPiece.m_Origin.x, currentPiece.m_Origin.y);
        
        // Can't rotate pieces on outer edges of level
        if (shapeOrigin.x == 0 || shapeOrigin.x == BOARD_WIDTH - 1)
            return false;

        // Copy the shape and rotate it on the local copy
        TetrisShape shapeCopy = new TetrisShape(currentPiece.m_Shape.m_Chunks);
        shapeCopy.RotateShape();

        // Check if each chunk would fit if rotated
        for (int i = 0; i < shapeCopy.m_Chunks.Length; i++)
        {
            // Check if the chunk is necessary and then if it would be taken in the board
            if (shapeCopy.m_Chunks[i] && board.board[shapeOrigin.x + (i % 3), shapeOrigin.y + ((int)Mathf.Floor(i / 3))].filled)
                canRotate = false;
        }

        return canRotate;
    }

    private void RotatePiece()
    {
        // Set old squares to empty and colourless
        for (int i = 0; i < currentPiece.m_BoardPositionsClaimed.Length; i++)
            DeactivateSquare(currentPiece.m_BoardPositionsClaimed[i]);

        // Rotate the piece 90 degrees clockwise
        currentPiece.RotatePiece();

        // Set new squares to colour and filled
        for (int i = 0; i < currentPiece.m_BoardPositionsClaimed.Length; i++)
            ActivateSquare(currentPiece.m_BoardPositionsClaimed[i]);
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
            // Get the Y level of this current row
            int currentRowY = rowsFilled[i] - i; // Account for rows already removed with (i)

            // Remove the row that was filled
            for (int x = 0; x < BOARD_WIDTH; x++)
                DeactivateSquare(new BoardPosition(x, currentRowY));

            // Move all squares down
            MoveAllSquaresDown(currentRowY);
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
        SetSquareColour(position, currentPiece.m_Colour);
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

    private bool CanSpawnShape(TetrisShape shape)
    {
        BoardPosition[] squaresRequired = TranslateShapeToPositionArray(shape.m_Chunks, shapesOrigin);

        bool canSpawn = true;

        for (int i = 0; i < squaresRequired.Length; i++)
        {
            BoardPosition currentSquare = squaresRequired[i];

            if (board.board[currentSquare.x, currentSquare.y].filled)
                canSpawn = false;
        }

        return canSpawn;
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
