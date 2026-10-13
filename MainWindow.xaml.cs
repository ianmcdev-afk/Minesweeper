using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Minesweeper;

public sealed record Level(string Key, int Rows, int Cols, int Mines, int Size);

public sealed class ScoreRow
{
    public string Rank { get; set; } = "";
    public string Name { get; set; } = "";
    public string Time { get; set; } = "";
    public string Date { get; set; } = "";
    public bool IsNew { get; set; }
}

public partial class MainWindow : Window
{
    static readonly Level[] Levels =
    {
        new("Beginner", 9, 9, 10, 38),
        new("Intermediate", 16, 16, 40, 32),
        new("Expert", 16, 30, 99, 28)
    };

    static SolidColorBrush Br(string hex)
    {
        var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)!);
        b.Freeze();
        return b;
    }

    // Brushes for the active theme (rebuilt by ApplyTheme).
    SolidColorBrush _coverA = null!, _coverB = null!, _coverEdge = null!;
    SolidColorBrush _openA = null!, _openB = null!, _openEdge = null!;
    SolidColorBrush _danger = null!, _wrongBg = null!, _flag = null!, _mineInk = null!;
    SolidColorBrush _muted = null!, _win = null!, _lose = null!;
    SolidColorBrush[] _nums = Array.Empty<SolidColorBrush>();
    Theme _theme = Themes.All[0];
    enum StatusKind { Normal, Win, Lose }
    StatusKind _kind = StatusKind.Normal;
    bool _init = true;

    static readonly FontFamily SymbolFont = new("Segoe UI Symbol");

    Level _level = Levels[0];
    Game _g = null!;
    Border[] _cells = Array.Empty<Border>();
    readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(100) };
    readonly Stopwatch _sw = new();
    string? _lastNewId;

    public MainWindow()
    {
        InitializeComponent();
        _timer.Tick += (_, _) => TimeText.Text = Fmt((int)_sw.Elapsed.TotalSeconds);

        var saved = ScoreStore.Load();
        _theme = Themes.All.FirstOrDefault(t => t.Name == saved.Theme) ?? Themes.All[0];
        ApplyTheme(_theme);
        ThemeBox.ItemsSource = Themes.All.Select(t => t.Name).ToList();
        ThemeBox.SelectedItem = _theme.Name;
        _init = false;

        NewGame();
    }

    /* ---------- themes ---------- */

    void ApplyTheme(Theme t)
    {
        _theme = t;
        Resources["BgBrush"] = Br(t.Bg);
        Resources["PanelBrush"] = Br(t.Panel);
        Resources["LineBrush"] = Br(t.Line);
        Resources["InkBrush"] = Br(t.Ink);
        Resources["MutedBrush"] = Br(t.Muted);
        Resources["AccentBrush"] = Br(t.Accent);
        Resources["NewRowBrush"] = Br(t.NewRow);

        _coverA = Br(t.CoverA); _coverB = Br(t.CoverB); _coverEdge = Br(t.CoverEdge);
        _openA = Br(t.OpenA); _openB = Br(t.OpenB); _openEdge = Br(t.OpenEdge);
        _danger = Br(t.Danger); _wrongBg = Br(t.WrongBg); _flag = Br(t.Flag); _mineInk = Br(t.MineInk);
        _muted = Br(t.Muted); _win = Br(t.Win); _lose = Br(t.Lose);
        _nums = t.Nums.Select(Br).ToArray();

        StatusText.Foreground = _kind switch
        {
            StatusKind.Win => _win,
            StatusKind.Lose => _lose,
            _ => _muted
        };
        if (_cells.Length > 0) Refresh();
    }

    void ThemeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_init || ThemeBox.SelectedItem is not string name) return;
        var theme = Themes.All.FirstOrDefault(t => t.Name == name);
        if (theme is null) return;

        ApplyTheme(theme);
        var data = ScoreStore.Load();
        data.Theme = name;
        ScoreStore.Save(data);
    }

    void SetStatus(string text, StatusKind kind)
    {
        _kind = kind;
        StatusText.Text = text;
        StatusText.Foreground = kind switch
        {
            StatusKind.Win => _win,
            StatusKind.Lose => _lose,
            _ => _muted
        };
    }

    /* ---------- formatting ---------- */

    static string Fmt(int n) => Math.Clamp(n, -99, 999).ToString("000", CultureInfo.InvariantCulture);

    static string FmtMs(long ms) =>
        (ms / 1000.0).ToString("0.00", CultureInfo.InvariantCulture) + "s";

    /* ---------- setup ---------- */

    void NewGame()
    {
        _timer.Stop();
        _sw.Reset();
        _g = new Game(_level.Rows, _level.Cols, _level.Mines);
        _lastNewId = null;

        BuildBoard();
        TimeText.Text = "000";
        SetStatus("Click any square to start", StatusKind.Normal);

        foreach (var tb in new[] { BtnBeginner, BtnIntermediate, BtnExpert })
            tb.IsChecked = (string)tb.Tag == _level.Key;

        Refresh();
        RenderScores();
    }

    void BuildBoard()
    {
        BoardGrid.Children.Clear();
        BoardGrid.Rows = _g.Rows;
        BoardGrid.Columns = _g.Cols;
        BoardGrid.Width = _g.Cols * _level.Size;
        BoardGrid.Height = _g.Rows * _level.Size;

        _cells = new Border[_g.Cells.Length];
        for (int i = 0; i < _cells.Length; i++)
        {
            var text = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = SymbolFont,
                FontWeight = FontWeights.Bold,
                FontSize = _level.Size * 0.55
            };
            var cell = new Border
            {
                Width = _level.Size,
                Height = _level.Size,
                Tag = i,
                BorderThickness = new Thickness(1),
                Child = text
            };
            cell.MouseLeftButtonDown += Cell_Left;
            cell.MouseRightButtonDown += Cell_Right;
            cell.MouseDown += Cell_Middle;
            _cells[i] = cell;
            BoardGrid.Children.Add(cell);
        }
    }

    /* ---------- rendering ---------- */

    void Refresh()
    {
        for (int i = 0; i < _cells.Length; i++) Paint(i);
        MinesText.Text = Fmt(_g.Mines - _g.Flags);
    }

    void Paint(int i)
    {
        var c = _g.Cells[i];
        var b = _cells[i];
        var t = (TextBlock)b.Child;
        bool alt = ((i / _g.Cols) + (i % _g.Cols)) % 2 == 1;
        t.Text = "";

        if (c.Open)
        {
            b.BorderBrush = _openEdge;
            if (c.Mine)
            {
                b.Background = i == _g.Hit ? _danger : _openA;
                t.Text = "●";
                t.Foreground = _mineInk;
            }
            else
            {
                b.Background = alt ? _openB : _openA;
                if (c.N > 0)
                {
                    t.Text = c.N.ToString(CultureInfo.InvariantCulture);
                    t.Foreground = _nums[c.N];
                }
            }
        }
        else
        {
            b.BorderBrush = _coverEdge;
            b.Background = alt ? _coverB : _coverA;
            if (c.Flag)
            {
                if (_g.Over && !_g.Won && !c.Mine)
                {
                    b.Background = _wrongBg;
                    t.Text = "✗";
                    t.Foreground = _mineInk;
                }
                else
                {
                    t.Text = "⚑";
                    t.Foreground = _flag;
                }
            }
        }
    }

    /* ---------- input ---------- */

    static int Idx(object sender) => (int)((FrameworkElement)sender).Tag;

    void Cell_Left(object sender, MouseButtonEventArgs e)
    {
        int i = Idx(sender);
        if (_g.Cells[i].Open) Act(() => _g.Chord(i));
        else Act(() => _g.Open(i));
    }

    void Cell_Right(object sender, MouseButtonEventArgs e)
    {
        int i = Idx(sender);
        Act(() => _g.ToggleFlag(i));
    }

    void Cell_Middle(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle) return;
        int i = Idx(sender);
        Act(() => _g.Chord(i));
    }

    void Act(Action action)
    {
        if (_g.Over) return;
        bool wasStarted = _g.Started;
        action();

        if (!wasStarted && _g.Started)
        {
            _sw.Restart();
            _timer.Start();
            SetStatus("Good luck", StatusKind.Normal);
        }

        Refresh();
        if (!_g.Over) return;

        _timer.Stop();
        _sw.Stop();

        if (_g.Won)
        {
            long ms = _sw.ElapsedMilliseconds;
            TimeText.Text = Fmt((int)(ms / 1000));
            SetStatus("Cleared in " + FmtMs(ms), StatusKind.Win);
            OfferScore(ms);
        }
        else
        {
            SetStatus("Boom. Game over", StatusKind.Lose);
        }
    }

    void Level_Click(object sender, RoutedEventArgs e)
    {
        string key = (string)((FrameworkElement)sender).Tag;
        _level = Levels.First(l => l.Key == key);
        NewGame();
    }

    void NewGame_Click(object sender, RoutedEventArgs e) => NewGame();

    void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F2) NewGame();
    }

    /* ---------- high scores ---------- */

    void OfferScore(long ms)
    {
        var data = ScoreStore.Load();
        var list = data.Scores.TryGetValue(_level.Key, out var existing) ? existing : new List<ScoreEntry>();
        if (list.Count >= ScoreStore.Max && ms >= list[^1].Ms) return;

        var dlg = new NameDialog($"{_level.Key}: {FmtMs(ms)}", data.LastName) { Owner = this };
        if (dlg.ShowDialog() != true) return;

        string name = dlg.EnteredName;
        if (name.Length == 0) name = "Anonymous";

        var entry = new ScoreEntry
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name,
            Ms = ms,
            Date = DateTime.Now
        };
        list.Add(entry);
        data.Scores[_level.Key] = list.OrderBy(s => s.Ms).ThenBy(s => s.Date).Take(ScoreStore.Max).ToList();
        data.LastName = name;

        if (!ScoreStore.Save(data))
        {
            MessageBox.Show(this, "Couldn't write the scores file:\n" + ScoreStore.FilePath,
                "Minesweeper", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        _lastNewId = entry.Id;
        RenderScores();
    }

    void RenderScores()
    {
        var data = ScoreStore.Load();
        var list = data.Scores.TryGetValue(_level.Key, out var l) ? l : new List<ScoreEntry>();

        ScoresTitle.Text = "Best times: " + _level.Key;
        ScoreList.ItemsSource = list
            .Select((s, i) => new ScoreRow
            {
                Rank = (i + 1).ToString(CultureInfo.InvariantCulture),
                Name = s.Name,
                Time = FmtMs(s.Ms),
                Date = s.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                IsNew = s.Id == _lastNewId
            })
            .ToList();
        EmptyText.Visibility = list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    void ClearScores_Click(object sender, RoutedEventArgs e)
    {
        var answer = MessageBox.Show(this, $"Clear all {_level.Key} times?", "Clear scores",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (answer != MessageBoxResult.Yes) return;

        var data = ScoreStore.Load();
        data.Scores.Remove(_level.Key);
        ScoreStore.Save(data);
        _lastNewId = null;
        RenderScores();
    }
}
