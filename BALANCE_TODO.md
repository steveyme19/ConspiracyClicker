# Balance Optimization TODO

## Current State (January 2026)

### Targets
- **Without ascensions**: ~50 hours to complete all 25 conspiracies
- **With optimal ascensions**: ~5 hours to complete all 25 conspiracies
- **First ascension**: ~2 hours into gameplay

### Actual Results
| Metric | Target | Actual | Status |
|--------|--------|--------|--------|
| No-ascension completion | 50h | ~50h | ✅ Hit |
| With-ascension completion | 5h | ~15-18h | ❌ Too slow |
| First ascension timing | 2h | ~1h | ✅ Close |

## Problem

Ascensions currently provide ~3x speedup instead of the target 10x speedup.

- Current: 50h / 15h = 3.3x speedup
- Target: 50h / 5h = 10x speedup

## Potential Solutions

### Option 1: Boost Illuminati Multipliers
Increase the EPS multipliers on Illuminati upgrades:
- Current tier 1: x10, x10, x5 = 500x total
- Needed: ~3x more powerful = x30, x30, x15 or similar

### Option 2: Reduce Token Costs
Lower the token costs for key upgrades so players can buy more per ascension:
- Pyramid Scheme: 1 → 1 (keep)
- Reptilian DNA: 2 → 1
- Deep State Connections: 3 → 2

### Option 3: Increase Token Generation
Adjust the logarithmic formula to give more tokens per ascension:
- Current: `TOKEN_LOG_BASE = 3.0` (every 3x evidence = +1 token)
- Option: `TOKEN_LOG_BASE = 2.0` (every 2x evidence = +1 token)

### Option 4: Reduce Late-Game Conspiracy Costs
The endgame conspiracies (20-25) take too long even with multipliers. Consider:
- Reducing costs by 10-100x for conspiracies 20+
- Or adding more powerful late-tier Illuminati upgrades

## Files to Modify

- `Data/PrestigeData.cs` - Token formula, Illuminati upgrade values
- `Data/ConspiracyData.cs` - Conspiracy costs (if reducing)
- `Core/GameConstants.cs` - Any related constants

## Testing

Run simulations to verify:
```bash
cd Simulator
dotnet run -- --ascension optimal --interval 15
dotnet run -- --ascension none --interval 30
```

Target metrics after optimization:
- First ascension: 1-2 hours
- With ascensions completion: ~5 hours
- Without ascensions completion: ~50 hours (maintain current)
