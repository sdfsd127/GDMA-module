public class Board
{
    public class Boardspace
    {
        public BoardPosition m_Position;
        public UnityEngine.Color m_Colour;

        public bool m_Filled;
        public bool m_Active;

        public Boardspace()
        {
            m_Colour = UnityEngine.Color.white;
        }

        public Boardspace(BoardPosition position)
        {
            m_Position = position;
            m_Colour = UnityEngine.Color.white;
        }

        public void SetFilledState(bool state) { m_Filled = state; }
        public void SetActiveState(bool state) { m_Active = state; }
        public void SetColour(UnityEngine.Color colour) { m_Colour = colour; }

        public void WakeUp()
        {
            SetFilledState(true);
            SetActiveState(true);
        }

        public void Sleep()
        {
            SetFilledState(false);
            SetActiveState(false);
        }
    }

    public int BOARD_WIDTH;
    public int BOARD_HEIGHT;

    public Boardspace[,] board;

    public Board(int width, int height)
    {
        BOARD_WIDTH = width;
        BOARD_HEIGHT = height;

        ResetBoard();
    }

    private void ResetBoard()
    {
        board = new Boardspace[BOARD_WIDTH, BOARD_HEIGHT];

        for (int x = 0; x < BOARD_WIDTH; x++)
            for (int y = 0; y < BOARD_HEIGHT; y++)
                board[x, y] = new Boardspace(new BoardPosition(x, y));
    }
}

public class BoardPosition
{
    public int x;
    public int y;

    public BoardPosition(int x_, int y_)
    {
        x = x_;
        y = y_;
    }

    public BoardPosition(float x_, float y_)
    {
        x = (int)x_;
        y = (int)y_;
    }

    public BoardPosition(BoardPosition bp)
    {
        x = bp.x;
        y = bp.y;
    }

    public void DebugLog(string name = null)
    {
        if (name == null)
            UnityEngine.Debug.Log(string.Format("X: {0}, Y: {1}", x, y));
        else
            UnityEngine.Debug.Log(string.Format("{0}| X: {1}, Y: {2}", name, x, y));
    }

    //
    // STATICS
    //
    public static BoardPosition operator +(BoardPosition left, BoardPosition right) => new BoardPosition(left.x + right.x, left.y + right.y);
    public static BoardPosition operator -(BoardPosition left, BoardPosition right) => new BoardPosition(left.x - right.x, left.y - right.y);
    public static BoardPosition operator *(BoardPosition left, BoardPosition right) => new BoardPosition(left.x * right.x, left.y * right.y);
    public static BoardPosition operator /(BoardPosition left, BoardPosition right) => new BoardPosition(left.x / right.x, left.y / right.y);
    public static bool operator ==(BoardPosition left, BoardPosition right) { return (left.x == right.x && left.y == right.y); }
    public static bool operator !=(BoardPosition left, BoardPosition right) { return !(left == right); }

    public static BoardPosition up      = new BoardPosition(0, 1);
    public static BoardPosition down    = new BoardPosition(0, -1);
    public static BoardPosition right   = new BoardPosition(1, 0);
    public static BoardPosition left    = new BoardPosition(-1, 0);

    public static BoardPosition zero    = new BoardPosition(0, 0);
}