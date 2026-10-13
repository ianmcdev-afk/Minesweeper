namespace Minesweeper;

/// <param name="Nums">Colours for the numbers 1-8 (index 0 is unused).</param>
public sealed record Theme(
    string Name,
    string Bg, string Panel, string Line, string Ink, string Muted, string Accent,
    string CoverA, string CoverB, string CoverEdge,
    string OpenA, string OpenB, string OpenEdge,
    string Danger, string WrongBg, string Flag, string MineInk,
    string Win, string Lose, string NewRow,
    string[] Nums);

public static class Themes
{
    public static readonly Theme[] All =
    {
        new("Minefield",
            "#1F261C", "#2A3326", "#3B4736", "#F0EAD8", "#A9B39C", "#E8B84A",
            "#6B9A45", "#7AAB50", "#8FBF68",
            "#DCCAA4", "#D1BD93", "#C2AE82",
            "#D9432F", "#B86A5A", "#A8200F", "#1A1A1A",
            "#9FE070", "#FF8F7D", "#4D3F1E",
            new[] { "#000000", "#1E4FA8", "#2F7A2A", "#C22B2B", "#4B2A99", "#8A2A1A", "#0E7C7C", "#222222", "#666666" }),

        new("Classic",
            "#C0C0C0", "#D9D9D9", "#808080", "#000000", "#404040", "#B00000",
            "#D0D0D0", "#D0D0D0", "#FFFFFF",
            "#B4B4B4", "#B4B4B4", "#8A8A8A",
            "#FF0000", "#E08080", "#C00000", "#000000",
            "#006400", "#B00000", "#FFF2B3",
            new[] { "#000000", "#0000FF", "#008000", "#FF0000", "#000080", "#800000", "#008080", "#000000", "#808080" }),

        new("Midnight",
            "#12141C", "#1C202B", "#2D3345", "#E6E8F0", "#8A91A8", "#7AA2F7",
            "#3B4261", "#434B6E", "#5A6490",
            "#1A1D29", "#202434", "#2A2F42",
            "#E5484D", "#6B3A44", "#FF9E64", "#E6E8F0",
            "#9ECE6A", "#F7768E", "#2A3350",
            new[] { "#000000", "#7AA2F7", "#9ECE6A", "#F7768E", "#BB9AF7", "#FF9E64", "#7DCFFF", "#E0E0E0", "#9AA5CE" }),

        new("Ocean",
            "#0B2233", "#12364D", "#1F5272", "#EAF6FB", "#8FB6C9", "#4FD1C5",
            "#2F7FA3", "#3A8DB2", "#5FA9C9",
            "#CFE8F1", "#C2E0EB", "#A7CBD9",
            "#E4572E", "#D9A199", "#F2C94C", "#10212B",
            "#7EE8B0", "#FF9B85", "#1B5A63",
            new[] { "#000000", "#0B4F9E", "#1B7A3A", "#C0392B", "#4A2C8F", "#8A2A1A", "#00767A", "#1B1B1B", "#555555" }),

        new("Desert",
            "#2B2118", "#3A2D21", "#55412F", "#F5E9D3", "#BBA78B", "#E07A3C",
            "#C9A15F", "#D2AB68", "#E3C089",
            "#F1E3C4", "#E9D8B4", "#D6C29A",
            "#C0392B", "#D79A82", "#7A1F12", "#2B2118",
            "#A8D67A", "#FF9E85", "#5A4128",
            new[] { "#000000", "#1F4E9C", "#2F7A2A", "#B5281C", "#4B2A99", "#7A2E12", "#0E7C7C", "#222222", "#666666" })
    };
}
