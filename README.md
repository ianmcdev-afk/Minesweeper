# Minesweeper

A Minesweeper game for Windows built with WPF and .NET 8. It has three difficulty levels, a persistent high score table, and switchable colour themes.

<!-- Add a screenshot: ![Minesweeper](docs/screenshot.png) -->

## Features

- Three difficulties: Beginner (9×9, 10 mines), Intermediate (16×16, 40 mines), Expert (30×16, 99 mines)
- First click is always safe
- Chording: click (or middle-click) a number to open its neighbours once enough flags are placed
- Top 10 best times per difficulty, saved between sessions
- Five themes: Minefield, Classic, Midnight, Ocean, Desert
- Custom app icon and an optional Windows installer

## Controls

| Action | Input |
| --- | --- |
| Open a square | Left-click |
| Place or remove a flag | Right-click |
| Open surrounding squares (chord) | Left-click or middle-click an open number |
| New game | **New game** button or `F2` |

## High scores and settings

Scores, the last name you entered, and your chosen theme are stored in:

```
%LocalAppData%\Minesweeper\scores.json
```

This lives outside the install folder, so scores survive moving, rebuilding, updating or uninstalling the app. To reset everything, delete the file. If the file can't be read it is renamed to `scores.json.bad` rather than overwritten.

## Requirements

- Windows 10 or 11 (x64)
- To build from source: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- To build the installer: [Inno Setup](https://jrsoftware.org/isinfo.php) 6.3 or later (`winget install JRSoftware.InnoSetup`)

## Building

### Single-file exe

```
build.bat
```

This runs `dotnet publish -c Release -o dist` and produces `dist\Minesweeper.exe`. The exe is self-contained, so it needs no .NET runtime installed. To get a much smaller exe that requires the .NET 8 Desktop Runtime instead, set `SelfContained` to `false` in `Minesweeper.csproj`.

### Installer

```
build-installer.bat
```

This builds the exe, then compiles `installer.iss` into `installer\Minesweeper-Setup.exe`. The installer:

- installs per-user by default (no admin prompt), with an all-users option
- adds a Start menu entry and an optional desktop shortcut
- registers an uninstaller under Settings > Apps > Installed apps, with publisher **kingtreelo inc**

### Run from source

```
dotnet run --project Minesweeper.csproj
```

## Project structure

| File | Purpose |
| --- | --- |
| `Game.cs` | Board logic: mine placement, flood-fill opening, chording, win and lose |
| `MainWindow.xaml` / `.xaml.cs` | UI, input handling, rendering, timer and score table |
| `Themes.cs` | Theme palettes. Add a new entry to `Themes.All` to create a theme |
| `ScoreStore.cs` | Reads and writes `scores.json` |
| `NameDialog.cs` | Name prompt shown for a new high score |
| `Minesweeper.ico` | App and window icon |
| `installer.iss` | Inno Setup installer script |
| `build.bat`, `build-installer.bat` | Build helpers |

## Customising

- **Number of saved scores:** change `Max` in `ScoreStore.cs`
- **Difficulty settings and tile sizes:** edit the `Levels` array in `MainWindow.xaml.cs`
- **Themes:** edit or add entries in `Themes.cs`
