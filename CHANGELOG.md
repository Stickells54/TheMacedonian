# Changelog

All notable changes to The Macedonian mod will be documented in this file.

## [0.2.0] - 2025-12-11 - Full Implementation Release

### Added

#### Game Menus
- **Intrigue Menu** - New "Pursue dark intrigues..." option in town and castle menus
- **Assassination Planning** - Select targets and methods from settlement menus
- **Method Selection Menu** - Choose between Poison, Hunting Accident, or Midnight Dagger
- **Confirmation Menu** - Review success/detection chances before committing
- **Framing Option** - Attempt to frame enemy kingdoms for assassinations
- **Conspiracy Status** - View conspiracy strength, leak risk, and recruited clans
- **Suspicion Status** - Check ruler, court, and kingdom suspicion levels
- **Rival Identification** - Identify your greatest political rival

#### Assassination Execution System
- **Full Assassination Mechanic** - Complete execution flow with success/failure handling
- **Success Handling** - Target eliminated via `KillCharacterAction.ApplyByMurder()`
- **Failure Handling** - Consequences including relation damage and suspicion increase
- **Detection System** - Post-assassination detection rolls with method-specific chances
- **Framing System** - Attempt to redirect blame with `CalculateFrameSuccessChance()`
- **Cooldown Tracking** - Per-target cooldowns prevent repeated attempts
- **Target Awareness** - Failed attempts increase target's paranoia

#### Post-Coronation Features
- **Host Feast** - Spend 10,000 denars to boost legitimacy and relations (ruler only)
- **Make Donations** - Spend 5,000 denars to boost legitimacy (ruler only)
- **Victory/Defeat Tracking** - Weekly battle outcomes affect legitimacy
- **Counter-Coup Risk** - Low legitimacy can trigger faction plots against you
- **Legitimacy State Display** - Shows current legitimacy status and warnings

#### Event Handlers
- **HeroKilledEvent** - Tracks assassination plot completions and ruler deaths
- **RulingClanChanged** - Detects when player becomes ruler
- **Ruler Status Check** - Daily validation of ruler status

#### Status Display
- `ShowConspiracyStatus()` - Display conspirators count, coup strength, leak risk
- `ShowSuspicionStatus()` - Display all three suspicion levels with color coding
- `IdentifyAndShowRival()` - Show top rival's name, clan, rivalry score, and ruler favor

### Changed
- **MacedonianBehavior.cs** expanded from ~340 lines to ~1590 lines
- Legitimacy system now uses weekly calculations from `UsurpationModel`
- Game menus use simplified `AddGameMenu()` overload for API compatibility

### Technical
- Added `HeroKilledEvent` and `RulingClanChanged` event listeners
- Added weekly tracking fields for victories, defeats, feasts, donations
- Added `OnBattleVictory()` and `OnBattleDefeat()` public methods
- Added `CheckForCounterCoup()` for low legitimacy consequences

---

## [0.1.0] - 2025-01-22 - Initial Foundation Release

### Added

#### Core Framework
- **SubModule.xml** - Mod definition for Bannerlord loader with proper dependencies
- **TheMacedonian.csproj** - Project configuration with all TaleWorlds references and MCM integration
- **SubModule.cs** - Entry point with Harmony initialization and behavior registration

#### Data Models (`src/Models/`)
- **IntrigueModels.cs** - Core data structures for the intrigue system:
  - `AssassinationMethod` enum (Poison, HuntingAccident, DuelProvocation, MidnightDagger, PraetorianCoup)
  - `PlotPhase` enum (Planning, Preparation, Execution, Completed, Abandoned)
  - `ConspiracyResult` enum (Pending, Success, Failure, Exposed, Abandoned)
  - `LeakSeverity` enum (Rumor, Suspicion, Evidence, FullExposure)
  - `FrameTarget` enum (NoFrame, RivalClan, EnemyKingdom, Bandits, etc.)
  - `SuspicionLevel` enum (None, Low, Medium, High, Critical)
  - `ConspiracyStatus` enum (Unknown, Loyalist, Neutral, Sympathizer, Conspirator)
  - `ClanIntrigueData` class - Per-clan conspiracy tracking
  - `SuspicionData` class - Multi-level suspicion system
  - `AssassinationCooldown` class - Cooldown tracking per target
  - `PlotRecord` class - Active plot tracking with save/load support

- **UsurpationModel.cs** - Probability calculation engine:
  - Assassination chance calculations for all 4 methods
  - Detection chance calculations based on method and outcome
  - Suspicion increase calculations with alibi support
  - Conspiracy join chance calculations based on traits and relations
  - Leak risk and severity calculations
  - Buying silence price calculations
  - Silence reliability calculations
  - Rivalry score calculations
  - Legitimacy calculations (initial and weekly changes)
  - Frame success chance calculations
  - Final usurpation success calculations

- **BannerlordHelpers.cs** - Version-safe API wrappers:
  - Trait access helpers (Honor, Valor, Mercy, Calculating, Generosity)
  - Clan strength calculations
  - Math clamp helpers
  - Distance calculations
  - War state checks
  - `HeroTraits` container class
  - `GetHeroTraits()` extension method

#### Core Behavior (`src/Behaviors/`)
- **MacedonianBehavior.cs** - Main campaign behavior:
  - Save/load support for all intrigue state
  - Coup Strength calculation (% of kingdom military power in conspiracy)
  - Leak Risk tracking
  - Daily suspicion decay
  - Weekly leak checks and handling
  - Weekly rivalry score updates
  - Clan data management with leak risk modifiers
  - Rivalry score calculations
  - Top rival identification
  - Conspiracy dialog system for recruiting lords
  - Right Hand progression tracking (battles with ruler)
  - Hostage companion tracking
  - Legitimacy management for post-coronation phase
  - Event handlers for ruler death, battle participation, hostage events

#### Harmony Patches (`src/Patches/`)
- **IntriguePatches.cs** - Game event interception:
  - Hero murder event tracking
  - Natural death event tracking
  - Relation change monitoring for conspiracy stability

#### MCM Settings (`src/Settings/`)
- **MCMSettings.cs** - Full configuration system:
  - **General**: Debug logging, intrigue system toggle, UI panel toggle
  - **Difficulty**: Assassination success modifier, detection modifier, leak risk modifier, legitimacy decay modifier, assassination cooldown days
  - **Right Hand System**: Enable toggle, battles required for trust, hostage duration
  - **Assassination Methods**: Individual toggles for Poison, Hunting Accident, Duel, Midnight Dagger, and Framing
  - **Conspiracy**: Minimum conspirators for coup, coup strength threshold, silence price base, lord refusal toggle
  - **Post-Coronation**: Enable toggle, initial legitimacy values, counter-coup toggle
  - **Notifications**: Suspicion warnings, leak warnings, rivalry updates, legitimacy updates

### Technical Details
- Target Framework: .NET Framework 4.7.2
- Language Version: C# 9.0 with nullable reference types
- Dependencies: Lib.Harmony 2.2.2, Bannerlord.MCM 5.x
- Build Output: `bin\Win64_Shipping_Client\TheMacedonian.dll`

### Design Document
- Complete design specification in `DESIGN_DOC.md` covering:
  - Right Hand System (Royal Protector path)
  - Political Dominance (Influence mechanics)
  - Assassination System (4 methods with detailed mechanics)
  - Conspiracy System (Coup Strength, Leak Risk)
  - Legitimacy & Suspicion systems
  - Rival identification
  - Framing mechanics
  - Post-Coronation phase

### Known Limitations
This is a foundation release establishing the core framework. The following features are defined in the design doc but require additional implementation:

1. ~~**Game Menus** - Intrigue action menus in settlements not yet implemented~~ ✅ Implemented in v0.2.0
2. **Full Dialog Trees** - Basic conspiracy recruitment exists; advanced assassination dialogs pending
3. **UI Panel** - Intrigue status panel design pending (information shown via messages currently)
4. **Right Hand Quest Chain** - Companion hostage mechanics need quest integration
5. ~~**Assassination Execution** - Method-specific success/failure handling pending~~ ✅ Implemented in v0.2.0
6. ~~**Post-Coronation Events** - Feast, donation, and legitimacy event triggers pending~~ ✅ Implemented in v0.2.0

### Next Steps
- Add detailed dialog trees for each assassination method
- Create intrigue status UI panel (Gauntlet UI)
- Implement Right Hand appointment quest
- Add battle participation tracking for ruler army battles
- Testing and balance passes
