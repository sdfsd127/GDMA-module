using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    // Board vars
    private Board board; // Actual playfield board
    private const int BOARD_WIDTH = 5;
    private const int BOARD_HEIGHT = 10;

    // Square gameobjects
    [SerializeField] private GameObject tetrisSquarePrefab;
    private SpriteRenderer[,] tetrisSquareGridRenderer;

    // Input Keys
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
    private bool needNewPiece;

    private const int MAX_TIME = 30;
    private int actualMaxTime;

    // UI
    [SerializeField] private Text timeText;

    //
    // GAME LOOP
    //
    private void Start()
    {
        InitTetris();
        InitMinigame("Tetris", MINIGAME_END_CONDITION.AFTER_TIMER, actualMaxTime);
    }

    private void InitTetris()
    {
        // Setup the timer
        gameStepTimer = new Timer();
        gameStepTimer.SetElapsedTime(0);
        gameStepTimer.SetTargetTime(TIME_STEP);

        // Centre camera
        int yDif = (SHOW_FIRST_THREE) ? 0 : -3;
        Camera.main.transform.position = new Vector3((BOARD_WIDTH / 2) + 0.5f, (BOARD_HEIGHT / 2) + 0.5f + yDif, Camera.main.transform.position.z);

        // Account for difficulty here by scaling time and number of hits required
        actualMaxTime = (int)(MAX_TIME / ConsistentData.m_MinigameDifficulty);

        // Setup the board
        InitBoard();

        // Setup the shapes
        InitShapes();

        // Begin the game
        BeginTetris();
    }

    private void InitBoard()
    {
        // Initialise board
        board = new Board(BOARD_WIDTH, BOARD_HEIGHT);

        // Create a GameObject for each square
        GameObject squareParent = new GameObject("Tetris Background Square Parent");

        tetrisSquareGridRenderer = new SpriteRenderer[BOARD_WIDTH, BOARD_HEIGHT];

        for (int x = 0; x < BOARD_WIDTH; x++)
        {
            for (int y = 0; y < BOARD_HEIGHT; y++)
            {
                GameObject backgroundSquare = Instantiate(tetrisSquarePrefab, new Vector3(x, y, 0), Quaternion.identity, squareParent.transform);
                tetrisSquareGridRenderer[x, y] = backgroundSquare.GetComponent<SpriteRenderer>();

                if (y > BOARD_HEIGHT - 4 && !SHOW_FIRST_THREE)
                    tetrisSquareGridRenderer[x, y].enabled = false;
            }
        }

        // Initialise spawn point of shapes on board
        shapesOrigin = new BoardPosition((BOARD_WIDTH / 2) - 1, BOARD_HEIGHT - 3);
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

            // Check if the piece can move down
            if (CanMovePiece(BoardPosition.down))
                MovePiece(BoardPosition.down); // If so move it down
            else // If not then the piece has finshed moving -> prep for the next
            {
                // DEAL WITH THE PIECE
                // Piece is no longer moving -> deactivate and delink
                needNewPiece = true;

                // Deactivate currently active squares to stop the piece moving
                BoardPosition[] activeSquares = GetAllActiveSquarePositions();

                for (int i = 0; i < activeSquares.Length; i++)
                    board.board[activeSquares[i].x, activeSquares[i].y].SetActiveState(false); // However, they are still filled

                // DEAL WITH THE ROWS
                // Check for any rows that have now been filled in and remove them
                int[] filledRows = GetRowsFilled();

                RemoveFilledRows(filledRows);
            }

            // Spawn the next piece in if there isnt one
            if (needNewPiece)
            {
                SpawnNextPiece();

                needNewPiece = false;
            }
        }
        else
        {
            gameStepTimer.AddTime(Time.deltaTime);
            timeText.text = (actualMaxTime - GetTimerElapsedTime()).ToString("0.0");
        }
        
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
    }

    private void SpawnNextPiece()
    {
        // Initialise to a default
        TetrisShape pieceShape = new TetrisShape(shapes[0].m_Chunks);
        Color pieceColour = Color.white;

        // Update in external function to random piece
        GetNewPiece(ref pieceShape, ref pieceColour);

        // Find the squares to activate for this shape
        BoardPosition[] squaresToActivate = TranslateShapeToPositionArray(pieceShape.m_Chunks, shapesOrigin);

        bool ableToSpawn = true;

        // Check if the squares are taken or not
        for (int i = 0; i < squaresToActivate.Length; i++)
            if (IsSquareFilled(squaresToActivate[i]) && !IsSquareActive(squaresToActivate[i]))
                ableToSpawn = false;

        if (ableToSpawn)
        {
            // Activate all the squares
            ActivateSquares(squaresToActivate, pieceColour);
        }
        else
            MinigameLost();
    }

    private void GetNewPiece(ref TetrisShape ts, ref Color rc)
    {
        ts = GetRandomShape();
        rc = GetRandomColour();
    }

    private bool CanMovePiece(BoardPosition direction)
    {
        bool canMove = true;

        for (int y = 0; y < BOARD_HEIGHT; y++)
        {
            for (int x = 0; x < BOARD_WIDTH; x++)
            {
                BoardPosition currentPosition = new BoardPosition(x, y);

                if (IsSquareActive(currentPosition))
                {
                    BoardPosition movedBoardPosition = currentPosition + direction;

                    if (!IsInbounds(movedBoardPosition) || (IsSquareFilled(movedBoardPosition) && !IsSquareActive(movedBoardPosition)))
                        canMove = false;
                }
            }
        }     

        return canMove;
    }

    private void MovePiece(BoardPosition direction)
    {
        List<BoardPosition> squaresToActivate = new List<BoardPosition>();

        Color pieceColour = Color.white;
        bool colourGrabbed = false;

        for (int y = 0; y < BOARD_HEIGHT; y++)
        {
            for (int x = 0; x < BOARD_WIDTH; x++)
            {
                if (board.board[x, y].m_Active)
                {
                    // Get board positions of current and future
                    BoardPosition currentSquare = new BoardPosition(x, y);
                    BoardPosition movedPosition = currentSquare + direction;

                    // Add future position to list
                    squaresToActivate.Add(movedPosition);

                    // Get the colour of the piece
                    if (!colourGrabbed)
                    {
                        pieceColour = board.board[currentSquare.x, currentSquare.y].m_Colour;
                        colourGrabbed = true;
                    }
                    
                    // Set current to white and deactivate
                    DeactivateSquare(currentSquare);
                }
            }
        }
        
        ActivateSquares(squaresToActivate.ToArray(), pieceColour);
    }

    private TetrisShape GetRandomShape() { return shapes[Random.Range(0, shapes.Length)]; }
    private Color GetRandomColour() { return shapeColours[Random.Range(0, shapeColours.Length)]; }

    private void ClearBoard()
    {
        for (int x = 0; x < BOARD_WIDTH; x++)
            for (int y = 0; y < BOARD_HEIGHT; y++)
                DeactivateSquare(new BoardPosition(x, y));
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
                // Check if the square is filled
                if (!IsSquareFilled(new BoardPosition(x, y)))
                    rowFilled = false; // noting if there is a break in the chain
            }

            if (rowFilled)
                rowsFilled.Add(y);
        }

        return rowsFilled.ToArray();
    }

    private void RemoveFilledRows(int[] rowsFilled)
    {
        // Put the list in ascending order and flip it
        System.Array.Sort(rowsFilled);
        System.Array.Reverse(rowsFilled);

        // Loop through this process for as many rows as has been filled
        for (int i = 0; i < rowsFilled.Length; i++)
        {
            // Get the Y level of this current completed row
            int currentRowY = rowsFilled[i];

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

    private void DeactivateSquares(BoardPosition[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
            DeactivateSquare(positions[i]);
    }

    private void ActivateSquare(BoardPosition position, Color colour)
    {
        board.board[position.x, position.y].WakeUp();
        SetSquareColour(position, colour);
    }

    private void ActivateSquares(BoardPosition[] positions, Color colour)
    {
        for (int i = 0; i < positions.Length; i++)
            ActivateSquare(positions[i], colour);
    }

    private BoardPosition[] GetAllActiveSquarePositions()
    {
        List<BoardPosition> squarePositions = new List<BoardPosition>();

        for (int x = 0; x < BOARD_WIDTH; x++)
        {
            for (int y = 0; y < BOARD_HEIGHT; y++)
            {
                BoardPosition currentPosition = new BoardPosition(x, y);

                if (IsSquareActive(currentPosition))
                    squarePositions.Add(currentPosition);
            }
        }

        return squarePositions.ToArray();
    }

    private bool IsSquareActive(BoardPosition position)
    {
        if (board.board[position.x, position.y].m_Active)
            return true;
        else
            return false;
    }

    private bool IsSquareFilled(BoardPosition position)
    {
        if (board.board[position.x, position.y].m_Filled)
            return true;
        else
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

        // Get old values
        bool filled = IsSquareFilled(origin);
        bool active = IsSquareActive(origin);

        // Set new position to old values
        board.board[movedPosition.x, movedPosition.y].SetFilledState(filled);
        board.board[movedPosition.x, movedPosition.y].SetActiveState(active);
        SetSquareColour(movedPosition, board.board[origin.x, origin.y].m_Colour);

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
        board.board[squarePosition.x, squarePosition.y].m_Colour = colour;
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
