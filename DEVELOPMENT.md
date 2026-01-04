# Conspiracy Clicker - Development Guide

## Architecture Overview

```
ConspiracyClicker/
├── App.xaml                    # Application entry
├── MainWindow.xaml             # Main UI (XAML)
├── MainWindow.xaml.cs          # Main UI logic (C#)
├── Core/
│   ├── GameEngine.cs           # Core game mechanics
│   └── GameState.cs            # Serializable state
├── Data/
│   ├── GeneratorData.cs        # Generator definitions
│   ├── UpgradeData.cs          # Upgrade definitions
│   ├── ConspiracyData.cs       # Conspiracy definitions
│   ├── TinfoilShopData.cs      # Shop items
│   ├── QuestData.cs            # Quest definitions
│   ├── AchievementData.cs      # Achievements
│   ├── IlluminatiUpgradeData.cs # Prestige upgrades
│   ├── MatrixUpgradeData.cs    # Matrix system
│   └── IconData.cs             # Emoji fallbacks
├── Models/
│   ├── Generator.cs
│   ├── Upgrade.cs
│   ├── Conspiracy.cs
│   └── ...
├── Utils/
│   ├── IconHelper.cs           # SVG icon loading
│   └── NumberFormatter.cs      # Number display
└── Resources/
    └── Icons.xaml              # All vector icons (~3600 lines)
```

## UI Layout (MainWindow.xaml)

### Three-Column Grid
```
┌─────────────────┬─────────────────────────┬──────────────┐
│   Left Panel    │      Center Panel       │ Right Panel  │
│    (700px)      │         (*)             │   (300px)    │
│                 │                         │              │
│  Click Area     │   TabControl with:      │  Active      │
│  - Eye/Pyramid  │   - Generators          │  Effects     │
│  - Stats        │   - Upgrades            │              │
│  - Orbit        │   - Conspiracies        │  Statistics  │
│                 │   - Shop                │              │
│                 │   - Quests              │              │
│                 │   - Achievements        │              │
│                 │   - Prestige            │              │
│                 │   - Matrix              │              │
│                 │   - Settings            │              │
└─────────────────┴─────────────────────────┴──────────────┘
```

### The Illuminati Eye Component (lines ~250-320)
```
Grid (EyeContainer)
├── Ellipse (EyeAmbientGlow) - 200x200, pulsing glow
├── Image (PyramidImage) - 180x180, static pyramid
├── Image (EyeImage) - 50x50, animated eye with:
│   ├── DropShadowEffect (EyeGlow)
│   └── ScaleTransform (EyeScale)
└── Button (ClickButton) - 180x180, transparent click target
```

## Key Code Sections in MainWindow.xaml.cs

### Game Loop
- `_gameTimer` - 60fps timer for UI updates
- `UpdateUI()` - Main update method (~line 2280)
- `UpdateGeneratorPanel()`, `UpdateUpgradePanel()`, etc.

### Click Handling
- `ClickButton_Click()` - Handles clicks (~line 1900)
- `OnComboBurst()` - Combo pop animation (~line 1990)
- `AnimateEyeClick()` - Click feedback (~line 1940)

### Eye Animation (in UpdateUI, ~line 2356)
```csharp
// Idle pulse + combo scaling
double pulsePhase = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) % 3000 / 3000.0;
double pulseFactor = 1.0 + 0.04 * Math.Sin(pulsePhase * 2 * Math.PI);
double targetScale = (1.0 + 0.25 * comboProgress) * pulseFactor;
EyeScale.ScaleX = targetScale;
EyeScale.ScaleY = targetScale;
```

### Color Progression (combo)
```csharp
// Green (0%) -> Yellow (50%) -> Gold (100%)
if (comboProgress < 0.5) {
    // Green to Yellow
    t = comboProgress * 2;
    color = Color.FromRgb(0, (byte)(255 - 40*t), (byte)(65 - 65*t));
} else {
    // Yellow to Gold
    t = (comboProgress - 0.5) * 2;
    color = Color.FromRgb((byte)(255), (byte)(255 - 40*t), 0);
}
```

## Icon System (Resources/Icons.xaml)

### DrawingImage Structure
Icons are 64x64 coordinate space:
```xml
<DrawingImage x:Key="Icon_example">
    <DrawingImage.Drawing>
        <DrawingGroup>
            <!-- Shapes using 0-64 coordinates -->
            <GeometryDrawing Brush="#00ff41" Geometry="M32,32 A16,16 0 1,1 32.01,32"/>
            <GeometryDrawing Geometry="M10,10 L54,54">
                <GeometryDrawing.Pen>
                    <Pen Brush="#ffd700" Thickness="2"/>
                </GeometryDrawing.Pen>
            </GeometryDrawing>
        </DrawingGroup>
    </DrawingImage.Drawing>
</DrawingImage>
```

### Common Geometry Patterns
```
Circle:     M32,32 A16,16 0 1,1 32.01,32    (center 32,32, radius 16)
Rectangle:  M8,8 L56,8 L56,56 L8,56 Z
Triangle:   M32,8 L8,56 L56,56 Z
Line:       M10,10 L54,54
Arc:        M8,32 A24,12 0 1,1 56,32        (elliptical arc)
```

### Color Palette
- Primary Green: `#00ff41`
- Gold: `#ffd700`
- Purple: `#9933ff`
- Red: `#cc0000`
- Panel Background: `#16213e`, `#0f3460`
- Text Light: `#e0e0e0`
- Text Dim: `#888888`

## Game State (Core/GameState.cs)

Key properties:
```csharp
public double Evidence { get; set; }
public double TotalEvidenceEarned { get; set; }
public int Tinfoil { get; set; }
public int Believers { get; set; }
public long TotalClicks { get; set; }
public int ComboClicks { get; set; }
public Dictionary<string, int> Generators { get; set; }
public HashSet<string> PurchasedUpgrades { get; set; }
public HashSet<string> ProvenConspiracies { get; set; }
public HashSet<string> TinfoilShopPurchases { get; set; }
public HashSet<string> IlluminatiUpgrades { get; set; }
public int IlluminatiPoints { get; set; }
// ... more
```

## Adding New Content

### New Generator
1. `Data/GeneratorData.cs`:
```csharp
new Generator {
    Id = "my_generator",
    Name = "My Generator",
    Description = "Does something cool",
    BaseCost = 1000,
    BaseEps = 10,
    CostMultiplier = 1.15,
    UnlockEvidence = 500
}
```

2. `Resources/Icons.xaml`:
```xml
<DrawingImage x:Key="Icon_my_generator">...</DrawingImage>
<DrawingImage x:Key="Icon_orbit_my_gen">...</DrawingImage>
```

3. `Utils/IconHelper.cs`:
```csharp
// In GeneratorOrbitIcons dictionary
["my_generator"] = "orbit_my_gen"
```

### New Upgrade
1. `Data/UpgradeData.cs`:
```csharp
new Upgrade {
    Id = "my_upgrade",
    Name = "My Upgrade",
    Description = "+10% something",
    FlavorText = "A witty comment",
    Type = UpgradeType.GlobalBoost,
    Value = 1.10,
    EvidenceCost = 10000,
    RequiredEvidence = 5000
}
```

2. `Resources/Icons.xaml`:
```xml
<DrawingImage x:Key="Icon_my_upgrade">...</DrawingImage>
```

## Debugging Tips

1. **Build errors**: Check for duplicate icon keys in Icons.xaml
2. **Icons not showing**: Verify key matches exactly (case-sensitive)
3. **Animations not working**: Check if XAML storyboards override code values
4. **Game not saving**: Check GameState serialization

## Performance Notes

- UI updates at 60fps via DispatcherTimer
- Panel rebuilds are cached (only rebuild when count changes)
- Large icon file (~3600 lines) loads at startup

## Testing

```bash
# Build
dotnet build

# Run
dotnet run

# Clean build
dotnet clean && dotnet build
```
