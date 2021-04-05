public class Board
{
    public class Boardspace
    {
        public BoardPosition position;
        public bool filled;
        public bool active;

        public Boardspace()
        {

        }

        public Boardspace(BoardPosition position_)
        {
            position = position_;
        }

        public void SetFilledState(bool state) { filled = state; }
        public void SetActiveState(bool state) { active = state; }

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

// Disable .Equals and .Hash overload warning
#pragma warning disable 0660
#pragma warning disable 0661
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

    public static BoardPosition up = new BoardPosition(0, 1);
    public static BoardPosition down = new BoardPosition(0, -1);
    public static BoardPosition right = new BoardPosition(1, 0);
    public static BoardPosition left = new BoardPosition(-1, 0);
}