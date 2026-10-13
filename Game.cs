namespace Minesweeper;

public sealed class Cell
{
    public bool Mine;
    public bool Open;
    public bool Flag;
    public int N;
}

public sealed class Game
{
    static readonly Random Rng = new();

    public int Rows { get; }
    public int Cols { get; }
    public int Mines { get; }
    public Cell[] Cells { get; }
    public bool Started { get; private set; }
    public bool Over { get; private set; }
    public bool Won { get; private set; }
    public int Opened { get; private set; }
    public int Flags { get; private set; }
    public int Hit { get; private set; } = -1;

    public Game(int rows, int cols, int mines)
    {
        Rows = rows;
        Cols = cols;
        Mines = mines;
        Cells = new Cell[rows * cols];
        for (int i = 0; i < Cells.Length; i++) Cells[i] = new Cell();
    }

    public IEnumerable<int> Neighbours(int i)
    {
        int r = i / Cols, c = i % Cols;
        for (int dr = -1; dr <= 1; dr++)
        {
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                int rr = r + dr, cc = c + dc;
                if (rr >= 0 && rr < Rows && cc >= 0 && cc < Cols) yield return rr * Cols + cc;
            }
        }
    }

    void Place(int first)
    {
        // First click and its neighbours are mine-free (if the board has room).
        var safe = new HashSet<int>(Neighbours(first)) { first };
        if (Cells.Length - safe.Count < Mines)
        {
            safe.Clear();
            safe.Add(first);
        }

        var pool = Enumerable.Range(0, Cells.Length).Where(i => !safe.Contains(i)).ToArray();
        Rng.Shuffle(pool);
        for (int k = 0; k < Mines; k++) Cells[pool[k]].Mine = true;

        for (int i = 0; i < Cells.Length; i++)
            Cells[i].N = Neighbours(i).Count(j => Cells[j].Mine);
    }

    public void Open(int i)
    {
        var c = Cells[i];
        if (Over || c.Open || c.Flag) return;

        if (!Started)
        {
            Place(i);
            Started = true;
        }

        if (c.Mine)
        {
            Lose(i);
            return;
        }

        var stack = new Stack<int>();
        stack.Push(i);
        while (stack.Count > 0)
        {
            int k = stack.Pop();
            var ck = Cells[k];
            if (ck.Open || ck.Flag) continue;
            ck.Open = true;
            Opened++;
            if (ck.N == 0)
            {
                foreach (int j in Neighbours(k))
                    if (!Cells[j].Open) stack.Push(j);
            }
        }

        if (Opened == Cells.Length - Mines) Win();
    }

    public void Chord(int i)
    {
        var c = Cells[i];
        if (Over || !c.Open || c.N == 0) return;

        var nb = Neighbours(i).ToList();
        if (nb.Count(j => Cells[j].Flag) != c.N) return;

        foreach (int j in nb)
        {
            if (Over) break;
            Open(j);
        }
    }

    public void ToggleFlag(int i)
    {
        var c = Cells[i];
        if (Over || c.Open) return;
        c.Flag = !c.Flag;
        Flags += c.Flag ? 1 : -1;
    }

    void Lose(int i)
    {
        Over = true;
        Hit = i;
        foreach (var c in Cells)
            if (c.Mine && !c.Flag) c.Open = true;
    }

    void Win()
    {
        Over = true;
        Won = true;
        foreach (var c in Cells)
            if (c.Mine) c.Flag = true;
        Flags = Mines;
    }
}
