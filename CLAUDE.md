# Claude Code Context for Conspiracy Clicker

This file provides context for Claude Code (or any AI assistant) to continue development on this project.

## Project Overview

**Conspiracy Clicker** is a WPF-based incremental/clicker game with a conspiracy theory theme. Players click to gather "evidence," unlock generators, prove conspiracies, and progress through various upgrade systems.

## Tech Stack

- **Framework**: .NET 8.0 Windows (WPF)
- **Language**: C# 12
- **UI**: XAML with code-behind
- **Icons**: Custom sprite sheet icons (PNG) + legacy vector SVG icons

## User Preferences

- **Icon Style**: Sprite sheet icons with black backgrounds (#000000), neon color scheme (green #00ff41, gold #ffd700, purple #9933ff)
- **No Emojis**: Avoid using emojis in code or UI unless explicitly requested
- **Minimal Changes**: Only make changes that are directly requested; avoid over-engineering
- **Sprite Sheets**: New icons should be generated as sprite sheets using AI image generation, then loaded via SpriteSheetLoader classes
- **Matrix Feature**: Currently disabled (UI hidden but code preserved for potential future use)
- **Rank Display**: Removed from UI (titles under EPS area)

## Project Structure

```
ConspiracyClicker/
├── App.xaml(.cs)           # Application entry point
├── MainWindow.xaml(.cs)    # Main game UI (~6000 lines code-behind)
├── Core/
│   ├── GameEngine.cs       # Core game logic, state management
│   ├── GameState.cs        # Serializable game state model
│   └── GameConstants.cs    # Game balance constants
├── Data/
│   ├── GeneratorData.cs    # Generator definitions (12 tiers, 50+ generators)
│   ├── UpgradeData.cs      # Upgrade definitions (~90 upgrades)
│   ├── ConspiracyData.cs   # Conspiracy definitions (25 conspiracies)
│   ├── TinfoilShopData.cs  # Shop item definitions (~50 items)
│   ├── QuestData.cs        # Quest definitions
│   ├── AchievementData.cs  # Achievement definitions
│   ├── IlluminatiUpgradeData.cs  # Prestige upgrade definitions
│   ├── MatrixData.cs       # Matrix upgrade definitions (DISABLED)
│   ├── DailyChallengeData.cs
│   ├── ChallengeModeData.cs
│   ├── RandomEventData.cs
│   └── IconData.cs         # Emoji fallback icons (legacy)
├── Models/
│   └── Upgrade.cs          # Upgrade model
├── Utils/
│   ├── IconHelper.cs       # Icon loading with fallback chain
│   ├── NumberFormatter.cs  # Large number formatting
│   ├── SoundManager.cs     # Audio playback
│   ├── FlavorText.cs       # Random flavor text
│   ├── SpriteSheetLoader.cs           # Generator icons (main)
│   ├── GeneratorSpriteSheetLoader2.cs # Final tier generators
│   ├── GeneratorSpriteSheetLoader3.cs # information_nexus, timeline_harvester
│   ├── ConspiracySpriteSheetLoader.cs
│   ├── ConspiracySpriteSheetLoader2.cs
│   ├── UpgradeSpriteSheetLoader.cs
│   ├── UpgradeSpriteSheetLoader2.cs
│   ├── TinfoilShopSpriteSheetLoader.cs
│   ├── TinfoilShopSpriteSheetLoader2.cs
│   ├── TinfoilShopSpriteSheetLoader3.cs  # Transcendent/omega tier
│   ├── QuestSpriteSheetLoader.cs
│   ├── EventSpriteSheetLoader.cs
│   ├── AchievementSpriteSheetLoader.cs
│   ├── AchievementSpriteSheetLoader2.cs
│   ├── ChallengeSpriteSheetLoader.cs
│   ├── DailyChallengeSpriteSheetLoader.cs
│   ├── MatrixSpriteSheetLoader.cs    # (DISABLED feature)
│   ├── IlluminatiSpriteSheetLoader.cs
│   └── UISpriteSheetLoader.cs
├── Resources/
│   ├── Icons.xaml          # Vector icons (legacy, ~3600 lines)
│   ├── GeneratorSpriteSheet.png
│   ├── GeneratorSpriteSheet2.png
│   ├── GeneratorSpriteSheet3.png
│   └── [other sprite sheets...]
└── Simulator/              # Balance testing simulator (excluded from main build)
```

## Icon System

Icons are loaded via a fallback chain in `IconHelper.CreateIconWithFallback()`:

1. SpriteSheetLoader (generators)
2. GeneratorSpriteSheetLoader2 (final tier)
3. GeneratorSpriteSheetLoader3 (info/timeline)
4. ConspiracySpriteSheetLoader
5. ConspiracySpriteSheetLoader2
6. UpgradeSpriteSheetLoader
7. UpgradeSpriteSheetLoader2
8. QuestSpriteSheetLoader
9. TinfoilShopSpriteSheetLoader (1, 2, 3)
10. EventSpriteSheetLoader
11. MatrixSpriteSheetLoader
12. IlluminatiSpriteSheetLoader
13. AchievementSpriteSheetLoader (1, 2)
14. ChallengeSpriteSheetLoader
15. DailyChallengeSpriteSheetLoader
16. UISpriteSheetLoader
17. Vector icons (Icons.xaml)
18. Fallback text

### Adding New Sprite Sheet Icons

1. Generate sprite sheet using AI (e.g., Gemini) with prompt specifying:
   - Grid dimensions (columns x rows)
   - Icon size (e.g., 276x274 pixels)
   - Black background (#000000)
   - Neon color scheme

2. Copy to `Resources/` folder

3. Create loader class (copy existing pattern):
```csharp
public static class MyNewSpriteSheetLoader
{
    private static readonly Dictionary<string, (int row, int col)> SpritePositions = new()
    {
        ["icon_id"] = (0, 0),
        // ...
    };
    // ... (see existing loaders for full implementation)
}
```

4. Add to `ConspiracyClicker.csproj`:
```xml
<Resource Include="Resources\MySpriteSheet.png" />
```

5. Add to `IconHelper.CreateIconWithFallback()` fallback chain

## Disabled Features (Dead Code)

The following features are currently disabled but code is preserved:

### Matrix Prestige System
- **Files**: `Data/MatrixData.cs`, `Utils/MatrixSpriteSheetLoader.cs`
- **UI**: Hidden stubs in `MainWindow.xaml` (lines 770-780)
- **Code-behind**: `UpdateMatrixPanel()`, `MatrixBreakButton_Click()`, Matrix-related methods in `GameEngine.cs`
- **State**: `GameState.MatrixUpgrades`, `GameState.GlitchTokens`, `GameState.TimesMatrixBroken`
- **To re-enable**: Restore the Matrix section XAML (see git history) and set visibility

### Rank Display
- **UI**: Hidden stubs `RankDisplayPanel`, `RankSocietyDisplay`, `RankTitleDisplay`
- **Code-behind**: Rank update logic around line 3136
- **To re-enable**: Remove `Visibility="Collapsed"` from `RankDisplayPanel`

## Main UI Structure

Three-column layout:
1. **Left Panel** (700px): Evidence/EPS display, Tinfoil/Tokens/Believers, Click area with Illuminati eye, Combo label
2. **Center Panel**: Tabbed interface (Generators, Upgrades, Tinfoil Shop, Conspiracies, Quests, Achievements, Illuminati, Stats, Settings)
3. **Right Panel** (300px): Active effects, notifications

### Key UI Elements
- `EvidenceDisplay` - Main evidence counter
- `EpsDisplay` - Evidence per second
- `PyramidImage` - Background pyramid (180x180)
- `EyeImage` - Clickable eye (50x50)
- `ClickButton` - Transparent click target
- `ComboLabel` - Combo counter
- `OrbitCanvas` - Generator icons orbiting the eye

## Game Systems

### Combo System
- Clicks within 0.5s add to combo
- Multiplier increases click power
- Eye scales 1.0 to 1.25 with combo
- Color transitions: green -> yellow -> gold
- "Bursts" with pop animation at max

### Generators (12 Tiers)
- Tier 1: Red String, Suspicious Neighbor, etc.
- Tier 2-4: Advanced conspiracy machines
- Tier 5+: Cosmic/dimensional generators
- Each produces evidence per second

### Conspiracies (25 total)
- Require evidence threshold + believers
- Unlock features and provide bonuses
- Pyramid evolves with proven conspiracies

### Prestige (Illuminati)
- Soft reset for Illuminati Tokens
- Permanent multipliers
- Based on total evidence earned

### Quests
- Send believers on missions
- Risk/reward system (Low/Medium/High)
- Cooldown between quests

## Build & Run

```bash
cd C:\Users\quent\ConspiracyClicker
dotnet build
dotnet run
```

## Recent Changes (January 2026)

1. **Sprite Sheet Icon System**: Replaced vector icons with AI-generated sprite sheets
2. **Matrix Feature Disabled**: UI hidden, code preserved as stubs
3. **Rank Display Removed**: Hidden from EPS area
4. **Icon Alignment Fix**: Added AlignmentX/AlignmentY to ImageBrush in CreateStyledIconBorder
5. **Matrix Stats Removed**: Removed from Statistics tab
6. **New Generator Icons**: Added GeneratorSpriteSheet3 for information_nexus and timeline_harvester

## Known Issues

- Some late-game balance may need tuning
- Could add more achievements
- Could expand quest system

## Development Tips

1. **Always read before editing**: Use Read tool before making changes
2. **Test builds frequently**: Run `dotnet build` after changes
3. **Kill running instance**: Close game before rebuilding
4. **Sprite sheet dimensions**: Calculate icon positions using (row * iconHeight, col * iconWidth)
5. **Icon padding**: Most sprite sheets use 12-15px padding from cell edges
